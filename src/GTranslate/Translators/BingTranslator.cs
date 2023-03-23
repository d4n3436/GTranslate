using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GTranslate.Extensions;
using GTranslate.Results;

namespace GTranslate.Translators;

/// <summary>
/// Represents the Bing Translator.
/// </summary>
public sealed class BingTranslator : ITranslator, IDisposable
{
    private const string HostUrl = "https://www.bing.com";
    private const string TtsEndpoint = $"{HostUrl}/tfettts";
    private static readonly Uri _translatorPageUri = new($"{HostUrl}/translator");  
    private const string Iid = "translator.5024.1";
    private static ReadOnlySpan<byte> CredentialsStart => "var params_AbusePreventionHelper = ["u8;

    /// <inheritdoc/>
    public string Name => nameof(BingTranslator);

    private readonly HttpClient _httpClient;
    private CachedObject<BingCredentials> _cachedCredentials;
    private readonly SemaphoreSlim _credentialsSemaphore = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BingTranslator"/> class.
    /// </summary>
    public BingTranslator()
        : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BingTranslator"/> class with the provided <see cref="HttpClient"/> instance.
    /// </summary>
    /// <param name="httpClient">An <see cref="HttpClient"/> instance.</param>
    public BingTranslator(HttpClient httpClient)
    {
        TranslatorGuards.NotNull(httpClient);

        if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.DefaultUserAgent);
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
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="toLanguage"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="Language"/> could not be obtained from <paramref name="toLanguage"/> or <paramref name="fromLanguage"/>.</exception>
    /// <exception cref="TranslatorException">
    /// Thrown when <paramref name="toLanguage"/> or <paramref name="fromLanguage"/> are not supported, or an error occurred during the operation.
    /// </exception>
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
        var uri = new Uri($"{HostUrl}/ttranslatev3?isVertical=1&IG={credentials.ImpressionGuid.ToString("N").ToUpperInvariant()}&IID={Iid}");
        using var response = await _httpClient.PostAsync(uri, content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        // Bing Translator always return status code 200 regardless of the content
        using var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
        var root = document.RootElement;

        TranslatorGuards.ThrowIfStatusCodeIsPresent(root);

        var first = root.FirstOrDefault();
        var translation = first.GetPropertyOrDefault("translations"u8).FirstOrDefault();

        if (first.ValueKind == JsonValueKind.Undefined || translation.ValueKind == JsonValueKind.Undefined)
        {
            throw new TranslatorException("The API returned an empty response.", Name);
        }

        var langDetection = first.GetProperty("detectedLanguage"u8);
        string detectedLanguage = langDetection.GetProperty("language"u8).GetString() ?? string.Empty;
        float score = langDetection.GetProperty("score"u8).GetSingle();
        string translatedText = translation.GetProperty("text"u8).GetString() ?? throw new TranslatorException("Failed to get the translated text.", Name);
        string targetLanguage = translation.GetProperty("to"u8).GetString() ?? toLanguage.ISO6391;
        string? script = translation.GetPropertyOrDefault("transliteration"u8).GetPropertyOrDefault("script"u8).GetStringOrDefault();
        string? transliteration = translation.GetPropertyOrDefault("transliteration"u8).GetPropertyOrDefault("text"u8).GetStringOrDefault()
                                  ?? root.ElementAtOrDefault(1).GetPropertyOrDefault("inputTransliteration"u8).GetStringOrDefault();

        return new BingTranslationResult(translatedText, text, Language.GetLanguage(targetLanguage), Language.GetLanguage(detectedLanguage), transliteration, script, score);
    }

    /// <summary>
    /// Transliterates a text using Bing Translator.
    /// </summary>
    /// <param name="text">The text to transliterate.</param>
    /// <param name="toLanguage">The target language.</param>
    /// <param name="fromLanguage">The source language.</param>
    /// <returns>A task that represents the asynchronous transliteration operation. The task contains the transliteration result.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="toLanguage"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="Language"/> could not be obtained from <paramref name="toLanguage"/> or <paramref name="fromLanguage"/>.</exception>
    /// <exception cref="TranslatorException">
    /// Thrown when <paramref name="toLanguage"/> or <paramref name="fromLanguage"/> are not supported, or an error occurred during the operation.
    /// </exception>
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
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <exception cref="TranslatorException">Thrown when an error occurred during the operation.</exception>
    public async Task<Language> DetectLanguageAsync(string text)
    {
        TranslatorGuards.NotNull(text);

        var result = await TranslateAsync(text, "en").ConfigureAwait(false);
        if (result.SourceLanguage is null)
        {
            throw new TranslatorException("Failed to get the detected language.", Name);
        }

        return result.SourceLanguage;
    }

    /// <summary>
    /// Converts text into synthesized speech.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="language">The language of the voice. Only the languages in <see cref="MicrosoftTranslator.DefaultVoices"/> are supported.</param>
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
    /// <remarks>No validation will be performed to the <paramref name="voice"/> parameter. Make sure to get the correct voices from either <see cref="MicrosoftTranslator.DefaultVoices"/> or <see cref="MicrosoftTranslator.GetTTSVoicesAsync"/>.</remarks>
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

        var credentials = await GetOrUpdateCredentialsAsync().ConfigureAwait(false);

        string ssml = $"<speak version='1.0' xml:lang='{voice.Locale}'><voice xml:lang='{voice.Locale}' xml:gender='{voice.Gender}' name='{voice.ShortName}'><prosody rate='{speakRate}'>{MicrosoftTranslator._ssmlEncoder.Encode(text)}</prosody></voice></speak>";

        var data = new Dictionary<string, string>
        {
            { "ssml", ssml },
            { "token", credentials.Token },
            { "key", credentials.Key.ToString() }
        };

        using var content = new FormUrlEncodedContent(data);

        var uri = new Uri($"{TtsEndpoint}?isVertical=1&IG={credentials.ImpressionGuid.ToString("N").ToUpperInvariant()}&IID={Iid}");
        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = uri,
            Content = content
        };

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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

    // returns new credentials as a cached object
    private static async Task<CachedObject<BingCredentials>> GetCredentialsAsync(ITranslator translator, HttpClient httpClient)
    {
        byte[] bytes = await httpClient.GetByteArrayAsync(_translatorPageUri).ConfigureAwait(false);
        return GetCredentials(bytes, translator);
    }

    private static CachedObject<BingCredentials> GetCredentials(ReadOnlySpan<byte> bytes, ITranslator translator)
    {
        int credentialsStartIndex = bytes.IndexOf(CredentialsStart);
        if (credentialsStartIndex == -1)
        {
            throw new TranslatorException("Unable to find the Bing credentials.", translator.Name);
        }

        int keyStartIndex = credentialsStartIndex + CredentialsStart.Length;
        int keyLength = bytes[keyStartIndex..].IndexOf((byte)',');
        if (keyLength == -1)
        {
            throw new TranslatorException("Unable to find the Bing key.", translator.Name);
        }

        // Unix timestamp generated once the page is loaded. Valid for 3600000 milliseconds or 1 hour
        if (!Utf8Parser.TryParse(bytes.Slice(keyStartIndex, keyLength), out long key, out _))
        {
            // This shouldn't happen but we'll handle this case anyways
            key = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        int tokenStartIndex = keyStartIndex + keyLength + 2;
        int tokenLength = bytes[tokenStartIndex..].IndexOf((byte)'"');
        if (tokenLength == -1)
        {
            throw new TranslatorException("Unable to find the Bing token.", translator.Name);
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        string token = Encoding.UTF8.GetString(bytes.Slice(tokenStartIndex, tokenLength));
#else
        string token = Encoding.UTF8.GetString(bytes.Slice(tokenStartIndex, tokenLength).ToArray());
#endif
        var credentials = new BingCredentials(token, key, Guid.NewGuid());

        return new CachedObject<BingCredentials>(credentials, DateTimeOffset.FromUnixTimeMilliseconds(key + 3600000));
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
        if (!MicrosoftTranslator.DefaultVoices.TryGetValue(language, out var temp))
        {
            throw new ArgumentException($"Unable to get the voice from language {language}.", nameof(language));
        }

        voice = temp;
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
        _credentialsSemaphore.Dispose();
        _disposed = true;
    }

    private async ValueTask<BingCredentials> GetOrUpdateCredentialsAsync()
    {
        if (!_cachedCredentials.IsExpired())
        {
            return _cachedCredentials.Value;
        }

        await _credentialsSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (!_cachedCredentials.IsExpired())
            {
                return _cachedCredentials.Value;
            }

            _cachedCredentials = await GetCredentialsAsync(this, _httpClient).ConfigureAwait(false);
        }
        finally
        {
            _credentialsSemaphore.Release();
        }

        return _cachedCredentials.Value;
    }
}