﻿using GTranslate.Translators;
using System.Diagnostics;

namespace GTranslate.Results;

/// <summary>
/// Represents a translation result from Microsoft Translator.
/// </summary>
public class MicrosoftTranslationResult : ITranslationResult<Language>, ITranslationResult
{
    internal MicrosoftTranslationResult(string translation, string source, Language targetLanguage,
        Language sourceLanguage, float score)
    {
        Translation = translation;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
        Score = score;
    }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Translation"/>
    public string Translation { get; }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Source"/>
    public string Source { get; }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Service"/>
    public string Service => nameof(MicrosoftTranslator);

    /// <inheritdoc/>
    public Language TargetLanguage { get; }

    /// <inheritdoc/>
    public Language SourceLanguage { get; }

    /// <summary>
    /// Gets the language detection score.
    /// </summary>
    public float? Score { get; } // TODO: Remove nullable mark

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITranslationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITranslationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Translation)}: '{Translation}', {nameof(TargetLanguage)}: '{TargetLanguage.Name} ({TargetLanguage.ISO6391})', {nameof(SourceLanguage)}: '{SourceLanguage.Name} ({SourceLanguage.ISO6391})', {nameof(Service)}: {Service}";
}