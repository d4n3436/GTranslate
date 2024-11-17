using GTranslate.Models;
using GTranslate.Results;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GTranslate.Translators;

/// <summary>
/// Represents the DeepL Translator.
/// </summary>
public sealed class DeepLTranslator : ITranslator, IDisposable
{
    private const string VersionName = "24.13.1";
    private const string VersionCode = "137";
    private const string OsVersion = "14";
    private const string DeviceModel = "Android SDK built for x86_64";
    private const string CpuAbi = "x86_64";

    private const string DefaultUserAgent = $"DeepL/{VersionName}({VersionCode}) Android {OsVersion} ({DeviceModel};{CpuAbi})";
    private static readonly Uri ApiEndpointUri = new("https://www2.deepl.com/jsonrpc");
    private static readonly Uri DeepLUri  = new("https://www.deepl.com/");

    private readonly string _instanceId = Guid.NewGuid().ToString();
    private readonly HttpClient _httpClient;
    private readonly DeepLTranslatorState _state;
    private bool _disposed;

    /// <summary>
    /// Gets the name of this translator.
    /// </summary>
    public string Name => nameof(DeepLTranslator);

    /// <summary>
    /// Initializes a new instance of the <see cref="DeepLTranslator"/> class.
    /// </summary>
    public DeepLTranslator()
        : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeepLTranslator"/> class with the provided <see cref="HttpClient"/> instance.
    /// </summary>
    /// <param name="httpClient">An <see cref="HttpClient"/> instance.</param>
    public DeepLTranslator(HttpClient httpClient)
        : this(httpClient, new DeepLTranslatorState())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeepLTranslator"/> class with the provided <see cref="HttpClient"/> instance and state.
    /// </summary>
    /// <param name="httpClient">An <see cref="HttpClient"/> instance.</param>
    /// <param name="state">The translator state.</param>
    public DeepLTranslator(HttpClient httpClient, DeepLTranslatorState state)
    {
        TranslatorGuards.NotNull(httpClient);
        TranslatorGuards.NotNull(state);

        httpClient.DefaultRequestHeaders.Add("X-Instance", _instanceId);
        httpClient.DefaultRequestHeaders.Add("X-Product", "translator");
        httpClient.DefaultRequestHeaders.Add("Client-Id", _instanceId);

        string traceId = Guid.NewGuid().ToString("N");
        string spanId = Guid.NewGuid().ToString("N")[..16];
        httpClient.DefaultRequestHeaders.Add("sentry-trace", $"{traceId}-{spanId}"); // https://develop.sentry.dev/sdk/telemetry/traces/#header-sentry-trace

        if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", DefaultUserAgent);
        }

        httpClient.DefaultRequestHeaders.Add("x-app-os-name", "Android");
        httpClient.DefaultRequestHeaders.Add("x-app-os-version", OsVersion);
        httpClient.DefaultRequestHeaders.Add("x-app-version", VersionName);
        httpClient.DefaultRequestHeaders.Add("x-app-build", VersionCode);
        httpClient.DefaultRequestHeaders.Add("x-app-device", DeviceModel);
        httpClient.DefaultRequestHeaders.Add("x-app-instance-id", _instanceId);
        httpClient.DefaultRequestHeaders.Referrer = DeepLUri;

        _httpClient = httpClient;
        _state = state;
    }

    /// <summary>
    /// Translates a text using DeepL Translator.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <param name="toLanguage">The target language.</param>
    /// <param name="fromLanguage">The source language.</param>
    /// <param name="cancellationToken">The cancellation token to use on this request.</param>
    /// <returns>A task that represents the asynchronous translation operation. The task contains the translation result.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="toLanguage"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="Language"/> could not be obtained from <paramref name="toLanguage"/> or <paramref name="fromLanguage"/>.</exception>
    /// <exception cref="TranslatorException">
    /// Thrown when <paramref name="toLanguage"/> or <paramref name="fromLanguage"/> are not supported, or an error occurred during the operation.
    /// </exception>
    public async Task<DeepLTranslationResult> TranslateAsync(string text, string toLanguage, string? fromLanguage = null, CancellationToken cancellationToken = default)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        cancellationToken.ThrowIfCancellationRequested();
        TranslatorGuards.LanguageFound(toLanguage, out var toLang, "Unknown target language.");
        TranslatorGuards.LanguageFound(fromLanguage, out var fromLang, "Unknown source language.");
        TranslatorGuards.LanguageSupported(this, toLang, fromLang);

        return await TranslateAsync(text, toLang, fromLang, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc cref="TranslateAsync(string, string, string, CancellationToken)"/>
    public async Task<DeepLTranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null, CancellationToken cancellationToken = default)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        cancellationToken.ThrowIfCancellationRequested();
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

        var request = new DeepLTranslatorRequest
        {
            Id = _state.RequestId,
            JsonRpc = "2.0",
            Method = "LMT_handle_jobs",
            Params = new DeepLParams
            {
                Jobs =
                [
                    new DeepLJob
                    {
                        Kind = "default",
                        PreferredNumBeams = 1,
                        RawEnContextAfter = [],
                        RawEnContextBefore = [],
                        Sentences =
                        [
                            new DeepLSentence
                            {
                                Id = 0,
                                Prefix = "",
                                Text = text
                            }
                        ]
                    }
                ],
                CommonJobParams = new DeepLCommonJobParams
                {
                    Mode = "translate",
                    RegionalVariant = "en-US"
                },
                Lang = new DeepLLang
                {
                    TargetLang = toLanguage.ISO6391,
                    SourceLangUserSelected = fromLanguage?.ISO6391 ?? "en" // "AUTODETECT"
                },
                Priority = 1,
                Timestamp = GetTimestamp(text)
            }
        };

        string traceId = Guid.NewGuid().ToString("N");
        string spanId = Guid.NewGuid().ToString("N")[..16];

        var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpointUri);
        message.Headers.Add("X-Trace-ID", traceId);
        message.Headers.Add("traceparent", $"00-{traceId}-{spanId}-01");
        message.Content = JsonContent.Create(request, DeepLTranslatorRequestContext.Default.DeepLTranslatorRequest);

        var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        var model = (await response.Content.ReadFromJsonAsync(DeepLTranslationResultModelContext.Default.DeepLTranslationResultModel, cancellationToken).ConfigureAwait(false))!;

        if (model.Error is not null)
        {
            throw new TranslatorException($"Received error {model.Error.Code} from response. Message: \"{model.Error.Message}\"", Name);
        }

        response.EnsureSuccessStatusCode();

        if (model.Result is null)
        {
            throw new TranslatorException("DeepL API returned an invalid response (perhaps something changed?)", Name);
        }

        return new DeepLTranslationResult(string.Join(" ", model.Result.Translations.Select(x => x.Beams[0].Sentences[0].Text)), text, Language.GetLanguage(toLanguage.ISO6391), Language.GetLanguage(model.Result.SourceLang));
    }

    /// <summary>
    /// Transliterates a text using DeepL Translator.
    /// </summary>
    /// <param name="text">The text to transliterate.</param>
    /// <param name="toLanguage">The target language.</param>
    /// <param name="fromLanguage">The source language.</param>
    /// <param name="cancellationToken">The cancellation token to use on this request.</param>
    /// <returns>A task that represents the asynchronous transliteration operation. The task contains the transliteration result.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="toLanguage"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown when a <see cref="Language"/> could not be obtained from <paramref name="toLanguage"/> or <paramref name="fromLanguage"/>.</exception>
    /// <exception cref="TranslatorException">
    /// Thrown when <paramref name="toLanguage"/> or <paramref name="fromLanguage"/> are not supported, or an error occurred during the operation.
    /// </exception>
    public async Task<YandexTransliterationResult> TransliterateAsync(string text, string toLanguage, string? fromLanguage = null, CancellationToken cancellationToken = default)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        cancellationToken.ThrowIfCancellationRequested();
        TranslatorGuards.LanguageFound(toLanguage, out var toLang, "Unknown target language.");
        TranslatorGuards.LanguageFound(fromLanguage, out var fromLang, "Unknown source language.");
        TranslatorGuards.LanguageSupported(this, toLang, fromLang);

        return await TransliterateAsync(text, toLang, fromLang, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc cref="TransliterateAsync(string, string, string, CancellationToken)"/>
    public async Task<YandexTransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null, CancellationToken cancellationToken = default)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        TranslatorGuards.NotNull(toLanguage);
        cancellationToken.ThrowIfCancellationRequested();
        TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

        throw new NotImplementedException();
    }

    /// <summary>
    /// Detects the language of a text using DeepL Translator.
    /// </summary>
    /// <param name="text">The text to detect its language.</param>
    /// <param name="cancellationToken">The cancellation token to use on this request.</param>
    /// <returns>A task that represents the asynchronous language detection operation. The task contains the detected language.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this translator has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <exception cref="TranslatorException">Thrown when an error occurred during the operation.</exception>
    public async Task<Language> DetectLanguageAsync(string text, CancellationToken cancellationToken = default)
    {
        TranslatorGuards.ObjectNotDisposed(this, _disposed);
        TranslatorGuards.NotNull(text);
        cancellationToken.ThrowIfCancellationRequested();

        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns whether DeepL Translator supports the specified language.
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

    /// <inheritdoc cref="TranslateAsync(string, string, string, CancellationToken)"/>
    async Task<ITranslationResult> ITranslator.TranslateAsync(string text, string toLanguage, string? fromLanguage)
        => await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

    /// <inheritdoc cref="TranslateAsync(string, ILanguage, ILanguage, CancellationToken)"/>
    async Task<ITranslationResult> ITranslator.TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage)
        => await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

    /// <inheritdoc cref="TransliterateAsync(string, string, string, CancellationToken)"/>
    async Task<ITransliterationResult> ITranslator.TransliterateAsync(string text, string toLanguage, string? fromLanguage)
        => await TransliterateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

    /// <inheritdoc cref="TransliterateAsync(string, ILanguage, ILanguage, CancellationToken)"/>
    async Task<ITransliterationResult> ITranslator.TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage)
        => await TransliterateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

    /// <inheritdoc cref="DetectLanguageAsync(string, CancellationToken)"/>
    async Task<ILanguage> ITranslator.DetectLanguageAsync(string text) => await DetectLanguageAsync(text).ConfigureAwait(false);

    /// <inheritdoc cref="IsLanguageSupported(Language)"/>
    bool ITranslator.IsLanguageSupported(ILanguage language) => language is Language lang && IsLanguageSupported(lang);

    private static long GetTimestamp(string text)
    {
        TranslatorGuards.NotNull(text);

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int count = 1;

        foreach (char c in text)
        {
            if (c == 'i')
                count++;
        }

        return timestamp - timestamp % count + count;
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
