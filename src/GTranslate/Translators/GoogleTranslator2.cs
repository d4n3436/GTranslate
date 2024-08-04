using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GTranslate.Extensions;
using GTranslate.Models;
using GTranslate.Results;

namespace GTranslate.Translators;

/// <summary>
/// Represents a translator for the new Google Translate RPC API.
/// </summary>
public sealed class GoogleTranslator2 : ITranslator, IDisposable
{
    private const string TranslateRpcId = "MkEWBc";
    private const string TtsRpcId = "jQ1olc";
    private static readonly Uri DefaultBaseAddress = new("https://translate.google.com/");

    private static readonly string[] TtsLanguages =
    [
        "af", "am", "ar", "bg", "bn", "bs", "ca", "cs", "cy", "da", "de", "el", "en", "eo", "es", "et", "eu", "fi", "fr", "gl", "gu", "ha", "hi", "hr",
        "hu", "hy", "id", "is", "it", "iw", "ja", "jv", "km", "kn", "ko", "la", "lt", "lv", "mk", "ml", "mr", "ms", "my", "ne", "nl", "no", "pa", "pl",
        "pt", "pt-PT", "ro", "ru", "si", "sk", "sq", "sr", "su", "sv", "sw", "ta", "te", "th", "tl", "tr", "uk", "ur", "vi", "yue", "zh-CN", "zh-TW"
    ];

    private static readonly Lazy<HashSet<ILanguage>> LazyTtsLanguages = new(() => [..TtsLanguages.Select(Language.GetLanguage)]);

    /// <summary>
    /// Gets a read-only collection of languages that support text-to-speech.
    /// </summary>
    public static IReadOnlyCollection<ILanguage> TextToSpeechLanguages => LazyTtsLanguages.Value;

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
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.DefaultUserAgent);
        }

        httpClient.BaseAddress ??= DefaultBaseAddress;

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

        object[] payload = [new object[] { text, GoogleHotPatch(fromLanguage?.ISO6391 ?? "auto"), GoogleHotPatch(toLanguage.ISO6391), 1 }, Array.Empty<object>()];
        using var request = BuildRequest(TranslateRpcId, payload);
        using var document = await SendAndParseResponseAsync(request).ConfigureAwait(false);

        var root = document.RootElement;

        string target = root[1][1].GetString() ?? toLanguage.ISO6391;
        string source = root[1][3].GetString()!;
        
        if (source == "auto")
        {
            source = root.GetArrayLength() > 2
                ? root[2].GetString()!
                : "en"; // Source language is not present, this happens when the text is a hyperlink and fromLanguage is null
        }

        string translation;
        if (root[1][0][0].GetArrayLength() > 5 && root[1][0][0][5].ValueKind == JsonValueKind.Array)
        {
            translation = string.Join(" ", root[1][0][0][5].EnumerateArray().Select(x => x[0].GetString()));
        }
        else
        {
            // no chunks found, could be a link or gender-specific translation
            // should we provide the value of the link and the gender-specific translations in separate properties?
            translation = root[1][0][0][0].GetString()!;
        }

        string? targetTransliteration = root[1][0][0][1].GetString();
        string? sourceTransliteration = root[0].ValueKind == JsonValueKind.Array ? root[0][0].GetString() : null;

        return new GoogleTranslationResult(translation, text, Language.GetLanguage(target), Language.GetLanguage(source), targetTransliteration, sourceTransliteration, null, Name);
    }

    /// <summary>
    /// Transliterates a text using Google Translate.
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

        return new GoogleTransliterationResult(result.Transliteration!, result.SourceTransliteration, text, result.TargetLanguage, result.SourceLanguage, Name);
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
        
        return chunks.AsSpan().AsReadOnlySequence().AsStream();
        
        async Task<ReadOnlyMemory<byte>> ProcessRequestAsync(ReadOnlyMemory<char> textChunk)
        {
            object?[] payload = [textChunk.ToString(), language.ISO6391, null, "undefined", new object[] { slow ? 1 : 0 }];
            using var request = BuildRequest(TtsRpcId, payload);
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
        if (!LazyTtsLanguages.Value.Contains(language))
        {
            throw new ArgumentException("Language not supported.", nameof(language));
        }
    }

    private static HttpRequestMessage BuildRequest(string rpcId, object?[] payload)
    {
        var serializedPayload = JsonSerializer.Serialize(payload, ObjectArrayContext.Default.ObjectArray!);
        object?[][][] request = [[[rpcId, serializedPayload, null, "generic"]]];

        return new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"_/TranslateWebserverUi/data/batchexecute?rpcids={rpcId}", UriKind.Relative),
            Content = new FormUrlEncodedContent([new KeyValuePair<string, string>("f.req", JsonSerializer.Serialize(request, ObjectArrayContext.Default.ObjectArrayArrayArray!))])
        };
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
            "mni" => "mni-Mtei",
            _ => languageCode
        };
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

    private async Task<JsonDocument> SendAndParseResponseAsync(HttpRequestMessage request)
    {
        using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        using var document = await GetJsonDocumentAsync(response).ConfigureAwait(false);

        // get the actual data
        string data = document.RootElement[0][2].GetString() ?? throw new TranslatorException("Unable to get the data from the response.", Name);
        return JsonDocument.Parse(data);
    }

    private static async Task<JsonDocument> GetJsonDocumentAsync(HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        // skip magic chars
        if (stream.CanSeek)
        {
            stream.Seek(6, SeekOrigin.Begin);
            return await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
        }

        byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        return JsonDocument.Parse(bytes.AsMemory(6, bytes.Length - 6));
    }
}