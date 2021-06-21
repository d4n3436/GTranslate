namespace GTranslate.Results
{
    /// <summary>
    /// Represents a transliteration result from Bing Translator.
    /// </summary>
    public class BingTransliterationResult : ITransliterationResult
    {
        internal BingTransliterationResult(string result, string source, Language targetLanguage, Language sourceLanguage, string script)
        {
            Result = result;
            Source = source;
            TargetLanguage = targetLanguage;
            SourceLanguage = sourceLanguage;
            Script = script;
        }

        /// <inheritdoc/>
        public string Service => "BingTranslator";

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

        /// <summary>
        /// Gets the language script.
        /// </summary>
        /// <remarks>This value is not always present.</remarks>
        public string Script { get; }
    }
}