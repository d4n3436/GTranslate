using System.Collections.Generic;
using JetBrains.Annotations;

namespace GTranslate.Results;

/// <summary>
/// Represents a provider-neutral rich dictionary result.
/// </summary>
[PublicAPI]
public interface IDictionaryResult
{
    /// <summary>
    /// Gets the source text.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Gets the service that actually produced this result.
    /// </summary>
    string Service { get; }

    /// <summary>
    /// Gets the target language.
    /// </summary>
    ILanguage TargetLanguage { get; }

    /// <summary>
    /// Gets the source language.
    /// </summary>
    ILanguage SourceLanguage { get; }

    /// <summary>
    /// Gets the normalized headword, or <see langword="null"/> when unavailable.
    /// </summary>
    string? Headword { get; }

    /// <summary>
    /// Gets the pronunciation or transliteration, or <see langword="null"/> when unavailable.
    /// </summary>
    string? Pronunciation { get; }

    /// <summary>
    /// Gets dictionary groups returned by the provider.
    /// </summary>
    IReadOnlyList<IDictionaryGroup> Groups { get; }

    /// <summary>
    /// Gets provider examples that are not associated with one specific entry.
    /// </summary>
    IReadOnlyList<IDictionaryExample> Examples { get; }
}
