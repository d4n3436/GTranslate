namespace GTranslate.Results
{
    /// <summary>
    /// Represents a translation result.
    /// </summary>
    public interface ITranslationResult : IResult<string>
    {
        /// <summary>
        /// Gets the source text.
        /// </summary>
        string Source { get; }

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