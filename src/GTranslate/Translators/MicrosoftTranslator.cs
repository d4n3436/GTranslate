using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using GTranslate.Extensions;
using GTranslate.Results;

namespace GTranslate.Translators;

/// <summary>
/// Represents the Microsoft Azure translator.
/// </summary>
public sealed class MicrosoftTranslator : ITranslator, IDisposable
{
    private const string _apiEndpoint = "dev.microsofttranslator-int.com";
    private const string _apiVersion = "3.0";
    private const string _detectUrl = $"{_apiEndpoint}/detect?api-version={_apiVersion}";
    private const string _speechTokenUrl = "dev.microsofttranslator.com/apps/endpoint?api-version=1.0";
    // test base domain: dev-sn2-test1.microsofttranslator-int.com
    // end point co4: https://dev-co4-test1.microsofttranslator-int.com/ // works
    // end point int: https://dev.microsofttranslator-int.com/ // works

    private static readonly Uri _detectUri = new($"https://{_detectUrl}");
    private static readonly Uri _speechTokenUri = new($"https://{_speechTokenUrl}");
    internal static readonly HtmlEncoder _ssmlEncoder = HtmlEncoder.Create(UnicodeRanges.All); // Like the default encoder but only encodes required characters

    // From Microsoft Translator Android app
    private static readonly byte[] _privateKey =
    {
        0xa2, 0x29, 0x3a, 0x3d, 0xd0, 0xdd, 0x32, 0x73,
        0x97, 0x7a, 0x64, 0xdb, 0xc2, 0xf3, 0x27, 0xf5,
        0xd7, 0xbf, 0x87, 0xd9, 0x45, 0x9d, 0xf0, 0x5a,
        0x09, 0x66, 0xc6, 0x30, 0xc6, 0x6a, 0xaa, 0x84,
        0x9a, 0x41, 0xaa, 0x94, 0x3a, 0xa8, 0xd5, 0x1a,
        0x6e, 0x4d, 0xaa, 0xc9, 0xa3, 0x70, 0x12, 0x35,
        0xc7, 0xeb, 0x12, 0xf6, 0xe8, 0x23, 0x07, 0x9e,
        0x47, 0x10, 0x95, 0x91, 0x88, 0x55, 0xd8, 0x17
    };

    /// <inheritdoc/>
    public string Name => nameof(MicrosoftTranslator);

    private readonly HttpClient _httpClient;
    private CachedObject<MicrosoftAuthTokenInfo> _cachedAuthTokenInfo;
    private readonly SemaphoreSlim _voicesSemaphore = new(1, 1);
    private readonly SemaphoreSlim _authTokenInfoSemaphore = new(1, 1);
    private MicrosoftVoice[] _voices = Array.Empty<MicrosoftVoice>();
    private bool _disposed;
    
    /// <summary>
    /// Gets a read-only dictionary containing the hardcoded TTS voices used internally in Bing Translator.
    /// </summary>
    /// <remarks>
    /// This dictionary is incomplete; it only includes 1 voice per language despite the API offering multiple voices.<br/>
    /// To get the complete list, use <see cref="GetTTSVoicesAsync"/>.
    /// </remarks>
    public static IReadOnlyDictionary<string, MicrosoftVoice> DefaultVoices => new ReadOnlyDictionary<string, MicrosoftVoice>(new Dictionary<string, MicrosoftVoice>
    {
        ["ar"] = new("Hamed", "ar-SA-HamedNeural", "Male", "ar-SA"),
        ["bg"] = new("Borislav", "bg-BG-BorislavNeural", "Male", "bg-BG"),
        ["ca"] = new("Joana", "ca-ES-JoanaNeural", "Female", "ca-ES"),
        ["cs"] = new("Antonin", "cs-CZ-AntoninNeural", "Male", "cs-CZ"),
        ["da"] = new("Christel", "da-DK-ChristelNeural", "Female", "da-DK"),
        ["de"] = new("Katja", "de-DE-KatjaNeural", "Female", "de-DE"),
        ["el"] = new("Nestoras", "el-GR-NestorasNeural", "Male", "el-GR"),
        ["en"] = new("Aria", "en-US-AriaNeural", "Female", "en-US"),
        ["es"] = new("Elvira", "es-ES-ElviraNeural", "Female", "es-ES"),
        ["fi"] = new("Noora", "fi-FI-NooraNeural", "Female", "fi-FI"),
        ["fr"] = new("Denise", "fr-FR-DeniseNeural", "Female", "fr-FR"),
        ["fr-CA"] = new("Sylvie", "fr-CA-SylvieNeural", "Female", "fr-CA"),
        ["he"] = new("Avri", "he-IL-AvriNeural", "Male", "he-IL"),
        ["hi"] = new("Swara", "hi-IN-SwaraNeural", "Female", "hi-IN"),
        ["hr"] = new("Srecko", "hr-HR-SreckoNeural", "Male", "hr-HR"),
        ["hu"] = new("Tamas", "hu-HU-TamasNeural", "Male", "hu-HU"),
        ["id"] = new("Ardi", "id-ID-ArdiNeural", "Male", "id-ID"),
        ["it"] = new("Diego", "it-IT-DiegoNeural", "Male", "it-IT"),
        ["ja"] = new("Nanami", "ja-JP-NanamiNeural", "Female", "ja-JP"),
        ["ko"] = new("SunHi", "ko-KR-SunHiNeural", "Female", "ko-KR"),
        ["ms"] = new("Osman", "ms-MY-OsmanNeural", "Male", "ms-MY"),
        ["nl"] = new("Colette", "nl-NL-ColetteNeural", "Female", "nl-NL"),
        ["no"] = new("Pernille", "nb-NO-PernilleNeural", "Female", "nb-NO"), // nb
        ["pl"] = new("Zofia", "pl-PL-ZofiaNeural", "Female", "pl-PL"),
        ["pt"] = new("Francisca", "pt-BR-FranciscaNeural", "Female", "pt-BR"),
        ["pt-PT"] = new("Fernanda", "pt-PT-FernandaNeural", "Female", "pt-PT"),
        ["ro"] = new("Emil", "ro-RO-EmilNeural", "Male", "ro-RO"),
        ["ru"] = new("Dariya", "ru-RU-DariyaNeural", "Female", "ru-RU"),
        ["sk"] = new("Lukas", "sk-SK-LukasNeural", "Male", "sk-SK"),
        ["sl"] = new("Rok", "sl-SI-RokNeural", "Male", "sl-SI"),
        ["sv"] = new("Sofie", "sv-SE-SofieNeural", "Female", "sv-SE"),
        ["ta"] = new("Pallavi", "ta-IN-PallaviNeural", "Female", "ta-IN"),
        ["te"] = new("Shruti", "te-IN-ShrutiNeural", "Male", "te-IN"),
        ["th"] = new("Niwat", "th-TH-NiwatNeural", "Male", "th-TH"),
        ["tr"] = new("Emel", "tr-TR-EmelNeural", "Female", "tr-TR"),
        ["vi"] = new("NamMinh", "vi-VN-NamMinhNeural", "Male", "vi-VN"),
        ["zh-CN"] = new("Xiaoxiao", "zh-CN-XiaoxiaoNeural", "Female", "zh-CN"), // zh-Hans
        ["zh-TW"] = new("Xiaoxiao", "zh-CN-XiaoxiaoNeural", "Female", "zh-CN"), // zh-Hant
        ["yue"] = new("HiuGaai", "zh-HK-HiuGaaiNeural", "Female", "zh-HK")
    });

    /// <summary>
    /// Gets a read-only dictionary containing the languages that support transliteration and their supported scripts.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> Scripts { get; }
        = new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(new Dictionary<string, IReadOnlyCollection<string>>
    {
        ["ar"] = new[] { "Latn", "Arab" },
        ["as"] = new[] { "Latn", "Beng" },
        ["be"] = new[] { "Latn", "Cyrl" },
        ["bg"] = new[] { "Latn", "Cyrl" },
        ["bn"] = new[] { "Latn", "Beng" },
        ["el"] = new[] { "Latn", "Grek" },
        ["fa"] = new[] { "Latn", "Arab" },
        ["gu"] = new[] { "Latn", "Gujr" },
        ["he"] = new[] { "Latn", "Hebr" },
        ["hi"] = new[] { "Latn", "Deva" },
        ["ja"] = new[] { "Latn", "Jpan" },
        ["kk"] = new[] { "Latn", "Cyrl" },
        ["kn"] = new[] { "Latn", "Knda" },
        ["ko"] = new[] { "Latn", "Kore" },
        ["ky"] = new[] { "Latn", "Cyrl" },
        ["mk"] = new[] { "Latn", "Cyrl" },
        ["ml"] = new[] { "Latn", "Mlym" },
        ["mn"] = new[] { "Latn", "Cyrl" },
        ["mr"] = new[] { "Latn", "Deva" },
        ["or"] = new[] { "Latn", "Orya" },
        ["pa"] = new[] { "Latn", "Guru" },
        ["ru"] = new[] { "Latn", "Cyrl" },
        ["sd"] = new[] { "Latn", "Arab" },
        ["si"] = new[] { "Latn", "Sinh" },
        ["ta"] = new[] { "Latn", "Taml" },
        ["te"] = new[] { "Latn", "Telu" },
        ["tg"] = new[] { "Latn", "Cyrl" },
        ["tt"] = new[] { "Latn", "Cyrl" },
        ["uk"] = new[] { "Latn", "Cyrl" },
        ["ur"] = new[] { "Latn", "Arab" },
        ["zh-CN"] = new[] { "Latn", "Hans" }, // zh-Hans
        ["zh-TW"] = new[] { "Latn", "Hant" } // zh-Hant
    });

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrosoftTranslator"/> class.
    /// </summary>
    public MicrosoftTranslator()
        : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrosoftTranslator"/> class with the provided <see cref="HttpClient"/> instance.
    /// </summary>
    /// <param name="httpClient">An <see cref="HttpClient"/> instance.</param>
    public MicrosoftTranslator(HttpClient httpClient)
    {
        TranslatorGuards.NotNull(httpClient);

        if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.DefaultUserAgent);
        }

        _httpClient = httpClient;
    }

    /// <summary>
    /// Translates a text using Microsoft Translator.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <param name="toLanguage">The target language.</param>
    /// <param name="fromLanguage">The source language.</param>
    /// <returns>A task that represents the asynchronous translation operation. The task contains the translation result.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="toLanguage"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="Language"/> could not be obtained from <paramref name="toLanguage"/> or <paramref name="fromLanguage"/>.</exception>
    /// <exception cref="TranslatorException">
    /// Thrown when <paramref name="toLanguage"/> or <paramref name="fromLanguage"/> are not supported, or an error occurred during the operation.
    /// </exception>
    public async Task<MicrosoftTranslationResult> TranslateAsync(string text, string toLanguage, string? fromLanguage = null)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        TranslatorGuards.LanguageFound(toLanguage, out var toLang, "Unknown target language.");
        TranslatorGuards.LanguageFound(fromLanguage, out var fromLang, "Unknown source language.");
        TranslatorGuards.LanguageSupported(this, toLang, fromLang);

        return await TranslateAsync(text, toLang, fromLang).ConfigureAwait(false);
    }

    /// <inheritdoc cref="TranslateAsync(string, string, string)"/>
    public async Task<MicrosoftTranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

        string url = $"{_apiEndpoint}/translate?api-version={_apiVersion}&to={MicrosoftHotPatch(toLanguage.ISO6391)}";
        if (fromLanguage is not null)
        {
            url += $"&from={MicrosoftHotPatch(fromLanguage.ISO6391)}";
        }

        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"https://{url}")
        };

        request.Headers.Add("X-MT-Signature", GetSignature(url));
        request.Content = new StringContent($"[{{\"Text\":\"{text.AsSpan().SafeJsonTextEncode()}\"}}]", Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

        var root = document.RootElement[0];

        var detectedLanguage = root.GetPropertyOrDefault("detectedLanguage"u8);
        string sourceLanguage = detectedLanguage
            .GetPropertyOrDefault("language"u8)
            .GetStringOrDefault(fromLanguage?.ISO6391) ?? throw new TranslatorException("Failed to get the source language.", Name);

        string targetLanguage = root.GetProperty("translations"u8)[0].GetProperty("to"u8).GetString() ?? toLanguage.ISO6391;

        float? score = detectedLanguage.TryGetSingle("score"u8, out float temp) ? temp : null;
        string translation = root.GetProperty("translations"u8)[0].GetProperty("text"u8).GetString() ?? throw new TranslatorException("Failed to get the translation.", Name);

        return new MicrosoftTranslationResult(translation, text, Language.GetLanguage(targetLanguage), Language.GetLanguage(sourceLanguage), score);
    }

    /// <summary>
    /// Transliterates a text using Microsoft Translator.
    /// </summary>
    /// <param name="text">The text to transliterate.</param>
    /// <param name="language">The language of the text.</param>
    /// <param name="fromScript">The source script.</param>
    /// <param name="toScript">The target script.</param>
    /// <returns>A task that represents the asynchronous transliteration operation. The task contains the transliteration result.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when a parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="Language"/> could not be obtained from <paramref name="language"/>.</exception>
    /// <exception cref="TranslatorException">Thrown when a parameter is not supported, or an error occurred during the operation.</exception>
    public async Task<MicrosoftTransliterationResult> TransliterateAsync(string text, string language, string fromScript, string toScript)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(language);
        TranslatorGuards.NotNull(fromScript);
        TranslatorGuards.NotNull(toScript);
        TranslatorGuards.LanguageFound(language, out var lang);
        TranslatorGuards.LanguageSupported(this, lang);

        return await TransliterateAsync(text, lang, fromScript, toScript).ConfigureAwait(false);
    }

    /// <inheritdoc cref="TransliterateAsync(string, string, string, string)"/>
    public async Task<MicrosoftTransliterationResult> TransliterateAsync(string text, ILanguage language, string fromScript, string toScript)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(language);
        TranslatorGuards.NotNull(fromScript);
        TranslatorGuards.NotNull(toScript);
        EnsureValidScripts(language.ISO6391, fromScript, toScript);

        string url = $"{_apiEndpoint}/transliterate?api-version={_apiVersion}&language={MicrosoftHotPatch(language.ISO6391)}&fromScript={fromScript}&toScript={toScript}";
        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"https://{url}")
        };

        request.Headers.Add("X-MT-Signature", GetSignature(url));
        request.Content = new StringContent($"[{{\"Text\":\"{text.AsSpan().SafeJsonTextEncode()}\"}}]", Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

        var root = document.RootElement[0];
        string transliteration = root.GetProperty("text"u8).GetString() ?? throw new TranslatorException("Failed to get the transliteration.", Name);
        string script = root.GetProperty("script"u8).GetString() ?? throw new TranslatorException("Failed to get the output script", Name);

        return new MicrosoftTransliterationResult(transliteration, text, script);
    }

    /// <summary>
    /// Detects the language of a text using Microsoft Translator.
    /// </summary>
    /// <param name="text">The text to detect its language.</param>
    /// <returns>A task that represents the asynchronous language detection operation. The task contains the detected language.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <exception cref="TranslatorException">Thrown when an error occurred during the operation.</exception>
    public async Task<Language> DetectLanguageAsync(string text)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);

        
        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = _detectUri
        };

        request.Headers.Add("X-MT-Signature", GetSignature(_detectUrl));
        request.Content = new StringContent($"[{{\"Text\":\"{text.AsSpan().SafeJsonTextEncode()}\"}}]", Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

        string language = document.RootElement[0].GetProperty("language"u8).GetString() ?? throw new TranslatorException("Failed to get the detected language.", Name);

        return Language.GetLanguage(language);
    }

    /// <summary>
    /// Converts text into synthesized speech.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="language">The language of the voice. Only the languages in <see cref="DefaultVoices"/> are supported.</param>
    /// <param name="speakRate">The speaking rate of the text, expressed as a number that acts as a multiplier of the default.</param>
    /// <returns>A task that represents the asynchronous synthesis operation. The task contains the synthesized speech in a MP3 <see cref="Stream"/>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="language"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="MicrosoftVoice"/> could not be obtained from <paramref name="language"/>.</exception>
    /// <exception cref="TranslatorException">Thrown when <paramref name="language"/> is not supported, or an error occurred during the operation.</exception>
    public async Task<Stream> TextToSpeechAsync(string text, string language, float speakRate = 1)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(language);
        EnsureValidTTSLanguage(language, out var voice);

        return await TextToSpeechAsync(text, voice, speakRate).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts text into synthesized speech.
    /// </summary>
    /// <remarks>No validation will be performed to the <paramref name="voice"/> parameter. Make sure to get the correct voices from either <see cref="DefaultVoices"/> or <see cref="GetTTSVoicesAsync"/>.</remarks>
    /// <param name="text">The text to convert.</param>
    /// <param name="voice">The voice.</param>
    /// <param name="speakRate">The speaking rate of the text, expressed as a number that acts as a multiplier of the default.</param>
    /// <returns>A task that represents the asynchronous synthesis operation. The task contains the synthesized speech in a MP3 <see cref="Stream"/>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="voice"/> are null.</exception>
    /// <exception cref="TranslatorException">Thrown when an error occurred during the operation.</exception>
    public async Task<Stream> TextToSpeechAsync(string text, MicrosoftVoice voice, float speakRate = 1)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(voice);
        
        var authInfo = await GetOrUpdateMicrosoftAuthTokenAsync().ConfigureAwait(false);

        string payload = $"<speak version='1.0' xml:lang='{voice.Locale}'><voice xml:lang='{voice.Locale}' xml:gender='{voice.Gender}' name='{voice.ShortName}'><prosody rate='{speakRate}'>{_ssmlEncoder.Encode(text)}</prosody></voice></speak>";

        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"https://{authInfo.Region}.tts.speech.microsoft.com/cognitiveservices/v1"),
            Content = new StringContent(payload, Encoding.UTF8, "application/ssml+xml")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authInfo.Token);
        request.Headers.Add("X-Microsoft-OutputFormat", "audio-16khz-32kbitrate-mono-mp3");

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }

    // https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/language-support#text-to-speech
    // https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/rest-text-to-speech#get-a-list-of-voices

    /// <summary>
    /// Gets a list of supported TTS voices and caches it.
    /// </summary>
    /// <returns>A <see cref="ValueTask{TResult}"/> containing the list of voices.</returns>
    public async ValueTask<IReadOnlyCollection<MicrosoftVoice>> GetTTSVoicesAsync()
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);

        if (_voices.Length != 0)
        {
            return _voices;
        }

        await _voicesSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_voices.Length != 0)
            {
                return _voices;
            }

            var authInfo = await GetOrUpdateMicrosoftAuthTokenAsync().ConfigureAwait(false);

            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://{authInfo.Region}.tts.speech.microsoft.com/cognitiveservices/voices/list"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authInfo.Token);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            _voices = await JsonSerializer.DeserializeAsync(stream, MicrosoftVoiceContext.Default.MicrosoftVoiceArray).ConfigureAwait(false) ?? throw new TranslatorException("Failed to deserialize voice list.", Name);
        }
        finally
        {
            _voicesSemaphore.Release();
        }

        return _voices;
    }

    /// <summary>
    /// Gets or updates the Microsoft Azure Authentication Token.
    /// </summary>
    /// <remarks>
    /// This token can be used in the following services:<br/>
    /// - <see href="https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/">Speech Services</see>
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation. The task contains the token.</returns>
    /// <exception cref="TranslatorException">Thrown when the token could not be obtained from the response.</exception>
    public async ValueTask<MicrosoftAuthTokenInfo> GetOrUpdateMicrosoftAuthTokenAsync()
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);

        if (!_cachedAuthTokenInfo.IsExpired())
        {
            return _cachedAuthTokenInfo.Value;
        }

        await _authTokenInfoSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (!_cachedAuthTokenInfo.IsExpired())
            {
                return _cachedAuthTokenInfo.Value;
            }

            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = _speechTokenUri
            };


            request.Headers.Add("X-ClientVersion", "N/A");
            request.Headers.Add("X-MT-Signature", GetSignature(_speechTokenUrl));
            request.Headers.Add("X-UserId", "0");

            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
            var root = document.RootElement;

            // Tokens are valid for 30 minutes. The exp. date might be closer
            // if another API request is made while the token is still valid.

            // https://docs.microsoft.com/en-us/azure/cognitive-services/authentication?tabs=powershell#authenticate-with-an-authentication-token
            var tokenProp = root.GetProperty("t"u8);
            if (!TryGetExpirationDate(tokenProp, out var expirationDate))
            {
                throw new TranslatorException("Unable to obtain the expiration date from the auth token.", Name);
            }

            string token = tokenProp.GetString() ?? throw new TranslatorException("Unable to get the Microsoft Azure Auth token.", Name);
            string region = root.GetProperty("r"u8).GetString() ?? throw new TranslatorException("Unable to get the Microsoft Azure API region.", Name);

            var authInfo = new MicrosoftAuthTokenInfo(token, region);

            _cachedAuthTokenInfo = new CachedObject<MicrosoftAuthTokenInfo>(authInfo, expirationDate);
        }
        finally
        {
            _authTokenInfoSemaphore.Release();
        }

        return _cachedAuthTokenInfo.Value;
    }

    /// <summary>
    /// Returns whether Microsoft Translator supports the specified language.
    /// </summary>
    /// <param name="language">The language.</param>
    /// <returns><see langword="true"/> if the language is supported, otherwise <see langword="false"/>.</returns>
    public bool IsLanguageSupported(string language)
    {
        TranslatorGuards.NotNull(language);

        return Language.TryGetLanguage(language, out var lang) && IsLanguageSupported(lang);
    }

    /// <inheritdoc cref="IsLanguageSupported(string)"/>
    public bool IsLanguageSupported(Language language)
    {
        TranslatorGuards.NotNull(language);

        return (language.SupportedServices & TranslationServices.Microsoft) == TranslationServices.Microsoft;
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(true);

    /// <inheritdoc cref="TranslateAsync(string, string, string)"/>
    async Task<ITranslationResult> ITranslator.TranslateAsync(string text, string toLanguage, string? fromLanguage)
        => await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

    /// <inheritdoc cref="TranslateAsync(string, ILanguage, ILanguage)"/>
    async Task<ITranslationResult> ITranslator.TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage)
        => await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

    Task<ITransliterationResult> ITranslator.TransliterateAsync(string text, string toLanguage, string? fromLanguage)
        => throw new NotSupportedException("This translator does not support transliteration via languages.");

    Task<ITransliterationResult> ITranslator.TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage)
        => throw new NotSupportedException("This translator does not support transliteration via languages.");

    /// <inheritdoc cref="DetectLanguageAsync(string)"/>
    async Task<ILanguage> ITranslator.DetectLanguageAsync(string text) => await DetectLanguageAsync(text).ConfigureAwait(false);

    /// <inheritdoc cref="IsLanguageSupported(Language)"/>
    bool ITranslator.IsLanguageSupported(ILanguage language) => language is Language lang && IsLanguageSupported(lang);

    /// <summary>
    /// Hot-patches language codes to Microsoft-specific ones.
    /// </summary>
    /// <param name="languageCode">The language code.</param>
    /// <returns>The hot-patched language code.</returns>
    private static string MicrosoftHotPatch(string languageCode)
    {
        TranslatorGuards.NotNull(languageCode);

        return languageCode switch
        {
            "lg" => "lug",
            "no" => "nb",
            "ny" => "nya",
            "sr" => "sr-Cyrl",
            "mn" => "mn-Cyrl",
            "tlh" => "tlh-Latn",
            "zh-CN" => "zh-Hans",
            "zh-TW" => "zh-Hant",
            _ => languageCode
        };
    }

    private static void EnsureValidTTSLanguage(string language, out MicrosoftVoice voice)
    {
        if (!DefaultVoices.TryGetValue(language, out var temp))
        {
            throw new ArgumentException($"Unable to get the voice from language {language}.", nameof(language));
        }

        voice = temp;
    }

    private static void EnsureValidScripts(string language, string fromScript, string toScript)
    {
        if (!Scripts.TryGetValue(language, out var scripts))
        {
            throw new ArgumentException("This language does not support transliteration.", nameof(language));
        }

        if (!scripts.Contains(fromScript))
        {
            throw new ArgumentException("Script not supported.", nameof(fromScript));
        }

        if (!scripts.Contains(toScript))
        {
            throw new ArgumentException("Script not supported.", nameof(toScript));
        }

        if (fromScript == toScript)
        {
            throw new ArgumentException($"\"{nameof(fromScript)}\" and \"{nameof(toScript)}\" cannot be equal.");
        }
    }

    /// <summary>
    /// Returns the name of this translator.
    /// </summary>
    /// <returns>The name of this translator.</returns>
    public override string ToString() => $"{nameof(Name)}: {Name}";

    /// <inheritdoc cref="Dispose()"/>
    private void Dispose(bool disposing)
    {
        if (!disposing || _disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _voicesSemaphore.Dispose();
        _authTokenInfoSemaphore.Dispose();
        _disposed = true;
    }

    private static string GetSignature(string url)
    {
        string guid = Guid.NewGuid().ToString("N");
        string escapedUrl = Uri.EscapeDataString(url);
        string dateTime = DateTimeOffset.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ssG\\MT", CultureInfo.InvariantCulture);

        byte[] bytes = Encoding.UTF8.GetBytes($"MSTranslatorAndroidApp{escapedUrl}{dateTime}{guid}".ToLowerInvariant());

#if NET6_0_OR_GREATER
        byte[] hash = HMACSHA256.HashData(_privateKey, bytes);
#else
        var hmac = new HMACSHA256(_privateKey);
        byte[] hash = hmac.ComputeHash(bytes);
#endif
        return $"MSTranslatorAndroidApp::{Convert.ToBase64String(hash)}::{dateTime}::{guid}";
    }

    private static bool TryGetExpirationDate(in JsonElement element, out DateTimeOffset expirationDate)
    {
        string? token = element.GetString();
        var span = token.AsSpan();
        int index = span.IndexOf('.');
        int lastIndex = span.LastIndexOf('.');

        if (index != -1 && index < lastIndex)
        {
            var encodedPayload = token.AsSpan()[++index..lastIndex];
            byte[] payload = Base64UrlDecode(encodedPayload.ToString());

            var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty("exp"u8, out var exp) && exp.TryGetInt64(out long unixSeconds))
            {
                expirationDate = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
                return true;
            }
        }

        expirationDate = default;
        return false;
    }

    private static byte[] Base64UrlDecode(string text)
    {
        int padding = 3 - (text.Length + 3) % 4;
        if (padding > 0)
        {
            text = $"{text}{new string('=', padding)}";
        }

        return Convert.FromBase64String(text.Replace('-', '+').Replace('_', '/'));
    }
}