namespace GTranslate.Results
{
    /// <summary>
    /// Represents a transliteration result from Yandex.Translate.
    /// </summary>
    public class YandexTransliterationResult : ITransliterationResult
    {
        internal YandexTransliterationResult(string transliteration, string source, Language targetLanguage, Language sourceLanguage)
        {
            Transliteration = transliteration;
            Source = source;
            TargetLanguage = targetLanguage;
            SourceLanguage = sourceLanguage;
        }

        /// <inheritdoc/>
        public string Transliteration { get; }

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public string Service => "YandexTranslator";

        /// <inheritdoc/>
        public Language TargetLanguage { get; }

        /// <inheritdoc/>
        public Language SourceLanguage { get; }
    }
}