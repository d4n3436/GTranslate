namespace GTranslate.Results
{
    /// <summary>
    /// Represents a transliteration result from Yandex.Translate.
    /// </summary>
    public class YandexTransliterationResult : ITransliterationResult
    {
        internal YandexTransliterationResult(string result, string source, Language targetLanguage, Language sourceLanguage)
        {
            Result = result;
            Source = source;
            TargetLanguage = targetLanguage;
            SourceLanguage = sourceLanguage;
        }

        /// <inheritdoc/>
        public string Service => "YandexTranslator";

        /// <summary>
        /// Gets the transliteration result.
        /// </summary>
        public string Result { get; }

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public Language TargetLanguage { get; }

        /// <inheritdoc/>
        public Language SourceLanguage { get; }
    }
}