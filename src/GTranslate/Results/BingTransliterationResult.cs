namespace GTranslate.Results
{
    /// <summary>
    /// Represents a transliteration result from Bing Translator.
    /// </summary>
    public class BingTransliterationResult : ITransliterationResult
    {
        internal BingTransliterationResult(string transliteration, string source, Language targetLanguage, Language sourceLanguage, string script)
        {
            Transliteration = transliteration;
            Source = source;
            TargetLanguage = targetLanguage;
            SourceLanguage = sourceLanguage;
            Script = script;
        }

        /// <inheritdoc/>
        public string Transliteration { get; }

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public string Service => "BingTranslator";

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