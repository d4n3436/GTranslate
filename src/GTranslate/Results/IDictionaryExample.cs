using JetBrains.Annotations;

namespace GTranslate.Results;

/// <summary>
/// Represents a dictionary usage example.
/// </summary>
[PublicAPI]
public interface IDictionaryExample
{
    /// <summary>
    /// Gets the source-language example text.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Gets the translated example text, or <see langword="null"/> when the provider does not return one.
    /// </summary>
    string? Translation { get; }
}
