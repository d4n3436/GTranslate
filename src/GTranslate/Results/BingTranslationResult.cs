namespace GTranslate.Results
{
    /// <summary>
    /// Represents a translation result from Bing Translator.
    /// </summary>
    public class BingTranslationResult : ITranslationResult
    {
        internal BingTranslationResult(string result, string source, Language targetLanguage,
            Language sourceLanguage, string transliteration, string script, float score)
        {
            Result = result;
            Source = source;
            TargetLanguage = targetLanguage;
            SourceLanguage = sourceLanguage;
            Transliteration = transliteration;
            Script = script;
            Score = score;
        }

        /// <inheritdoc/>
        public string Service => "BingTranslator";

        /// <summary>
        /// Gets the translation result.
        /// </summary>
        public string Result { get; }

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public Language TargetLanguage { get; }

        /// <inheritdoc/>
        public Language SourceLanguage { get; }

        /// <summary>
        /// Gets the transliteration of the text.
        /// </summary>
        /// <remarks>This value is not always present.</remarks>
        public string Transliteration { get; }

        /// <summary>
        /// Gets the language script.
        /// </summary>
        /// <remarks>This value is not always present.</remarks>
        public string Script { get; }

        /// <summary>
        /// Gets the language detection score.
        /// </summary>
        public float Score { get; }
    }
}