namespace GTranslate.Results
{
    /// <summary>
    /// Represents a translation result.
    /// </summary>
    public interface ITranslationResult
    {
        /// <summary>
        /// Gets the translation.
        /// </summary>
        string Translation { get; }

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