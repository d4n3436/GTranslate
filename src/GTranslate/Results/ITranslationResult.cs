namespace GTranslate.Results;

/// <summary>
/// Represents a translation result.
/// </summary>
/// <typeparam name="TLanguage">The language type.</typeparam>
public interface ITranslationResult<out TLanguage>
    where TLanguage : ILanguage
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
    TLanguage SourceLanguage { get; }

    /// <summary>
    /// Gets the target language.
    /// </summary>
    TLanguage TargetLanguage { get; }
}

/// <inheritdoc/>
public interface ITranslationResult : ITranslationResult<ILanguage>
{
}