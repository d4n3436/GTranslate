using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GTranslate.Models;
using GTranslate.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GTranslate.Translators
{
    /// <summary>
    /// Represents the Google Translator.
    /// </summary>
    public class GoogleTranslator : ITranslator, IDisposable
    {
        /// <summary>
        /// Returns the default API endpoint.
        /// </summary>
        public const string DefaultApiEndpoint = "https://clients5.google.com/translate_a/t";

        /// <summary>
        /// Returns the default User-Agent header.
        /// </summary>
        public const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36";

        /// <inheritdoc/>
        public string Name => "GoogleTranslator";

        private readonly HttpClient _httpClient;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleTranslator"/> class.
        /// </summary>
        public GoogleTranslator() : this(new HttpClient())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleTranslator"/> class with the provided <see cref="HttpClient"/>.
        /// </summary>
        public GoogleTranslator(HttpClient httpClient)
        {
            TranslatorGuards.NotNull(httpClient, nameof(httpClient));

            if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultUserAgent);
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
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="TranslatorException"/>
        public async Task<GoogleTranslationResult> TranslateAsync(string text, string toLanguage, string fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageFound(toLanguage, fromLanguage, out var toLang, out var fromLang);
            TranslatorGuards.LanguageSupported(this, toLang, fromLang);

            return await TranslateAsync(text, toLang, fromLang).ConfigureAwait(false);
        }

        /// <inheritdoc cref="TranslateAsync(string, string, string)"/>
        public async Task<GoogleTranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

            string query = "?client=dict-chrome-ex" +
                           $"&sl={GoogleHotPatch(fromLanguage?.ISO6393) ?? "auto"}" +
                           $"&tl={GoogleHotPatch(toLanguage.ISO6391)}" +
                           $"&q={Uri.EscapeDataString(text)}";

            string json = await _httpClient.GetStringAsync(new Uri($"{DefaultApiEndpoint}{query}")).ConfigureAwait(false);
            try
            {
                var model = JsonConvert.DeserializeObject<GoogleTranslationModel>(json);
                if (model == null)
                {
                    throw new TranslatorException("The API returned an empty response.", Name);
                }

                var alts = new List<GoogleAlternativeTranslation>();
                if (model.AlternativeTranslations.Count > 0)
                {
                    foreach (var alt in model.AlternativeTranslations[0].Alternative)
                    {
                        alts.Add(new GoogleAlternativeTranslation(alt.WordPostProcess, alt.Score));
                    }
                }

                var ld = new List<GoogleLanguageDetection>();
                var modelLd = model.LanguageDetection;

                for (int i = 0; i < modelLd.SourceLanguages.Count; i++)
                {
                    ld.Add(new GoogleLanguageDetection(model.LanguageDetection.SourceLanguages[i], modelLd.SourceLanguageConfidences[i]));
                }

                return new GoogleTranslationResult(string.Concat(model.Sentences.Select(x => x.Translation)), text, toLanguage as Language,
                    Language.GetLanguage(model.Source), string.Concat(model.Sentences.Select(x => x.Transliteration)), model.Confidence, alts, ld);
            }
            catch (JsonSerializationException)
            {
                var response = JToken.Parse(json)
                .FirstOrDefault()?
                .FirstOrDefault();

                string translation = response?
                    .FirstOrDefault()?
                    .FirstOrDefault()?
                    .FirstOrDefault()?
                    .ToString();

                if (translation == null)
                {
                    throw new TranslatorException("Error parsing the translation response.", Name);
                }

                string sourceLanguage = response
                    .ElementAtOrDefault(2)?
                    .ToString();

                return new GoogleTranslationResult(translation, text, toLanguage as Language, Language.GetLanguage(sourceLanguage));
            }
        }

        /// <summary>
        /// Transliterates a text using Google Translate.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <param name="toLanguage">The target language.</param>
        /// <param name="fromLanguage">The source language.</param>
        /// <returns>A task that represents the asynchronous transliteration operation. The task contains the transliteration result.</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="TranslatorException"/>
        public async Task<GoogleTransliterationResult> TransliterateAsync(string text, string toLanguage, string fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageFound(toLanguage, fromLanguage, out var toLang, out var fromLang);
            TranslatorGuards.LanguageSupported(this, toLang, fromLang);

            return await TransliterateAsync(text, toLang, fromLang).ConfigureAwait(false);
        }

        /// <inheritdoc cref="TransliterateAsync(string, string, string)"/>
        public async Task<GoogleTransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage fromLanguage = null)
        {
            var result = await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);
            if (string.IsNullOrEmpty(result.Transliteration))
            {
                throw new TranslatorException("Failed to get the transliteration.", Name);
            }

            return new GoogleTransliterationResult(result.Transliteration, text, result.TargetLanguage, result.SourceLanguage);
        }

        /// <summary>
        /// Detects the language of a text using Google Translate.
        /// </summary>
        /// <param name="text">The text to detect its language.</param>
        /// <returns>A task that represents the asynchronous language detection operation. The task contains the detected language.</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="TranslatorException"/>
        public async Task<Language> DetectLanguageAsync(string text)
        {
            var result = await TranslateAsync(text, "en").ConfigureAwait(false);
            return result.SourceLanguage;
        }

        /// <summary>
        /// Returns whether Google Translate supports the specified language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns><see langword="true"/> if the language is supported, otherwise <see langword="false"/>.</returns>
        public bool IsLanguageSupported(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                throw new ArgumentNullException(nameof(language));
            }

            return Language.TryGetLanguage(language, out var lang) && IsLanguageSupported(lang);
        }

        /// <inheritdoc cref="IsLanguageSupported(string)"/>
        public bool IsLanguageSupported(Language language)
        {
            if (language == null)
            {
                throw new ArgumentNullException(nameof(language));
            }

            return (language.SupportedServices & TranslationServices.Google) == TranslationServices.Google;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="TranslateAsync(string, string, string)"/>
        async Task<ITranslationResult> ITranslator.TranslateAsync(string text, string toLanguage, string fromLanguage)
            => await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

        /// <inheritdoc cref="TranslateAsync(string, ILanguage, ILanguage)"/>
        async Task<ITranslationResult> ITranslator.TranslateAsync(string text, ILanguage toLanguage, ILanguage fromLanguage)
            => await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

        /// <inheritdoc cref="TransliterateAsync(string, string, string)"/>
        async Task<ITransliterationResult> ITranslator.TransliterateAsync(string text, string toLanguage, string fromLanguage)
            => await TransliterateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

        /// <inheritdoc cref="TransliterateAsync(string, ILanguage, ILanguage)"/>
        async Task<ITransliterationResult> ITranslator.TransliterateAsync(string text, ILanguage toLanguage, ILanguage fromLanguage)
            => await TransliterateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);

        /// <inheritdoc cref="DetectLanguageAsync(string)"/>
        async Task<ILanguage> ITranslator.DetectLanguageAsync(string text) => await DetectLanguageAsync(text).ConfigureAwait(false);

        /// <inheritdoc cref="IsLanguageSupported(Language)"/>
        bool ITranslator.IsLanguageSupported(ILanguage language) => language is Language lang && IsLanguageSupported(lang);

        /// <inheritdoc cref="Dispose()"/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;

            _httpClient.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Hot-patches language codes to Google-specific ones.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <returns>The hot-patched language code.</returns>
        private static string GoogleHotPatch(string languageCode)
        {
            switch (languageCode)
            {
                case "jv":
                    return "jw";

                default:
                    return languageCode;
            }
        }
    }
}