using System;
using System.Collections.Generic;
using System.Diagnostics;
using GTranslate.Translators;

namespace GTranslate.Results;

/// <summary>
/// Represents the result of translating text using <see cref="AggregateTranslator"/>.
/// It wraps a <see cref="ITranslationResult"/> and includes exceptions that have occurred in other translators before receiving the result.
/// </summary>
public class AggregateTranslationResult : ITranslationResult
{
    internal AggregateTranslationResult(ITranslationResult result, IReadOnlyDictionary<string, Exception> exceptions)
    {
        InnerResult = result;
        Exceptions = exceptions;
    }

    /// <summary>
    /// Gets the translation result.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ITranslationResult InnerResult { get; }

    /// <summary>
    /// Gets a read-only dictionary of exceptions that have occurred in other translators before receiving the result. The key is the name of the translator that has thrown the exception (the value).
    /// </summary>
    public IReadOnlyDictionary<string, Exception> Exceptions { get; }

    /// <inheritdoc/>
    public string Translation => InnerResult.Translation;

    /// <inheritdoc/>
    public string Source => InnerResult.Source;

    /// <inheritdoc/>
    public string Service => InnerResult.Service;

    /// <inheritdoc/>
    public ILanguage SourceLanguage => InnerResult.SourceLanguage;

    /// <inheritdoc/>
    public ILanguage TargetLanguage => InnerResult.TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Translation)}: '{Translation}', {nameof(TargetLanguage)}: '{TargetLanguage.Name} ({TargetLanguage.ISO6391})', {nameof(SourceLanguage)}: '{SourceLanguage.Name} ({SourceLanguage.ISO6391})', {nameof(Service)}: {Service}";
}