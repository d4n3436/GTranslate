using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace GTranslate.Results;

/// <summary>
/// Represents a provider-neutral rich dictionary result.
/// </summary>
[PublicAPI]
public sealed class DictionaryResult : IDictionaryResult
{
    internal DictionaryResult(string source, string service, Language targetLanguage, Language sourceLanguage,
        string? headword = null, string? pronunciation = null, IReadOnlyList<IDictionaryGroup>? groups = null,
        IReadOnlyList<IDictionaryExample>? examples = null)
    {
        Source = source;
        Service = service;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
        Headword = headword;
        Pronunciation = pronunciation;
        Groups = groups ?? Array.Empty<IDictionaryGroup>();
        Examples = examples ?? Array.Empty<IDictionaryExample>();
    }

    /// <inheritdoc/>
    public string Source { get; }

    /// <inheritdoc/>
    public string Service { get; }

    /// <inheritdoc cref="IDictionaryResult.TargetLanguage"/>
    public Language TargetLanguage { get; }

    /// <inheritdoc cref="IDictionaryResult.SourceLanguage"/>
    public Language SourceLanguage { get; }

    /// <inheritdoc/>
    public string? Headword { get; }

    /// <inheritdoc/>
    public string? Pronunciation { get; }

    /// <inheritdoc/>
    public IReadOnlyList<IDictionaryGroup> Groups { get; }

    /// <inheritdoc/>
    public IReadOnlyList<IDictionaryExample> Examples { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage IDictionaryResult.TargetLanguage => TargetLanguage;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage IDictionaryResult.SourceLanguage => SourceLanguage;
}
