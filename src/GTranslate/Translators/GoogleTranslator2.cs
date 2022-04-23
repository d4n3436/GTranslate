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
/// Represents a translator for the new Google Translate RPC API.
/// </summary>
public sealed class GoogleTranslator2 : ITranslator, IDisposable
{
    private const string _translateRpcId = "MkEWBc";
    private const string _ttsRpcId = "jQ1olc";
    private const string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36";
    private static readonly Uri _defaultBaseAddress = new("https://translate.google.com/");
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
    public string Name => nameof(GoogleTranslator2);

    private readonly HttpClient _httpClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleTranslator2"/> class.
    /// </summary>
    public GoogleTranslator2()
        : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleTranslator2"/> class with the provided <see cref="HttpClient"/> instance.
    /// </summary>
    /// <param name="httpClient">An <see cref="HttpClient"/> instance.</param>
    public GoogleTranslator2(HttpClient httpClient)
    {
        TranslatorGuards.NotNull(httpClient);

        if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);
        }

        httpClient.BaseAddress ??= _defaultBaseAddress;

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

    // TODO: Decipher request header X-Goog-BatchExecute-Bgr, this produces a more accurate translation

    /// <inheritdoc cref="TranslateAsync(string, string, string)"/>
    public async Task<GoogleTranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

        string payload = $"[[\"{JsonEncodedText.Encode(text)}\",\"{fromLanguage?.ISO6391 ?? "auto"}\",\"{toLanguage.ISO6391}\",true],[null]]";

        using var request = BuildRequest(_translateRpcId, payload);
        using var document = await SendAndParseResponseAsync(request).ConfigureAwait(false);

        var root = document.RootElement;

        string target = root[1][1].GetString() ?? toLanguage.ISO6391;
        string source = root[1][3].GetString() ?? string.Empty;
        
        if (source == "auto")
        {
            source = root.ElementAtOrDefault(2).GetStringOrDefault()
                     ?? "en"; // Source language is not present, this happens when the text is a hyperlink and fromLanguage is null
        }

        string translation;
        var chunks = root[1][0][0]
            .EnumerateArray()
            .FirstOrDefault(x => x.ValueKind == JsonValueKind.Array);

        if (chunks.ValueKind == JsonValueKind.Array)
        {
            translation = string.Join(" ", chunks.EnumerateArray().Select(x => x.FirstOrDefault().GetString()));
        }
        else
        {
            // no chunks found, could be a link or gender-specific translation
            // should we provide the value of the link and the gender-specific translations in separate properties?
            translation = root[1][0][0][0].GetString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(translation))
        {
            throw new TranslatorException("Failed to get the translated text.", Name);
        }

        string? transliteration = root[1][0][0]
            .ElementAtOrDefault(1)
            .GetStringOrDefault() ?? root
            .FirstOrDefault()
            .FirstOrDefault()
            .GetStringOrDefault();

        return new GoogleTranslationResult(translation, text, Language.GetLanguage(target), Language.GetLanguage(source), transliteration, null, Name);
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

        return new GoogleTransliterationResult(result.Transliteration!, text, result.TargetLanguage, result.SourceLanguage, Name);
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
    /// <param name="slow">Whether to read the text slowly.</param>
    /// <returns>A task that represents the asynchronous synthesis operation. The task contains the synthesized speech in a MP3 <see cref="Stream"/>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="language"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="Language"/> could not be obtained from <paramref name="language"/>.</exception>
    /// <exception cref="TranslatorException">Thrown when <paramref name="language"/> is not supported.</exception>
    public async Task<Stream> TextToSpeechAsync(string text, string language, bool slow = false)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(language);
        TranslatorGuards.LanguageFound(language, out var lang);

        return await TextToSpeechAsync(text, lang, slow).ConfigureAwait(false);
    }

    /// <inheritdoc cref="TextToSpeechAsync(string, string, bool)"/>
    public async Task<Stream> TextToSpeechAsync(string text, ILanguage language, bool slow = false)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(language);
        EnsureValidTTSLanguage(language);

        var tasks = text.SplitWithoutWordBreaking().Select(ProcessRequestAsync);

        // Send requests and parse responses in parallel
        var chunks = await Task.WhenAll(tasks).ConfigureAwait(false);

        return chunks.AsReadOnlySequence().AsStream();

        async Task<ReadOnlyMemory<byte>> ProcessRequestAsync(ReadOnlyMemory<char> textChunk)
        {
            string payload = $"[\"{JsonEncodedText.Encode(textChunk.Span)}\",\"{language.ISO6391}\",{(slow ? "true" : "null")},\"null\"]";
            using var request = BuildRequest(_ttsRpcId, payload);
            using var document = await SendAndParseResponseAsync(request).ConfigureAwait(false);

            return document.RootElement[0].GetBytesFromBase64();
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

    private static HttpRequestMessage BuildRequest(string rpcId, string payload) => new()
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri($"_/TranslateWebserverUi/data/batchexecute?rpcids={rpcId}", UriKind.Relative),
        Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[] { new("f.req", $"[[[\"{rpcId}\",\"{JsonEncodedText.Encode(payload)}\",null,\"generic\"]]]") })
    };

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

    private async Task<JsonDocument> SendAndParseResponseAsync(HttpRequestMessage request)
    {
        using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        JsonDocument document;

        // skip magic chars
        if (stream.CanSeek)
        {
            stream.Seek(6, SeekOrigin.Begin);
            document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
        }
        else
        {
            byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            document = JsonDocument.Parse(bytes.AsMemory(6, bytes.Length - 6));
        }

        // get the actual data
        string data = document.RootElement[0][2].GetString() ?? throw new TranslatorException("Unable to get the data from the response.", Name);
        document.Dispose();

        return JsonDocument.Parse(data);
    }
}