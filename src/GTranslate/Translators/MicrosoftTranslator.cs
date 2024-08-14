using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using GTranslate.Extensions;
using GTranslate.Models;
using GTranslate.Results;

namespace GTranslate.Translators;

/// <summary>
/// Represents the Microsoft Azure translator.
/// </summary>
public sealed class MicrosoftTranslator : ITranslator, IDisposable
{
    private const string ApiEndpoint = "api.cognitive.microsofttranslator.com";
    private const string ApiVersion = "3.0";
    private const string DetectUrl = $"{ApiEndpoint}/detect?api-version={ApiVersion}";
    private const string SpeechTokenUrl = "dev.microsofttranslator.com/apps/endpoint?api-version=1.0";

    // test base domain (405 error): dev-sn2-test1.microsofttranslator-int.com
    // end point co4 (405 error): https://dev-co4-test1.microsofttranslator-int.com/
    // end point int (405 error): https://dev.microsofttranslator-int.com/

    private static readonly Uri DetectUri = new($"https://{DetectUrl}");
    private static readonly Uri SpeechTokenUri = new($"https://{SpeechTokenUrl}");
    internal static readonly HtmlEncoder SsmlEncoder = HtmlEncoder.Create(UnicodeRanges.All); // Like the default encoder but only encodes required characters

    // From Microsoft Translator Android app
    private static readonly byte[] PrivateKey =
    [
        0xa2, 0x29, 0x3a, 0x3d, 0xd0, 0xdd, 0x32, 0x73,
        0x97, 0x7a, 0x64, 0xdb, 0xc2, 0xf3, 0x27, 0xf5,
        0xd7, 0xbf, 0x87, 0xd9, 0x45, 0x9d, 0xf0, 0x5a,
        0x09, 0x66, 0xc6, 0x30, 0xc6, 0x6a, 0xaa, 0x84,
        0x9a, 0x41, 0xaa, 0x94, 0x3a, 0xa8, 0xd5, 0x1a,
        0x6e, 0x4d, 0xaa, 0xc9, 0xa3, 0x70, 0x12, 0x35,
        0xc7, 0xeb, 0x12, 0xf6, 0xe8, 0x23, 0x07, 0x9e,
        0x47, 0x10, 0x95, 0x91, 0x88, 0x55, 0xd8, 0x17
    ];

    /// <inheritdoc/>
    public string Name => nameof(MicrosoftTranslator);

    private readonly HttpClient _httpClient;
    private CachedObject<MicrosoftAuthTokenInfo> _cachedAuthTokenInfo;
    private readonly SemaphoreSlim _voicesSemaphore = new(1, 1);
    private readonly SemaphoreSlim _authTokenInfoSemaphore = new(1, 1);
    private MicrosoftVoice[] _voices = [];
    private bool _disposed;
    
    /// <summary>
    /// Gets a read-only dictionary containing the hardcoded TTS voices used internally in Bing Translator.
    /// </summary>
    /// <remarks>
    /// This dictionary is incomplete; it only includes 1 voice per language despite the API offering multiple voices.<br/>
    /// To get the complete list, use <see cref="GetTTSVoicesAsync"/>.
    /// </remarks>
    public static IReadOnlyDictionary<string, MicrosoftVoice> DefaultVoices { get; } = new Dictionary<string, MicrosoftVoice>
    {
        ["af"] = new("Adri", "af-ZA-AdriNeural", "Female", "af-ZA"),
        ["am"] = new("Mekdes", "am-ET-MekdesNeural", "Female", "am-ET"),
        ["ar"] = new("Hamed", "ar-SA-HamedNeural", "Male", "ar-SA"),
        ["bg"] = new("Borislav", "bg-BG-BorislavNeural", "Male", "bg-BG"),
        ["bn"] = new("Tanishaa", "bn-IN-TanishaaNeural", "Female", "br-IN"),
        ["ca"] = new("Joana", "ca-ES-JoanaNeural", "Female", "ca-ES"),
        ["cs"] = new("Antonin", "cs-CZ-AntoninNeural", "Male", "cs-CZ"),
        ["cy"] = new("Nia", "cy-GB-NiaNeural", "Female", "cy-GB"),
        ["da"] = new("Christel", "da-DK-ChristelNeural", "Female", "da-DK"),
        ["de"] = new("Katja", "de-DE-KatjaNeural", "Female", "de-DE"),
        ["el"] = new("Nestoras", "el-GR-NestorasNeural", "Male", "el-GR"),
        ["en"] = new("Aria", "en-US-AriaNeural", "Female", "en-US"),
        ["es"] = new("Elvira", "es-ES-ElviraNeural", "Female", "es-ES"),
        ["et"] = new("Anu", "et-EE-AnuNeural", "Female", "et-EE"),
        ["fa"] = new("Dilara", "fa-IR-DilaraNeural", "Female", "fa-IR"),
        ["fi"] = new("Noora", "fi-FI-NooraNeural", "Female", "fi-FI"),
        ["fr"] = new("Denise", "fr-FR-DeniseNeural", "Female", "fr-FR"),
        ["fr-CA"] = new("Sylvie", "fr-CA-SylvieNeural", "Female", "fr-CA"),
        ["ga"] = new("Orla", "ga-IE-OrlaNeural", "Female", "ga-IE"),
        ["gu"] = new("Dhwani", "gu-IN-DhwaniNeural", "Female", "gu-IN"),
        ["he"] = new("Avri", "he-IL-AvriNeural", "Male", "he-IL"),
        ["hi"] = new("Swara", "hi-IN-SwaraNeural", "Female", "hi-IN"),
        ["hr"] = new("Srecko", "hr-HR-SreckoNeural", "Male", "hr-HR"),
        ["hu"] = new("Tamas", "hu-HU-TamasNeural", "Male", "hu-HU"),
        ["id"] = new("Ardi", "id-ID-ArdiNeural", "Male", "id-ID"),
        ["is"] = new("Gudrun", "is-IS-GudrunNeural", "Female", "is-IS"),
        ["it"] = new("Diego", "it-IT-DiegoNeural", "Male", "it-IT"),
        ["ja"] = new("Nanami", "ja-JP-NanamiNeural", "Female", "ja-JP"),
        ["kk"] = new("Aigul", "kk-KZ-AigulNeural", "Female", "kk-KZ"),
        ["km"] = new("Sreymom", "km-KH-SreymomNeural", "Female", "km-KH"),
        ["kn"] = new("Sapna", "kn-IN-SapnaNeural", "Female", "kn-IN"),
        ["ko"] = new("SunHi", "ko-KR-SunHiNeural", "Female", "ko-KR"),
        ["lo"] = new("Keomany", "lo-LA-KeomanyNeural", "Female", "lo-LA"),
        ["lv"] = new("Everita", "lv-LV-EveritaNeural", "Female", "lv-LV"),
        ["lt"] = new("Ona", "lt-LT-OnaNeural", "Female", "lt-LT"),
        ["mk"] = new("Marija", "mk-MK-MarijaNeural", "Female", "mk-MK"),
        ["ml"] = new("Sobhana", "ml-IN-SobhanaNeural", "Female", "ml-IN"),
        ["mr"] = new("Aarohi", "mr-IN-AarohiNeural", "Female", "mr-IN"),
        ["ms"] = new("Osman", "ms-MY-OsmanNeural", "Male", "ms-MY"),
        ["mt"] = new("Grace", "mt-MT-GraceNeural", "Female", "mt-MT"),
        ["my"] = new("Nilar", "my-MM-NilarNeural", "Female", "my-MM"),
        ["nl"] = new("Colette", "nl-NL-ColetteNeural", "Female", "nl-NL"),
        ["no"] = new("Pernille", "nb-NO-PernilleNeural", "Female", "nb-NO"), // nb
        ["pl"] = new("Zofia", "pl-PL-ZofiaNeural", "Female", "pl-PL"),
        ["ps"] = new("Latifa", "ps-AF-LatifaNeural", "Female", "ps-AF"),
        ["pt"] = new("Francisca", "pt-BR-FranciscaNeural", "Female", "pt-BR"),
        ["pt-PT"] = new("Fernanda", "pt-PT-FernandaNeural", "Female", "pt-PT"),
        ["ro"] = new("Emil", "ro-RO-EmilNeural", "Male", "ro-RO"),
        ["ru"] = new("Dariya", "ru-RU-DariyaNeural", "Female", "ru-RU"),
        ["sk"] = new("Lukas", "sk-SK-LukasNeural", "Male", "sk-SK"),
        ["sl"] = new("Rok", "sl-SI-RokNeural", "Male", "sl-SI"),
        ["sr"] = new("Sophie", "sr-RS-SophieNeural", "Female", "sr-RS"), // sr-Cyrl
        ["sv"] = new("Sofie", "sv-SE-SofieNeural", "Female", "sv-SE"),
        ["ta"] = new("Pallavi", "ta-IN-PallaviNeural", "Female", "ta-IN"),
        ["te"] = new("Shruti", "te-IN-ShrutiNeural", "Male", "te-IN"),
        ["th"] = new("Niwat", "th-TH-NiwatNeural", "Male", "th-TH"),
        ["tr"] = new("Emel", "tr-TR-EmelNeural", "Female", "tr-TR"),
        ["uk"] = new("Polina", "uk-UA-PolinaNeural", "Female", "uk-UA"),
        ["ur"] = new("Gul", "ur-IN-GulNeural", "Female", "ur-IN"),
        ["uz"] = new("Madina", "uz-UZ-MadinaNeural", "Female", "uz-UZ"),
        ["vi"] = new("NamMinh", "vi-VN-NamMinhNeural", "Male", "vi-VN"),
        ["zh-CN"] = new("Xiaoxiao", "zh-CN-XiaoxiaoNeural", "Female", "zh-CN"), // zh-Hans
        ["zh-TW"] = new("Xiaoxiao", "zh-CN-XiaoxiaoNeural", "Female", "zh-CN"), // zh-Hant
        ["yue"] = new("HiuGaai", "zh-HK-HiuGaaiNeural", "Female", "zh-HK")
    }.ToReadOnlyDictionary();

    /// <summary>
    /// Gets a read-only dictionary containing the languages that support transliteration and their supported scripts.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> Scripts { get; } = new Dictionary<string, IReadOnlyCollection<string>>
    {
        ["ar"] = ["Latn", "Arab"],
        ["as"] = ["Latn", "Beng"],
        ["be"] = ["Latn", "Cyrl"],
        ["bg"] = ["Latn", "Cyrl"],
        ["bn"] = ["Latn", "Beng"],
        ["el"] = ["Latn", "Grek"],
        ["fa"] = ["Latn", "Arab"],
        ["gu"] = ["Latn", "Gujr"],
        ["he"] = ["Latn", "Hebr"],
        ["hi"] = ["Latn", "Deva"],
        ["ja"] = ["Latn", "Jpan"],
        ["kk"] = ["Latn", "Cyrl"],
        ["kn"] = ["Latn", "Knda"],
        ["ko"] = ["Latn", "Kore"],
        ["ky"] = ["Latn", "Cyrl"],
        ["mk"] = ["Latn", "Cyrl"],
        ["ml"] = ["Latn", "Mlym"],
        ["mn"] = ["Latn", "Cyrl"],
        ["mr"] = ["Latn", "Deva"],
        ["or"] = ["Latn", "Orya"],
        ["pa"] = ["Latn", "Guru"],
        ["ru"] = ["Latn", "Cyrl"],
        ["sd"] = ["Latn", "Arab"],
        ["si"] = ["Latn", "Sinh"],
        ["ta"] = ["Latn", "Taml"],
        ["te"] = ["Latn", "Telu"],
        ["tg"] = ["Latn", "Cyrl"],
        ["tt"] = ["Latn", "Cyrl"],
        ["uk"] = ["Latn", "Cyrl"],
        ["ur"] = ["Latn", "Arab"],
        ["zh-CN"] = ["Latn", "Hans"], // zh-Hans
        ["zh-TW"] = ["Latn", "Hant"] // zh-Hant
    }.ToReadOnlyDictionary();

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

        string url = $"{ApiEndpoint}/translate?api-version={ApiVersion}&to={MicrosoftHotPatch(toLanguage.ISO6391)}";
        if (fromLanguage is not null)
        {
            url += $"&from={MicrosoftHotPatch(fromLanguage.ISO6391)}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"https://{url}"));
        request.Headers.Add("X-MT-Signature", GetSignature(url));
        request.Content = JsonContent.Create([new MicrosoftTranslatorRequest { Text = text }], MicrosoftTranslatorRequestContext.Default.MicrosoftTranslatorRequestArray);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = (await response.Content.ReadFromJsonAsync(MicrosoftTranslationResultModelContext.Default.MicrosoftTranslationResultModelArray).ConfigureAwait(false))![0];
        var translation = result.Translations[0];

        return new MicrosoftTranslationResult(translation.Text, text, Language.GetLanguage(translation.To), Language.GetLanguage(result.DetectedLanguage.Language), result.DetectedLanguage.Score);
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

        string url = $"{ApiEndpoint}/transliterate?api-version={ApiVersion}&language={MicrosoftHotPatch(language.ISO6391)}&fromScript={fromScript}&toScript={toScript}";

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"https://{url}"));
        request.Headers.Add("X-MT-Signature", GetSignature(url));
        request.Content = JsonContent.Create([new MicrosoftTranslatorRequest { Text = text }], MicrosoftTranslatorRequestContext.Default.MicrosoftTranslatorRequestArray);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = (await response.Content.ReadFromJsonAsync(MicrosoftTransliterationResultModelContext.Default.MicrosoftTransliterationResultModelArray).ConfigureAwait(false))![0];
        return new MicrosoftTransliterationResult(result.Text, text, Language.GetLanguage(language.ISO6391), result.Script, fromScript);
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

        
        using var request = new HttpRequestMessage(HttpMethod.Post, DetectUri);
        request.Headers.Add("X-MT-Signature", GetSignature(DetectUrl));
        request.Content = JsonContent.Create([new MicrosoftTranslatorRequest { Text = text }], MicrosoftTranslatorRequestContext.Default.MicrosoftTranslatorRequestArray);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = (await response.Content.ReadFromJsonAsync(MicrosoftLanguageDetectionResultModelContext.Default.MicrosoftLanguageDetectionResultModelArray).ConfigureAwait(false))![0];
        return Language.GetLanguage(result.Language);
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

        string payload = $"<speak version='1.0' xml:lang='{voice.Locale}'><voice xml:lang='{voice.Locale}' xml:gender='{voice.Gender}' name='{voice.ShortName}'><prosody rate='{speakRate}'>{SsmlEncoder.Encode(text)}</prosody></voice></speak>";

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"https://{authInfo.Region}.tts.speech.microsoft.com/cognitiveservices/v1"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/ssml+xml");
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

            _voices = (await response.Content.ReadFromJsonAsync(MicrosoftVoiceContext.Default.MicrosoftVoiceArray).ConfigureAwait(false))!;
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

            using var request = new HttpRequestMessage(HttpMethod.Post, SpeechTokenUri);
            request.Headers.Add("X-ClientVersion", "N/A");
            request.Headers.Add("X-MT-Signature", GetSignature(SpeechTokenUrl));
            request.Headers.Add("X-UserId", "0");

            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var model = (await response.Content.ReadFromJsonAsync(MicrosoftAuthTokenModelContext.Default.MicrosoftAuthTokenModel).ConfigureAwait(false))!;

            // Tokens are valid for 30 minutes. The exp. date might be closer
            // if another API request is made while the token is still valid.

            // https://docs.microsoft.com/en-us/azure/cognitive-services/authentication?tabs=powershell#authenticate-with-an-authentication-token
            if (!TryGetExpirationDate(model.Token.AsSpan(), out var expirationDate))
            {
                throw new TranslatorException("Unable to obtain the expiration date from the auth token.", Name);
            }

            var authInfo = new MicrosoftAuthTokenInfo(model.Token, model.Region);

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
            "rn" => "run",
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
        byte[] hash = HMACSHA256.HashData(PrivateKey, bytes);
#else
        using var hmac = new HMACSHA256(PrivateKey);
        byte[] hash = hmac.ComputeHash(bytes);
#endif
        return $"MSTranslatorAndroidApp::{Convert.ToBase64String(hash)}::{dateTime}::{guid}";
    }

    private static bool TryGetExpirationDate(ReadOnlySpan<char> token, out DateTimeOffset expirationDate)
    {
        int index = token.IndexOf('.');
        int lastIndex = token.LastIndexOf('.');

        if (index != -1 && index < lastIndex)
        {
            var encodedPayload = token[++index..lastIndex];
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