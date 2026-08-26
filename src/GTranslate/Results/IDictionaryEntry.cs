using System.Collections.Generic;
using JetBrains.Annotations;

namespace GTranslate.Results;

/// <summary>
/// Represents one translated dictionary entry.
/// </summary>
[PublicAPI]
public interface IDictionaryEntry
{
    /// <summary>
    /// Gets the translated text.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Gets the provider-normalized translated text, or <see langword="null"/> when unavailable.
    /// </summary>
    string? NormalizedText { get; }

    /// <summary>
    /// Gets a provider prefix that should be displayed before <see cref="Text"/>, or <see langword="null"/> when unavailable.
    /// </summary>
    string? Prefix { get; }

    /// <summary>
    /// Gets the transliteration of <see cref="Text"/>, or <see langword="null"/> when unavailable.
    /// </summary>
    string? Transliteration { get; }

    /// <summary>
    /// Gets the provider confidence score, or <see langword="null"/> when unavailable.
    /// </summary>
    double? Confidence { get; }

    /// <summary>
    /// Gets the provider frequency value, or <see langword="null"/> when unavailable.
    /// </summary>
    long? Frequency { get; }

    /// <summary>
    /// Gets the back translations returned for this entry.
    /// </summary>
    IReadOnlyList<string> BackTranslations { get; }

    /// <summary>
    /// Gets usage examples associated with this entry.
    /// </summary>
    IReadOnlyList<IDictionaryExample> Examples { get; }
}
