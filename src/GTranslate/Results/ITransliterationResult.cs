namespace GTranslate.Results
{
    /// <summary>
    /// Represents a transliteration result.
    /// </summary>
    public interface ITransliterationResult
    {
        /// <summary>
        /// Gets the transliteration.
        /// </summary>
        string Transliteration { get; }

        /// <summary>
        /// Gets the source text.
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Gets the service this result is from.
        /// </summary>
        string Service { get; }

        /// <summary>
        /// Gets the source language.
        /// </summary>
        Language SourceLanguage { get; }

        /// <summary>
        /// Gets the target language.
        /// </summary>
        Language TargetLanguage { get; }
    }
}