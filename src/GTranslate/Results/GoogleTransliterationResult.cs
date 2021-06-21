namespace GTranslate.Results
{
    /// <summary>
    /// Represents a transliteration result from Google Translate.
    /// </summary>
    public class GoogleTransliterationResult : ITransliterationResult
    {
        internal GoogleTransliterationResult(string result, string source, Language targetLanguage, Language sourceLanguage)
        {
            Result = result;
            Source = source;
            TargetLanguage = targetLanguage;
            SourceLanguage = sourceLanguage;
        }

        /// <inheritdoc/>
        public string Service => "GoogleTranslator";

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