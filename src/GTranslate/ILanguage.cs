using JetBrains.Annotations;

namespace GTranslate;

/// <summary>
/// Provides information about a language.
/// </summary>
[PublicAPI]
public interface ILanguage
{
    /// <summary>
    /// Gets the name of this language.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the ISO 639-1 code of this language.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    // Updating name is a breaking change
    string ISO6391 { get; }

    /// <summary>
    /// Gets the ISO 639-3 code of this language.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    // Updating name is a breaking change
    string ISO6393 { get; }
}