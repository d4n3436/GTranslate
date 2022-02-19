namespace GTranslate.Results;

/// <summary>
/// Represents a transliteration result.
/// </summary>
/// <typeparam name="TLanguage">The language type.</typeparam>
public interface ITransliterationResult<out TLanguage>
    where TLanguage : ILanguage
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
    TLanguage SourceLanguage { get; }

    /// <summary>
    /// Gets the target language.
    /// </summary>
    TLanguage TargetLanguage { get; }
}

/// <inheritdoc/>
public interface ITransliterationResult : ITransliterationResult<ILanguage>
{
}