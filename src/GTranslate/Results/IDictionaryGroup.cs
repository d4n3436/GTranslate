using System.Collections.Generic;
using JetBrains.Annotations;

namespace GTranslate.Results;

/// <summary>
/// Represents dictionary entries grouped by part of speech.
/// </summary>
[PublicAPI]
public interface IDictionaryGroup
{
    /// <summary>
    /// Gets the provider part-of-speech label, or <see langword="null"/> when unavailable.
    /// </summary>
    string? PartOfSpeech { get; }

    /// <summary>
    /// Gets the translated entries in this group.
    /// </summary>
    IReadOnlyList<IDictionaryEntry> Entries { get; }

    /// <summary>
    /// Gets source-language definitions associated with this part of speech.
    /// </summary>
    IReadOnlyList<string> Definitions { get; }

    /// <summary>
    /// Gets source-language synonyms associated with this part of speech.
    /// </summary>
    IReadOnlyList<string> Synonyms { get; }
}
