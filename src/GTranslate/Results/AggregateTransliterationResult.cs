using System;
using System.Collections.Generic;
using System.Diagnostics;
using GTranslate.Translators;

namespace GTranslate.Results;

/// <summary>
/// Represents the result of transliterating text using <see cref="AggregateTranslator"/>.
/// It wraps a <see cref="ITranslationResult"/> and includes exceptions that have occurred in other translators before receiving the result.
/// </summary>
public class AggregateTransliterationResult : ITransliterationResult
{
    internal AggregateTransliterationResult(ITransliterationResult result, IReadOnlyDictionary<string, Exception> exceptions)
    {
        InnerResult = result;
        Exceptions = exceptions;
    }

    /// <summary>
    /// Gets the transliteration result.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ITransliterationResult InnerResult { get; }

    /// <summary>
    /// Gets a read-only dictionary of exceptions that have occurred in other translators before receiving the result. The key is the name of the translator that has thrown the exception (the value).
    /// </summary>
    public IReadOnlyDictionary<string, Exception> Exceptions { get; }

    /// <inheritdoc/>
    public string Transliteration => InnerResult.Transliteration;

    /// <inheritdoc/>
    public string Source => InnerResult.Source;

    /// <inheritdoc/>
    public string Service => InnerResult.Service;

    /// <inheritdoc/>
    public ILanguage SourceLanguage => InnerResult.SourceLanguage;

    /// <inheritdoc/>
    public ILanguage TargetLanguage => InnerResult.TargetLanguage;
}