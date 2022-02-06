namespace GTranslate;

/// <summary>
/// Represents a language.
/// </summary>
public interface ILanguage
{
    /// <summary>
    /// Gets the name of this language.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the ISO 639-1 code of this language.
    /// </summary>
    string ISO6391 { get; }

    /// <summary>
    /// Gets the ISO 639-3 code of this language.
    /// </summary>
    string ISO6393 { get; }
}