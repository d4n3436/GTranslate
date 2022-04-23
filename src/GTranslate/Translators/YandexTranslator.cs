using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GTranslate.Extensions;
using GTranslate.Results;

namespace GTranslate.Translators;

/// <summary>
/// Represents the Yandex Translator.
/// </summary>
public sealed class YandexTranslator : ITranslator, IDisposable
{
    private const string _apiUrl = "https://translate.yandex.net/api/v1/tr.json";
    private const string _defaultUserAgent = "ru.yandex.translate/3.20.2024";
    private static readonly Uri _transliterationApiUri = new("https://translate.yandex.net/translit/translit");
    private static readonly HashSet<ILanguage> _ttsLanguages = new()
    {
        Language.GetLanguage("ru"),
        Language.GetLanguage("en"),
        Language.GetLanguage("tr")
    };

    /// <summary>
    /// Gets a read-only collection of languages that support text-to-speech.
    /// </summary>
    public static IReadOnlyCollection<ILanguage> TextToSpeechLanguages => _ttsLanguages;

    /// <inheritdoc/>
    public string Name => nameof(YandexTranslator);

    private readonly HttpClient _httpClient;
    private CachedObject<Guid> _cachedUcid;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="YandexTranslator"/> class.
    /// </summary>
    public YandexTranslator()
        : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YandexTranslator"/> class with the provided <see cref="HttpClient"/> instance.
    /// </summary>
    /// <param name="httpClient">An <see cref="HttpClient"/> instance.</param>
    public YandexTranslator(HttpClient httpClient)
    {
        TranslatorGuards.NotNull(httpClient);

        if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);
        }

        _httpClient = httpClient;
    }

    /// <summary>
    /// Translates a text using Yandex.Translate.
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
    public async Task<YandexTranslationResult> TranslateAsync(string text, string toLanguage, string? fromLanguage = null)
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
    public async Task<YandexTranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

        string query = $"?ucid={GetOrUpdateUcid():N}" +
                       "&srv=android" +
                       "&format=text";

        var data = new Dictionary<string, string>
        {
            { "text", text },
            { "lang", fromLanguage == null ? YandexHotPatch(toLanguage.ISO6391) : $"{YandexHotPatch(fromLanguage.ISO6391)}-{YandexHotPatch(toLanguage.ISO6391)}" }
        };

        using var content = new FormUrlEncodedContent(data);
        using var response = await _httpClient.PostAsync(new Uri($"{_apiUrl}/translate{query}"), content).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        using var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

        ThrowIfStatusCodeIsPresent(document.RootElement);

        var textProp = document.RootElement.GetProperty("text");
        if (textProp.ValueKind != JsonValueKind.Array)
        {
            throw new TranslatorException("Failed to get the translated text.", Name);
        }

        string translation = string.Concat(textProp.EnumerateArray().Select(x => x.GetString()));

        string language = document.RootElement.GetProperty("lang").GetString() ?? throw new TranslatorException("Failed to get the source language.", Name);
        int index = language.IndexOf('-');
        if (index == -1)
        {
            throw new TranslatorException("Failed to get the source language.", Name);
        }

        string source = language.Substring(0, index);
        string target = language.Substring(index + 1, language.Length - index - 1);

        return new YandexTranslationResult(translation, text, Language.GetLanguage(target), Language.GetLanguage(source));
    }

    /// <summary>
    /// Transliterates a text using Yandex.Translate.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <param name="toLanguage">The target language.</param>
    /// <param name="fromLanguage">The source language.</param>
    /// <returns>A task that represents the asynchronous transliteration operation. The task contains the transliteration result.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="toLanguage"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="Language"/> could not be obtained from <paramref name="toLanguage"/> or <paramref name="fromLanguage"/>.</exception>
    /// <exception cref="TranslatorException">
    /// Thrown when <paramref name="toLanguage"/> or <paramref name="fromLanguage"/> are not supported, or an error occurred during the operation.
    /// </exception>
    public async Task<YandexTransliterationResult> TransliterateAsync(string text, string toLanguage, string? fromLanguage = null)
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
    public async Task<YandexTransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

        // It seems like the source language is required for transliterations
        fromLanguage ??= await DetectLanguageAsync(text).ConfigureAwait(false);

        var data = new Dictionary<string, string>
        {
            { "text", text },
            { "lang", $"{YandexHotPatch(fromLanguage.ISO6391)}-{YandexHotPatch(toLanguage.ISO6391)}" }
        };

        using var content = new FormUrlEncodedContent(data);
        using var response = await _httpClient.PostAsync(_transliterationApiUri, content).ConfigureAwait(false);
        string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new TranslatorException(result, Name);
        }

        response.EnsureSuccessStatusCode();
        var target = Language.GetLanguage(toLanguage.ISO6391);
        var source = Language.GetLanguage(fromLanguage.ISO6391);

        return new YandexTransliterationResult(JsonSerializer.Deserialize<string>(result)!, text, target, source);
    }

    /// <summary>
    /// Detects the language of a text using Yandex.Translate.
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

        string query = $"?ucid={GetOrUpdateUcid():N}" +
                       "&srv=android" +
                       "&format=text";

        var data = new Dictionary<string, string>
        {
            { "text", text },
            { "hint", "en" }
        };

        using var content = new FormUrlEncodedContent(data);
        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{_apiUrl}/detect{query}"),
            Content = content
        };

        using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        using var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

        ThrowIfStatusCodeIsPresent(document.RootElement);

        string? language = document.RootElement.GetProperty("lang").GetString();
        if (language is null || !Language.TryGetLanguage(language, out var lang))
        {
            throw new TranslatorException("Failed to get the detected language.", Name);
        }

        return lang;
    }

    /// <summary>
    /// Converts text into synthesized speech.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="language">The voice language. Only the languages in <see cref="TextToSpeechLanguages"/> are supported.</param>
    /// <param name="speed">The rate (speed) of synthesized speech.</param>
    /// <returns>A task that represents the asynchronous synthesis operation. The task contains the synthesized speech in a MP3 <see cref="Stream"/>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="language"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="Language"/> could not be obtained from <paramref name="language"/>.</exception>
    /// <exception cref="TranslatorException">Thrown when <paramref name="language"/> is not supported.</exception>
    public async Task<Stream> TextToSpeechAsync(string text, string language, float speed = 1)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(language);
        TranslatorGuards.LanguageFound(language, out var lang);
        EnsureValidTTSLanguage(lang);

        return await TextToSpeechAsync(text, lang, speed).ConfigureAwait(false);
    }

    /// <inheritdoc cref="TextToSpeechAsync(string, string, float)"/>
    public async Task<Stream> TextToSpeechAsync(string text, ILanguage language, float speed = 1)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(language);
        EnsureValidTTSLanguage(language);

        string url = $"https://tts.voicetech.yandex.net/tts?text={Uri.EscapeDataString(text)}&lang={YandexTTSHotPatch(language.ISO6391)}&speed={speed}&format=mp3&quality=hi&platform=android&application=translate";
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Returns whether Yandex.Translate supports the specified language.
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

        return (language.SupportedServices & TranslationServices.Yandex) == TranslationServices.Yandex;
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

    /// <summary>
    /// Hot-patches language codes to Yandex-specific ones.
    /// </summary>
    /// <param name="languageCode">The language code.</param>
    /// <returns>The hot-patched language code.</returns>
    private static string YandexHotPatch(string languageCode)
    {
        TranslatorGuards.NotNull(languageCode);

        return languageCode switch
        {
            "zh-CN" => "zh",
            _ => languageCode
        };
    }

    private static string YandexTTSHotPatch(string languageCode)
    {
        TranslatorGuards.NotNull(languageCode);

        return languageCode switch
        {
            "ru" => "ru_RU",
            "en" => "en_GB",
            "tr" => "tr_TR",
            _ => throw new ArgumentException("Unknown language.", nameof(languageCode))
        };
    }

    private static void ThrowIfStatusCodeIsPresent(JsonElement element)
    {
        if (element.TryGetInt32("code", out int code) && code != 200)
        {
            string message = element.GetPropertyOrDefault("message").GetStringOrDefault($"The API returned status code {code}.");

#if NET5_0_OR_GREATER
            throw new HttpRequestException(message, null, (HttpStatusCode)code);
#else
            throw new HttpRequestException(message);
#endif
        }
    }

    private static void EnsureValidTTSLanguage(ILanguage language)
    {
        if (!_ttsLanguages.Contains(language))
        {
            throw new ArgumentException("Language not supported.", nameof(language));
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
        _disposed = true;
    }

    private Guid GetOrUpdateUcid()
    {
        if (_cachedUcid.IsExpired())
        {
            _cachedUcid = new CachedObject<Guid>(Guid.NewGuid(), TimeSpan.FromSeconds(360));
        }

        return _cachedUcid.Value;
    }
}