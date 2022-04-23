using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GTranslate.Extensions;
using GTranslate.Results;

namespace GTranslate.Translators;

/// <summary>
/// Represents a translator that uses the old (previous) Google Translate API.
/// </summary>
public sealed class GoogleTranslator : ITranslator, IDisposable
{
    private const string _salt1 = "+-a^+6";
    private const string _salt2 = "+-3^+b+-f";
    private const string _apiEndpoint = "https://translate.googleapis.com/translate_a/single";
    private const string _ttsApiEndpoint = "https://translate.google.com/translate_tts";
    private const string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36";
    private static readonly string[] _ttsLanguages =
    {
        "af", "ar", "bg", "bn", "bs", "ca", "cs", "cy", "da", "de", "el", "en", "eo", "es", "et", "fi", "fr", "gu", "hi", "hr", "hu",
        "hy", "id", "is", "it", "iw", "ja", "jv", "km", "kn", "ko", "la", "lv", "mk", "ml", "mr", "ms", "my", "ne", "nl", "no", "pl",
        "pt", "ro", "ru", "si", "sk", "sq", "sr", "su", "sv", "sw", "ta", "te", "th", "tl", "tr", "uk", "ur", "vi", "zh-CN", "zh-TW"
    };

    private static readonly Lazy<HashSet<ILanguage>> _lazyTtsLanguages = new(() => new HashSet<ILanguage>(_ttsLanguages.Select(Language.GetLanguage)));

    /// <summary>
    /// Gets a read-only collection of languages that support text-to-speech.
    /// </summary>
    public static IReadOnlyCollection<ILanguage> TextToSpeechLanguages => _lazyTtsLanguages.Value;

    /// <inheritdoc/>
    public string Name => nameof(GoogleTranslator);

    private readonly HttpClient _httpClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleTranslator"/> class.
    /// </summary>
    public GoogleTranslator()
        : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleTranslator"/> class with the provided <see cref="HttpClient"/> instance.
    /// </summary>
    /// <param name="httpClient">An <see cref="HttpClient"/> instance.</param>
    public GoogleTranslator(HttpClient httpClient)
    {
        TranslatorGuards.NotNull(httpClient);

        if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);
        }

        _httpClient = httpClient;
    }

    /// <summary>
    /// Translates a text using Google Translate.
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
    public async Task<GoogleTranslationResult> TranslateAsync(string text, string toLanguage, string? fromLanguage = null)
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
    public async Task<GoogleTranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

        string query = "?client=gtx" +
                       $"&sl={GoogleHotPatch(fromLanguage?.ISO6391 ?? "auto")}" +
                       $"&tl={GoogleHotPatch(toLanguage.ISO6391)}" +
                       "&dt=t" +
                       "&dt=bd" +
                       "&dj=1" +
                       "&source=input" +
                       $"&tk={MakeToken(text.AsSpan())}";

        using var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[] { new("q", text) });
        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{_apiEndpoint}{query}"),
            Content = content
        };

        using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

        var sentences = document.RootElement.GetProperty("sentences");
        if (sentences.ValueKind != JsonValueKind.Array)
        {
            throw new TranslatorException("Failed to get the translated text.", Name);
        }

        string translation = string.Concat(sentences.EnumerateArray().Select(x => x.GetProperty("trans").GetString()));
        string transliteration = string.Concat(sentences.EnumerateArray().Select(x => x.GetPropertyOrDefault("translit").GetStringOrDefault()));
        string source = document.RootElement.GetProperty("src").GetString() ?? string.Empty;
        float? confidence = document.RootElement.TryGetSingle("confidence", out float temp) ? temp : null;

        return new GoogleTranslationResult(translation, text, Language.GetLanguage(toLanguage.ISO6391), Language.GetLanguage(source), transliteration, confidence);
    }

    /// <summary>
    /// Transliterates a text using Google Translate.
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
    public async Task<GoogleTransliterationResult> TransliterateAsync(string text, string toLanguage, string? fromLanguage = null)
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
    public async Task<GoogleTransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
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

        return new GoogleTransliterationResult(result.Transliteration!, text, result.TargetLanguage, result.SourceLanguage);
    }

    /// <summary>
    /// Detects the language of a text using Google Translate.
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
    /// <param name="language">The voice language. Only the languages in <see cref="TextToSpeechLanguages"/> are supported.</param>
    /// /// <param name="speed">The rate (speed) of synthesized speech. Google uses <c>1</c> for normal speed and <c>0.3</c> for slow speed.</param>
    /// <returns>A task that represents the asynchronous synthesis operation. The task contains the synthesized speech in a MP3 <see cref="Stream"/>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="language"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="Language"/> could not be obtained from <paramref name="language"/>.</exception>
    /// <exception cref="TranslatorException">Thrown when <paramref name="language"/> is not supported, or an error occurred during the operation.</exception>
    public async Task<Stream> TextToSpeechAsync(string text, string language, float speed = 1)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(language);
        TranslatorGuards.LanguageFound(language, out var lang);

        return await TextToSpeechAsync(text, lang, speed).ConfigureAwait(false);
    }

    /// <inheritdoc cref="TextToSpeechAsync(string, string, float)"/>
    public async Task<Stream> TextToSpeechAsync(string text, ILanguage language, float speed = 1)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(language);
        EnsureValidTTSLanguage(language);

        var textParts = text.SplitWithoutWordBreaking().ToArray();
        var tasks = new Task<ReadOnlyMemory<byte>>[textParts.Length];
        for (var i = 0; i < textParts.Length; i++)
        {
            tasks[i] = ProcessRequestAsync(textParts[i], i, textParts.Length);
        }

        // Send requests and parse responses in parallel
        var chunks = await Task.WhenAll(tasks).ConfigureAwait(false);

        return chunks.AsReadOnlySequence().AsStream();

        async Task<ReadOnlyMemory<byte>> ProcessRequestAsync(ReadOnlyMemory<char> textChunk, int index, int total)
        {
            string escapedText = Uri.EscapeDataString(textChunk.ToString());
            string token = MakeToken(textChunk.Span);

            string url = $"{_ttsApiEndpoint}?ie=UTF-8&q={escapedText}&tl={language.ISO6391}&ttsspeed={speed}&total={total}&idx={index}&client=tw-ob&textlen={textChunk.Length}&tk={token}";
            return await _httpClient.GetByteArrayAsync(new Uri(url)).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns whether Google Translate supports the specified language.
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

        return (language.SupportedServices & TranslationServices.Google) == TranslationServices.Google;
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

    private static void EnsureValidTTSLanguage(ILanguage language)
    {
        if (!_lazyTtsLanguages.Value.Contains(language))
        {
            throw new ArgumentException("Language not supported.", nameof(language));
        }
    }

    /// <summary>
    /// Hot-patches language codes to Google-specific ones.
    /// </summary>
    /// <param name="languageCode">The language code.</param>
    /// <returns>The hot-patched language code.</returns>
    private static string GoogleHotPatch(string languageCode)
    {
        return languageCode switch
        {
            "jv" => "jw",
            _ => languageCode
        };
    }

    private static string MakeToken(ReadOnlySpan<char> text)
    {
        long a = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 3600, b = a;

        foreach (char ch in text)
        {
            a = WorkToken(a + ch, _salt1);
        }

        a = WorkToken(a, _salt2);

        if (a < 0)
        {
            a = (a & int.MaxValue) + int.MaxValue + 1;
        }

        a %= 1000000;

        return $"{a}.{a ^ b}";
    }

    private static long WorkToken(long num, string seed)
    {
        for (int i = 0; i < seed.Length - 2; i += 3)
        {
            int d = seed[i + 2];

            if (d >= 'a')
            {
                d -= 'W';
            }

            if (seed[i + 1] == '+')
            {
                num = (num + (num >> d)) & uint.MaxValue;
            }
            else
            {
                num ^= num << d;
            }
        }

        return num;
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
}