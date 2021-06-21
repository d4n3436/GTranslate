namespace GTranslate.Results
{
    /// <summary>
    /// Represents a translation result from Yandex.Translate.
    /// </summary>
    public class YandexTranslationResult : ITranslationResult
    {
        internal YandexTranslationResult(string result, string source, Language targetLanguage, Language sourceLanguage)
        {
            Result = result;
            Source = source;
            TargetLanguage = targetLanguage;
            SourceLanguage = sourceLanguage;
        }

        /// <inheritdoc/>
        public string Service => "YandexTranslator";

        /// <inheritdoc/>
        public string Result { get; }

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public Language TargetLanguage { get; }

        /// <inheritdoc/>
        public Language SourceLanguage { get; }
    }
}