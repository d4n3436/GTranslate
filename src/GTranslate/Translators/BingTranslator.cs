using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GTranslate.Common;
using GTranslate.Models;
using GTranslate.Results;
using JetBrains.Annotations;

namespace GTranslate.Translators;

/// <summary>
/// Represents the Bing Translator.
/// </summary>
[PublicAPI]
public sealed class BingTranslator : ITranslator, IDisposable
{
    private const string HostUrl = "https://www.bing.com";
    private const string TtsEndpoint = $"{HostUrl}/tfettts";
    private static readonly Uri TranslatorPageUri = new($"{HostUrl}/translator");
    private const string Iid = "translator.5024.1";
    private const int MaxTextLength = 1000;

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
        ArgumentNullException.ThrowIfNull(httpClient);

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
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(toLanguage);
        TranslatorGuards.LanguageFound(toLanguage, out var toLang, "Unknown target language.");
        TranslatorGuards.LanguageFound(fromLanguage, out var fromLang, "Unknown source language.");
        TranslatorGuards.LanguageSupported(this, toLang, fromLang);
        TranslatorGuards.MaxTextLength(text, MaxTextLength);

        return await TranslateAsync(text, toLang, fromLang).ConfigureAwait(false);
    }

    /// <inheritdoc cref="TranslateAsync(string, string, string)"/>
    public async Task<BingTranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(toLanguage);
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);
        TranslatorGuards.MaxTextLength(text, MaxTextLength);

        var credentials = await GetOrUpdateCredentialsAsync().ConfigureAwait(false);

        var data = new Dictionary<string, string>
        {
            { "fromLang", BingHotPatch(fromLanguage?.ISO6391 ?? "auto-detect") },
            { "text", text },
            { "to", BingHotPatch(toLanguage.ISO6391) },
            { "token", credentials.Token },
            { "key", credentials.Key.ToString(CultureInfo.InvariantCulture) }
        };

        using var content = new FormUrlEncodedContent(data);

        // For some reason the "isVertical" parameter allows you to translate up to 1000 characters instead of 500
        var uri = new Uri($"{HostUrl}/ttranslatev3?isVertical=1&IG={credentials.ImpressionGuid.ToString("N").ToUpperInvariant()}&IID={Iid}");
        using var response = await _httpClient.PostAsync(uri, content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        // Bing Translator always return status code 200 regardless of the content
        using var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

        ThrowIfStatusCodeIsPresent(document);

        var results = document.Deserialize(BingTranslationResultModelContext.Default.BingTranslationResultModelArray)!;
        var result = results[0];

        if (result.Translations is null)
        {
            throw new TranslatorException("Received an invalid response from the API.");
        }

        var translation = result.Translations[0];
        string? transliteration = translation.Transliteration?.Text;
        string? sourceTransliteration = results.ElementAtOrDefault(1)?.InputTransliteration;
        string? sourceLanguage = result.DetectedLanguage?.Language ?? fromLanguage?.ISO6391;

        return new BingTranslationResult(translation.Text, text, Language.GetLanguage(translation.To), sourceLanguage is null ? null : Language.GetLanguage(sourceLanguage),
            transliteration, sourceTransliteration, translation.Transliteration?.Script, result.DetectedLanguage?.Score ?? 0);
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(toLanguage);
        TranslatorGuards.LanguageFound(toLanguage, out var toLang, "Unknown target language.");
        TranslatorGuards.LanguageFound(fromLanguage, out var fromLang, "Unknown source language.");
        TranslatorGuards.LanguageSupported(this, toLang, fromLang);
        TranslatorGuards.MaxTextLength(text, MaxTextLength);

        return await TransliterateAsync(text, toLang, fromLang).ConfigureAwait(false);
    }

    /// <inheritdoc cref="TransliterateAsync(string, string, string)"/>
    public async Task<BingTransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(toLanguage);
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);
        TranslatorGuards.MaxTextLength(text, MaxTextLength);

        var result = await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);
        if (!result.HasTransliteration)
        {
            throw new TranslatorException("Failed to get the transliteration.", Name);
        }

        return new BingTransliterationResult(result.Transliteration, result.SourceTransliteration, text, result.TargetLanguage, result.SourceLanguage, result.Script);
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
        ArgumentNullException.ThrowIfNull(text);
        TranslatorGuards.MaxTextLength(text, MaxTextLength);

        var result = await TranslateAsync(text, "en").ConfigureAwait(false);
        return result.SourceLanguage ?? throw new TranslatorException("Unable to detect the language of text.");
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(language);
        EnsureValidTextToSpeechLanguage(language, out var voice);

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
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(voice);

        var credentials = await GetOrUpdateCredentialsAsync().ConfigureAwait(false);

        string ssml = $"<speak version='1.0' xml:lang='{voice.Locale}'><voice xml:lang='{voice.Locale}' xml:gender='{voice.Gender}' name='{voice.ShortName}'><prosody rate='{speakRate}'>{MicrosoftTranslator.SsmlEncoder.Encode(text)}</prosody></voice></speak>";

        var data = new Dictionary<string, string>
        {
            { "ssml", ssml },
            { "token", credentials.Token },
            { "key", credentials.Key.ToString(CultureInfo.InvariantCulture) }
        };

        using var content = new FormUrlEncodedContent(data);

        var uri = new Uri($"{TtsEndpoint}?isVertical=1&IG={credentials.ImpressionGuid.ToString("N").ToUpperInvariant()}&IID={Iid}");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = content;

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
        ArgumentNullException.ThrowIfNull(language);

        return Language.TryGetLanguage(language, out var lang) && IsLanguageSupported(lang);
    }

    /// <inheritdoc cref="IsLanguageSupported(string)"/>
    public bool IsLanguageSupported(Language language)
    {
        ArgumentNullException.ThrowIfNull(language);

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

    private async Task<CachedObject<BingCredentials>> GetCredentialsAsync()
    {
        byte[] bytes = await _httpClient.GetByteArrayAsync(TranslatorPageUri).ConfigureAwait(false);
        return GetCredentials(bytes);
    }

    private CachedObject<BingCredentials> GetCredentials(byte[] html)
    {
        var bytes = html.AsSpan();
        int credentialsStartIndex = bytes.IndexOf(CredentialsStart);
        if (credentialsStartIndex == -1)
        {
            throw new TranslatorException("Unable to find the Bing credentials.", Name);
        }

        int keyStartIndex = credentialsStartIndex + CredentialsStart.Length;
        int keyLength = bytes[keyStartIndex..].IndexOf((byte)',');
        if (keyLength == -1)
        {
            throw new TranslatorException("Unable to find the Bing key.", Name);
        }

        // Unix timestamp generated once the page is loaded. Valid for 3600000 milliseconds or 1 hour
        if (!Utf8Parser.TryParse(bytes.Slice(keyStartIndex, keyLength), out long key, out _))
        {
            // This shouldn't happen, but we'll handle this case anyway
            key = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        int tokenStartIndex = keyStartIndex + keyLength + 2;
        int tokenLength = bytes[tokenStartIndex..].IndexOf((byte)'"');
        if (tokenLength == -1)
        {
            throw new TranslatorException("Unable to find the Bing token.", Name);
        }

        string token = Encoding.UTF8.GetString(html, tokenStartIndex, tokenLength);
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
        ArgumentNullException.ThrowIfNull(languageCode);

        return languageCode switch
        {
            // ReSharper disable StringLiteralTypo, CommentTypo
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
            // ReSharper restore StringLiteralTypo, CommentTypo
        };
    }

    private static void EnsureValidTextToSpeechLanguage(string language, out MicrosoftVoice voice)
    {
        if (!MicrosoftTranslator.DefaultVoices.TryGetValue(language, out var temp))
        {
            throw new ArgumentException($"Unable to get the voice from language {language}.", nameof(language));
        }

        voice = temp;
    }

    private static void ThrowIfStatusCodeIsPresent(JsonDocument document)
    {
        // If "statusCode" property is present, the response is not successful
        if (document.RootElement.ValueKind == JsonValueKind.Array || !document.RootElement.TryGetProperty("statusCode"u8, out _))
            return;

        var result = document.Deserialize(BingErrorResultModelContext.Default.BingErrorResultModel)!;
        string message = string.IsNullOrEmpty(result.Message) ? $"The API returned status code {(int)result.StatusCode}." : result.Message!;

#if NET5_0_OR_GREATER
        throw new HttpRequestException(message, null, result.StatusCode);
#else
        throw new HttpRequestException(message);
#endif
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
        if (!_cachedCredentials.IsExpired)
        {
            return _cachedCredentials.Value;
        }

        await _credentialsSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (!_cachedCredentials.IsExpired)
            {
                return _cachedCredentials.Value;
            }

            _cachedCredentials = await GetCredentialsAsync().ConfigureAwait(false);
        }
        finally
        {
            _credentialsSemaphore.Release();
        }

        return _cachedCredentials.Value;
    }
}