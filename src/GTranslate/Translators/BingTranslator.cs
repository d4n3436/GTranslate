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
    /// Represents the Bing Translator.
    /// </summary>
    public class BingTranslator : ITranslator, IDisposable
    {
        /// <summary>
        /// Returns the default API endpoint.
        /// </summary>
        public const string DefaultApiEndpoint = "https://www.bing.com/ttranslatev3";

        /// <summary>
        /// Returns the default User-Agent header.
        /// </summary>
        public const string DefaultUserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36";

        /// <inheritdoc/>
        public string Name => "BingTranslator";

        private readonly HttpClient _httpClient = new HttpClient();
        private CachedObject<BingCredentials> _cachedCredentials;
        private string _apiEndpoint;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BingTranslator"/> class.
        /// </summary>
        public BingTranslator()
        {
            Init(DefaultApiEndpoint, DefaultUserAgent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BingTranslator"/> class with the provided API endpoint.
        /// </summary>
        public BingTranslator(string apiEndpoint)
        {
            Init(apiEndpoint, DefaultUserAgent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BingTranslator"/> class with the provided API endpoint and User-Agent header.
        /// </summary>
        public BingTranslator(string apiEndpoint, string userAgent)
        {
            Init(apiEndpoint, userAgent);
        }

        private void Init(string apiEndpoint, string userAgent)
        {
            _apiEndpoint = apiEndpoint;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
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
        public async Task<BingTranslationResult> TranslateAsync(string text, string toLanguage, string fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageFound(toLanguage, fromLanguage, out var toLang, out var fromLang);
            TranslatorGuards.LanguageSupported(this, toLang, fromLang);

            return await TranslateAsync(text, toLang, fromLang).ConfigureAwait(false);
        }

        /// <inheritdoc cref="TranslateAsync(string, string, string)"/>
        public async Task<BingTranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

            var credentials = await GetOrUpdateCredentialsAsync().ConfigureAwait(false);

            var data = new Dictionary<string, string>
            {
                { "fromLang", BingHotPatch(fromLanguage?.ISO6391) ?? "auto-detect" },
                { "text", text },
                { "to", BingHotPatch(toLanguage.ISO6391) },
                { "token", credentials.Token },
                { "key", credentials.Key }
            };

            string json;
            using (var content = new FormUrlEncodedContent(data))
            {
                // For some reason the "isVertical" parameter allows you to translate up to 1000 characters instead of 500
                var response = await _httpClient.PostAsync(new Uri($"{_apiEndpoint}?isVertical=1"), content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            // Bing Translator always sends status code 200 but sends a different response on errors
            var props = JToken.Parse(json).OfType<JProperty>();
            var statusCode = props.FirstOrDefault(x => x.Name == "statusCode");

            if (statusCode != null)
            {
                var errorMessage = props.FirstOrDefault(x => x.Name == "errorMessage")?.Value.ToString();
                throw new TranslatorException(!string.IsNullOrEmpty(errorMessage) ? errorMessage : $"The API returned error code {statusCode.Value}.", Name);
            }

            var model = JsonConvert.DeserializeObject<List<BingTranslationModel>>(json);
            if (model == null || model.Count == 0 || model[0].Translations.Count == 0)
            {
                throw new TranslatorException("The API returned an empty response.", Name);
            }

            var translation = model[0].Translations[0];
            string transliteration = model.Count > 1 ? model[1].InputTransliteration : translation.Transliteration.Text;

            return new BingTranslationResult(translation.Text, text, Language.GetLanguage(translation.To),
                Language.GetLanguage(model[0].DetectedLanguage.Language), transliteration, translation.Transliteration.Script, model[0].DetectedLanguage.Score);
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
        public async Task<BingTransliterationResult> TransliterateAsync(string text, string toLanguage, string fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageFound(toLanguage, fromLanguage, out var toLang, out var fromLang);
            TranslatorGuards.LanguageSupported(this, toLang, fromLang);

            return await TransliterateAsync(text, toLang, fromLang).ConfigureAwait(false);
        }

        /// <inheritdoc cref="TransliterateAsync(string, string, string)"/>
        public async Task<BingTransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage fromLanguage = null)
        {
            var result = await TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);
            if (string.IsNullOrEmpty(result.Transliteration))
            {
                throw new TranslatorException("Failed to get the transliteration.", Name);
            }

            return new BingTransliterationResult(result.Transliteration, text, result.TargetLanguage, result.SourceLanguage, result.Script);
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
            var result = await TranslateAsync(text, "en").ConfigureAwait(false);
            return result.SourceLanguage;
        }

        /// <summary>
        /// Returns whether Bing Translator supports the specified language.
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

            return (language.SupportedServices & TranslationServices.Bing) == TranslationServices.Bing;
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
        /// Hot-patches language codes to Bing-specific ones.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <returns>The hot-patched language code.</returns>
        private static string BingHotPatch(string languageCode)
        {
            switch (languageCode)
            {
                case "no":
                    return "nb";

                case "sr":
                    return "sr-Cyrl";

                case "tlh":
                    return "tlh-Latn";

                case "zh-CN":
                    return "zh-Hans";

                case "zh-TW":
                    return "zh-Hant";

                default:
                    return languageCode;
            }
        }

        private async Task<BingCredentials> GetOrUpdateCredentialsAsync()
        {
            if (_cachedCredentials.Value != null && !_cachedCredentials.IsExpired())
            {
                return _cachedCredentials.Value;
            }

            const string credentialsStart = "var params_RichTranslateHelper = [";

            string content = await _httpClient.GetStringAsync(new Uri("https://www.bing.com/translator")).ConfigureAwait(false);

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
            string key = content.Substring(keyStartIndex, keyEndIndex - keyStartIndex);
            if (!long.TryParse(key, out long timestamp))
            {
                // This shouldn't happen but we'll handle this case anyways
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            int tokenStartIndex = keyEndIndex + 2;
            int tokenEndIndex = content.IndexOf('"', tokenStartIndex);
            if (tokenEndIndex == -1)
            {
                throw new TranslatorException("Unable to find the Bing token.", Name);
            }

            string token = content.Substring(tokenStartIndex, tokenEndIndex - tokenStartIndex);
            var credentials = new BingCredentials { Key = key, Token = token };

            _cachedCredentials = new CachedObject<BingCredentials>(credentials, DateTimeOffset.FromUnixTimeMilliseconds(timestamp + 3600000));
            return _cachedCredentials.Value;
        }

        private class BingCredentials
        {
            public string Key { get; set; }

            public string Token { get; set; }
        }
    }
}