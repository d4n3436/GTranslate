using JetBrains.Annotations;

namespace GTranslate.Results;

/// <summary>
/// Represents a dictionary usage example.
/// </summary>
[PublicAPI]
public sealed class DictionaryExample : IDictionaryExample
{
    internal DictionaryExample(string source, string? translation = null)
    {
        Source = source;
        Translation = translation;
    }

    /// <inheritdoc/>
    public string Source { get; }

    /// <inheritdoc/>
    public string? Translation { get; }
}
