using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GTranslate.Results;

/// <summary>
/// Represents one translated dictionary entry.
/// </summary>
[PublicAPI]
public sealed class DictionaryEntry : IDictionaryEntry
{
    internal DictionaryEntry(string text, double? confidence = null, long? frequency = null,
        IReadOnlyList<string>? backTranslations = null, IReadOnlyList<IDictionaryExample>? examples = null,
        string? normalizedText = null, string? prefix = null, string? transliteration = null)
    {
        Text = text;
        Confidence = confidence;
        Frequency = frequency;
        BackTranslations = backTranslations ?? Array.Empty<string>();
        Examples = examples ?? Array.Empty<IDictionaryExample>();
        NormalizedText = normalizedText;
        Prefix = prefix;
        Transliteration = transliteration;
    }

    /// <inheritdoc/>
    public string Text { get; }

    /// <inheritdoc/>
    public string? NormalizedText { get; }

    /// <inheritdoc/>
    public string? Prefix { get; }

    /// <inheritdoc/>
    public string? Transliteration { get; }

    /// <inheritdoc/>
    public double? Confidence { get; }

    /// <inheritdoc/>
    public long? Frequency { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> BackTranslations { get; }

    /// <inheritdoc/>
    public IReadOnlyList<IDictionaryExample> Examples { get; }
}
