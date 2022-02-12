using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GTranslate.Results;

namespace GTranslate.Translators;

/// <summary>
/// Represents the Bing Translator.
/// </summary>
public sealed class BingTranslator : ITranslator, IDisposable
{
    private const string _hostUrl = "https://www.bing.com";
    private const string _iid = "translator.5023.3";
    private const string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.81 Safari/537.36";

    /// <inheritdoc/>
    public string Name => "BingTranslator";

    private readonly HttpClient _httpClient;
    private CachedObject<BingCredentials> _cachedCredentials;
    private CachedObject<BingAuthTokenInfo> _cachedAuthTokenInfo;
    private BingVoice[] _voices = Array.Empty<BingVoice>();
    private bool _disposed;

    /// <summary>
    /// Gets a read-only dictionary containing the hardcoded TTS voices used internally in Bing Translator.
    /// </summary>
    /// <remarks>
    /// This dictionary is incomplete; it only includes 1 voice per language despite the API offering multiple voices.<br/>
    /// To get the complete list, use <see cref="GetTTSVoicesAsync"/>.
    /// </remarks>
    public static IReadOnlyDictionary<string, BingVoice> DefaultVoices => new ReadOnlyDictionary<string, BingVoice>(new Dictionary<string, BingVoice>
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
    /// Initializes a new instance of the <see cref="BingTranslator"/> class.
    /// </summary>
    public BingTranslator() : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BingTranslator"/> class with the provided <see cref="HttpClient"/>.
    /// </summary>
    public BingTranslator(HttpClient httpClient)
    {
        TranslatorGuards.NotNull(httpClient);

        if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);
        }

        _httpClient = httpClient;
    }

    /// <summary>
    /// Translates a text using Bing Translator.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <param name="toLanguage">The target language.</param>
    /// <param name="fromLanguage">The source language.</param>
    /// <returns>A task that represents the asynchronous translation operation. The task contains the translation result.</returns>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="TranslatorException"/>
    public async Task<BingTranslationResult> TranslateAsync(string text, string toLanguage, string? fromLanguage = null)
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
    public async Task<BingTranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

        var credentials = await GetOrUpdateCredentialsAsync().ConfigureAwait(false);

        var data = new Dictionary<string, string>
        {
            { "fromLang", BingHotPatch(fromLanguage?.ISO6391 ?? "auto-detect") },
            { "text", text },
            { "to", BingHotPatch(toLanguage.ISO6391) },
            { "token", credentials.Token },
            { "key", credentials.Key.ToString() }
        };

        using var content = new FormUrlEncodedContent(data);
        // For some reason the "isVertical" parameter allows you to translate up to 1000 characters instead of 500
        var uri = new Uri($"{_hostUrl}/ttranslatev3?isVertical=1&IG={credentials.ImpressionGuid.ToString("N").ToUpperInvariant()}&IID={_iid}");
        var response = await _httpClient.PostAsync(uri, content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

        // Bing Translator always return status code 200 regardless of the content
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;

        if (root.TryGetInt32("statusCode", out int code))
        {
            var errorMessage = root.GetPropertyOrDefault("errorMessage").GetStringOrDefault();
            throw new TranslatorException(!string.IsNullOrEmpty(errorMessage) ? errorMessage! : $"The API returned status code {code}.", Name);
        }

        var first = root.FirstOrDefault();
        var translation = first.GetPropertyOrDefault("translations").FirstOrDefault();

        if (first.ValueKind == JsonValueKind.Undefined || translation.ValueKind == JsonValueKind.Undefined)
        {
            throw new TranslatorException("The API returned an empty response.", Name);
        }

        var langDetection = first.GetProperty("detectedLanguage");
        string detectedLanguage = langDetection.GetProperty("language").GetString() ?? "";
        float score = langDetection.GetProperty("score").GetSingle();
        string translatedText = translation.GetProperty("text").GetString() ?? throw new TranslatorException("Failed to get the translated text.", Name);
        string targetLanguage = translation.GetProperty("to").GetString() ?? toLanguage.ISO6391;
        string? script = translation.GetPropertyOrDefault("transliteration").GetPropertyOrDefault("script").GetStringOrDefault();
        string? transliteration = translation.GetPropertyOrDefault("transliteration").GetPropertyOrDefault("text").GetStringOrDefault()
                                  ?? root.ElementAtOrDefault(1).GetPropertyOrDefault("inputTransliteration").GetStringOrDefault();

        return new BingTranslationResult(translatedText, text, Language.GetLanguage(targetLanguage), Language.GetLanguage(detectedLanguage), transliteration, script, score);
    }

    /// <summary>
    /// Transliterates a text using Bing Translator.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <param name="toLanguage">The target language.</param>
    /// <param name="fromLanguage">The source language.</param>
    /// <returns>A task that represents the asynchronous transliteration operation. The task contains the transliteration result.</returns>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="TranslatorException"/>
    public async Task<BingTransliterationResult> TransliterateAsync(string text, string toLanguage, string? fromLanguage = null)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        TranslatorGuards.LanguageFound(toLanguage, out var toLang, "Unknown target language.");
        TranslatorGuards.LanguageFound(fromLanguage, out var fromLang, "Unknown source language.");
        TranslatorGuards.LanguageSupported(this, toLang, fromLang);

        return await TransliterateAsync(text, toLang, fromLang).ConfigureAwait(false);
    }

    /// <inheritdoc cref="TransliterateAsync(string, string, string)"/>
    public async Task<BingTransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

        var result = await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);
        if (string.IsNullOrEmpty(result.Transliteration))
        {
            throw new TranslatorException("Failed to get the transliteration.", Name);
        }

        return new BingTransliterationResult(result.Transliteration!, text, result.TargetLanguage, result.SourceLanguage, result.Script);
    }

    /// <summary>
    /// Detects the language of a text using Bing Translator.
    /// </summary>
    /// <param name="text">The text to detect its language.</param>
    /// <returns>A task that represents the asynchronous language detection operation. The task contains the detected language.</returns>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="TranslatorException"/>
    public async Task<Language> DetectLanguageAsync(string text)
    {
        TranslatorGuards.NotNull(text);

        var result = await TranslateAsync(text, "en").ConfigureAwait(false);
        if (result.SourceLanguage is null)
        {
            throw new TranslatorException("Failed to get the detected language", Name);
        }

        return result.SourceLanguage;
    }

    /// <summary>
    /// Converts text into synthesized speech.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="language">The language of the voice. Only the languages in <see cref="DefaultVoices"/> are supported.</param>
    /// <param name="speakRate">The speaking rate of the text, expressed as a number that acts as a multiplier of the default.</param>
    /// <returns>A task that represents the asynchronous synthesis operation. The task contains the synthesized speech in a MP3 <see cref="Stream"/>.</returns>
    public async Task<Stream> TextToSpeechAsync(string text, string language, float speakRate = 1)
    {
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(language);

        if (!DefaultVoices.TryGetValue(language, out var voice))
        {
            throw new ArgumentException($"Unable to get the voice from language {language}.", nameof(language));
        }

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
    public async Task<Stream> TextToSpeechAsync(string text, BingVoice voice, float speakRate = 1)
    {
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(voice);

        return await TextToSpeechAsync(text, voice.ShortName, voice.Locale, voice.Gender, speakRate).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts text into synthesized speech.
    /// </summary>
    /// <remarks>No validation will be performed to the parameters. Make sure to get the correct voices from either <see cref="DefaultVoices"/> or <see cref="GetTTSVoicesAsync"/>.</remarks>
    /// <param name="text">The text to convert.</param>
    /// <param name="voiceName">The name (ShortName) of the voice.</param>
    /// <param name="voiceLocale">The locale of the voice.</param>
    /// <param name="voiceGender">The gender of the voice.</param>
    /// <param name="speakRate">The speaking rate of the text, expressed as a number that acts as a multiplier of the default.</param>
    /// <returns>A task that represents the asynchronous synthesis operation. The task contains the synthesized speech in a MP3 <see cref="Stream"/>.</returns>
    public async Task<Stream> TextToSpeechAsync(string text, string voiceName, string voiceLocale, string voiceGender, float speakRate = 1)
    {
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(voiceName);
        TranslatorGuards.NotNull(voiceLocale);
        TranslatorGuards.NotNull(voiceGender);

        var authInfo = await GetOrUpdateBingAuthTokenAsync();

        string payload = $"<speak version='1.0' xml:lang='{voiceLocale}'><voice xml:lang='{voiceLocale}' xml:gender='{voiceGender}' name='{voiceName}'><prosody rate='{speakRate}'>{text}</prosody></voice></speak>";

        var request = new HttpRequestMessage
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
    public async ValueTask<IReadOnlyCollection<BingVoice>> GetTTSVoicesAsync()
    {
        if (_voices.Length != 0)
        {
            return _voices;
        }

        var authInfo = await GetOrUpdateBingAuthTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://{authInfo.Region}.tts.speech.microsoft.com/cognitiveservices/voices/list"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authInfo.Token);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        _voices = await JsonSerializer.DeserializeAsync<BingVoice[]>(stream).ConfigureAwait(false) ?? throw new TranslatorException("Failed to deserialize voice list.", Name);

        return _voices;
    }

    /// <summary>
    /// Returns whether Bing Translator supports the specified language.
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

        return (language.SupportedServices & TranslationServices.Bing) == TranslationServices.Bing;
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(true);

    /// <inheritdoc cref="TranslateAsync(string, string, string)"/>
    async Task<ITranslationResult> ITranslator.TranslateAsync(string text, string toLanguage, string? fromLanguage)
        => await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

    /// <inheritdoc cref="TranslateAsync(string, ILanguage, ILanguage)"/>
    async Task<ITranslationResult> ITranslator.TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage)
        => await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

    /// <inheritdoc cref="TransliterateAsync(string, string, string)"/>
    async Task<ITransliterationResult> ITranslator.TransliterateAsync(string text, string toLanguage, string? fromLanguage)
        => await TransliterateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

    /// <inheritdoc cref="TransliterateAsync(string, ILanguage, ILanguage)"/>
    async Task<ITransliterationResult> ITranslator.TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage)
        => await TransliterateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

    /// <inheritdoc cref="DetectLanguageAsync(string)"/>
    async Task<ILanguage> ITranslator.DetectLanguageAsync(string text) => await DetectLanguageAsync(text).ConfigureAwait(false);

    /// <inheritdoc cref="IsLanguageSupported(Language)"/>
    bool ITranslator.IsLanguageSupported(ILanguage language) => language is Language lang && IsLanguageSupported(lang);

    /// <inheritdoc cref="Dispose()"/>
    private void Dispose(bool disposing)
    {
        if (!disposing || _disposed) return;

        _httpClient.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Hot-patches language codes to Bing-specific ones.
    /// </summary>
    /// <param name="languageCode">The language code.</param>
    /// <returns>The hot-patched language code.</returns>
    private static string BingHotPatch(string languageCode)
    {
        TranslatorGuards.NotNull(languageCode);

        return languageCode switch
        {
            "no" => "nb",
            "sr" => "sr-Cyrl",
            "tlh" => "tlh-Latn",
            "zh-CN" => "zh-Hans",
            "zh-TW" => "zh-Hant",
            _ => languageCode
        };
    }

    private async ValueTask<BingCredentials> GetOrUpdateCredentialsAsync()
    {
        if (!_cachedCredentials.IsExpired())
        {
            return _cachedCredentials.Value;
        }

        const string credentialsStart = "var params_RichTranslateHelper = [";

        string content = await _httpClient.GetStringAsync(new Uri($"{_hostUrl}/translator")).ConfigureAwait(false);

        int credentialsStartIndex = content.IndexOf(credentialsStart, StringComparison.Ordinal);
        if (credentialsStartIndex == -1)
        {
            throw new TranslatorException("Unable to find the Bing credentials.", Name);
        }

        int keyStartIndex = credentialsStartIndex + credentialsStart.Length;
        int keyEndIndex = content.IndexOf(',', keyStartIndex);
        if (keyEndIndex == -1)
        {
            throw new TranslatorException("Unable to find the Bing key.", Name);
        }

        // Unix timestamp generated once the page is loaded. Valid for 3600000 milliseconds or 1 hour
#if NET6_0_OR_GREATER
        if (!long.TryParse(content.AsSpan(keyStartIndex, keyEndIndex - keyStartIndex), out long key))
#else
        if (!long.TryParse(content.AsSpan(keyStartIndex, keyEndIndex - keyStartIndex).ToString(), out long key))
#endif
        {
            // This shouldn't happen but we'll handle this case anyways
            key = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        int tokenStartIndex = keyEndIndex + 2;
        int tokenEndIndex = content.IndexOf('"', tokenStartIndex);
        if (tokenEndIndex == -1)
        {
            throw new TranslatorException("Unable to find the Bing token.", Name);
        }

        string token = content.Substring(tokenStartIndex, tokenEndIndex - tokenStartIndex);
        var credentials = new BingCredentials(token, key, Guid.NewGuid());

        _cachedCredentials = new CachedObject<BingCredentials>(credentials, DateTimeOffset.FromUnixTimeMilliseconds(key + 3600000));
        return _cachedCredentials.Value;
    }

    private async ValueTask<BingAuthTokenInfo> GetOrUpdateBingAuthTokenAsync()
    {
        if (!_cachedAuthTokenInfo.IsExpired())
        {
            return _cachedAuthTokenInfo.Value;
        }

        var credentials = await GetOrUpdateCredentialsAsync().ConfigureAwait(false);

        var data = new Dictionary<string, string>
        {
            { "token", credentials.Token },
            { "key", credentials.Key.ToString() }
        };

        using var content = new FormUrlEncodedContent(data);
        var uri = new Uri($"{_hostUrl}/tfetspktok?isVertical=1&IG={credentials.ImpressionGuid.ToString("N").ToUpperInvariant()}&IID={_iid}");
        var response = await _httpClient.PostAsync(uri, content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

        // Bing Translator always return status code 200 regardless of the content
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;

        if (root.TryGetInt32("statusCode", out int code))
        {
            var errorMessage = root.GetPropertyOrDefault("errorMessage").GetStringOrDefault();
            throw new TranslatorException(!string.IsNullOrEmpty(errorMessage) ? errorMessage! : $"The API returned status code {code}.", Name);
        }

        // Authentication tokens seem to be always valid for 10 minutes
        // https://docs.microsoft.com/en-us/azure/cognitive-services/authentication?tabs=powershell#authenticate-with-an-authentication-token
        int expiryDurationInMs = root.GetProperty("expiryDurationInMS").GetInt32OrDefault(600000);
        string token = root.GetProperty("token").GetString() ?? throw new TranslatorException("Unable to get the Bing Auth token." ,Name);
        string region = root.GetProperty("region").GetString() ?? throw new TranslatorException("Unable to get the Bing API region.", Name);

        var authInfo = new BingAuthTokenInfo(token, region);

        _cachedAuthTokenInfo = new CachedObject<BingAuthTokenInfo>(authInfo, TimeSpan.FromMilliseconds(expiryDurationInMs));

        return authInfo;
    }

    /// <summary>
    /// Represents a TTS voice in Bing Translator.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
    public class BingVoice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BingVoice"/> class.
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="shortName"></param>
        /// <param name="gender"></param>
        /// <param name="locale"></param>
        public BingVoice(string displayName, string shortName, string gender, string locale)
        {
            DisplayName = displayName;
            Gender = gender;
            ShortName = shortName;
            Locale = locale;
        }

        /// <summary>
        /// Gets the display name of this voice.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the short name of this voice.
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Gets the gender of this voice.
        /// </summary>
        public string Gender { get; }

        /// <summary>
        /// Gets the locale of this voice.
        /// </summary>
        public string Locale { get; }

        /// <inheritdoc/>
        public override string ToString() => $"{DisplayName} ({Locale})";

        private string DebuggerDisplay => ToString();
    }

    private readonly struct BingCredentials
    {
        public BingCredentials(string token, long key, Guid impressionGuid)
        {
            Token = token;
            Key = key;
            ImpressionGuid = impressionGuid;
        }

        public string Token { get; }

        public long Key { get; }

        public Guid ImpressionGuid { get; }
    }

    private sealed class BingAuthTokenInfo
    {
        public BingAuthTokenInfo(string token, string region)
        {
            Token = token;
            Region = region;
        }

        public string Token { get; }

        public string Region { get; }
    }
}