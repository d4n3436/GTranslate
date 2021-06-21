using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GTranslate.Models;
using GTranslate.Results;
using Newtonsoft.Json;

namespace GTranslate.Translators
{
    /// <summary>
    /// Represents the Yandex Translator.
    /// </summary>
    public class YandexTranslator : ITranslator, IDisposable
    {
        /// <summary>
        /// Returns the default API endpoint.
        /// </summary>
        public const string DefaultApiUrl = "http://translate.yandex.net/api/v1/tr.json";

        /// <summary>
        /// Returns the default User-Agent header.
        /// </summary>
        public const string DefaultUserAgent = "ru.yandex.translate/3.20.2024";

        /// <inheritdoc/>
        public string Name => "YandexTranslator";

        private readonly HttpClient _httpClient = new HttpClient();
        private CachedObject<string> _cachedUcid;
        private readonly string _apiUrl;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BingTranslator"/> class.
        /// </summary>
        public YandexTranslator()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultUserAgent);
            _apiUrl = DefaultApiUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BingTranslator"/> class with the provided API endpoint.
        /// </summary>
        public YandexTranslator(string apiUrl)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultUserAgent);
            _apiUrl = apiUrl;
        }

        /// <summary>
        /// Translates a text using Yandex.Translate.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <param name="toLanguage">The target language.</param>
        /// <param name="fromLanguage">The source language.</param>
        /// <returns>A task that represents the asynchronous translation operation. The task contains the translation result.</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="TranslatorException"/>
        public async Task<YandexTranslationResult> TranslateAsync(string text, string toLanguage, string fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageFound(toLanguage, fromLanguage, out var toLang, out var fromLang);
            TranslatorGuards.LanguageSupported(this, toLang, fromLang);

            return await TranslateAsync(text, toLang, fromLang).ConfigureAwait(false);
        }

        /// <inheritdoc cref="TranslateAsync(string, string, string)"/>
        public async Task<YandexTranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

            string query = $"?ucid={GetOrUpdateUcid()}" +
                           "&srv=android" +
                           "&format=text";

            var data = new Dictionary<string, string>
            {
                { "text", text },
                { "lang", fromLanguage == null ? toLanguage.ISO6391 : $"{fromLanguage.ISO6391}-{toLanguage.ISO6391}" }
            };

            string json;
            using (var content = new FormUrlEncodedContent(data))
            {
                var response = await _httpClient.PostAsync(new Uri($"{_apiUrl}/translate{query}"), content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var model = JsonConvert.DeserializeObject<YandexTranslationModel>(json);
            if (model == null)
            {
                throw new TranslatorException("The API returned an empty response.", Name);
            }

            var split = model.Lang?.Split('-');
            if (split == null || split.Length < 2)
            {
                throw new TranslatorException("Unable to parse the result language codes.", Name);
            }

            return new YandexTranslationResult(string.Concat(model.Text), text, Language.GetLanguage(split[1]), Language.GetLanguage(split[0]));
        }

        /// <summary>
        /// Transliterates a text using Yandex.Translate.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <param name="toLanguage">The target language.</param>
        /// <param name="fromLanguage">The source language.</param>
        /// <returns>A task that represents the asynchronous transliteration operation. The task contains the transliteration result.</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="TranslatorException"/>
        public async Task<YandexTransliterationResult> TransliterateAsync(string text, string toLanguage, string fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageFound(toLanguage, fromLanguage, out var toLang, out var fromLang);
            TranslatorGuards.LanguageSupported(this, toLang, fromLang);

            return await TransliterateAsync(text, toLang, fromLang).ConfigureAwait(false);
        }

        /// <inheritdoc cref="TransliterateAsync(string, string, string)"/>
        public async Task<YandexTransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

            // It seems like the source language is required for transliterations
            fromLanguage = fromLanguage ?? await DetectLanguageAsync(text).ConfigureAwait(false);

            var data = new Dictionary<string, string>
            {
                { "text", text },
                { "lang", $"{fromLanguage.ISO6391}-{toLanguage.ISO6391}" }
            };

            string transliteration;
            using (var content = new FormUrlEncodedContent(data))
            {
                var response = await _httpClient.PostAsync(new Uri("https://translate.yandex.net/translit/translit"), content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                transliteration = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return new YandexTransliterationResult(transliteration, text, toLanguage as Language, fromLanguage as Language);
        }

        /// <summary>
        /// Detects the language of a text using Yandex.Translate.
        /// </summary>
        /// <param name="text">The text to detect its language.</param>
        /// <returns>A task that represents the asynchronous language detection operation. The task contains the detected language.</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="TranslatorException"/>
        public async Task<Language> DetectLanguageAsync(string text)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text);

            string query = $"?ucid={GetOrUpdateUcid()}" +
                           "&srv=android" +
                           "&format=text";

            var data = new Dictionary<string, string>
            {
                { "text", text },
                { "hint", "en" }
            };

            string json;
            using (var content = new FormUrlEncodedContent(data))
            {
                var response = await _httpClient.PostAsync(new Uri($"{_apiUrl}/translate{query}"), content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var model = JsonConvert.DeserializeObject<YandexLanguageDetectionModel>(json);
            if (model == null)
            {
                throw new TranslatorException("The API returned an empty response.", Name);
            }

            return Language.GetLanguage(model.Lang);
        }

        /// <summary>   
        /// Returns whether Yandex.Translate supports the specified language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns><see langword="true"/> if the language is supported, otherwise <see langword="false"/>.</returns>
        public bool IsLanguageSupported(string language)
        {
            if (!string.IsNullOrEmpty(language))
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

            return (language.SupportedServices & TranslationServices.Yandex) == TranslationServices.Yandex;
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

        private string GetOrUpdateUcid()
        {
            if (_cachedUcid == null || _cachedUcid.IsExpired())
            {
                _cachedUcid = new CachedObject<string>(Guid.NewGuid().ToString("N"), TimeSpan.FromSeconds(360));
            }

            return _cachedUcid.Value;
        }
    }
}