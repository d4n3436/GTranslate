using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GTranslate.Translators;

namespace GTranslate.Results;

/// <summary>
/// Represents a translation result from Bing Translator.
/// </summary>
public class BingTranslationResult : ITranslationResult<Language>, ITranslationResult
{
    internal BingTranslationResult(string translation, string source, Language targetLanguage,
        Language? sourceLanguage, string? transliteration, string? sourceTransliteration, string? script, float score)
    {
        Translation = translation;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
        Transliteration = transliteration;
        SourceTransliteration = sourceTransliteration;
        Script = script;
        Score = score;
    }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Translation"/>
    public string Translation { get; }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Source"/>
    public string Source { get; }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Service"/>
    public string Service => nameof(BingTranslator);

    /// <inheritdoc/>
    public Language TargetLanguage { get; }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.SourceLanguage"/>
    /// <remarks>This property returns <see langword="null"/> when the translator can't determine the source language.</remarks>
    public Language? SourceLanguage { get; }

    /// <summary>
    /// Gets the transliteration of the result (<see cref="Translation"/>).
    /// </summary>
    public string? Transliteration { get; }

    /// <summary>
    /// Gets the transliteration of the source text.
    /// </summary>
    public string? SourceTransliteration { get; }

    /// <summary>
    /// Gets the script of <see cref="Transliteration"/>.
    /// </summary>
    public string? Script { get; }

    /// <summary>
    /// Gets the language detection score.
    /// </summary>
    public float Score { get; }

    [MemberNotNullWhen(true, nameof(Transliteration), nameof(Script))]
    internal bool HasTransliteration => Transliteration is not null && Script is not null;

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Language ITranslationResult<Language>.SourceLanguage => SourceLanguage ?? throw new NotSupportedException("The source language is not available for this result.");

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITranslationResult<ILanguage>.SourceLanguage => SourceLanguage ?? throw new NotSupportedException("The source language is not available for this result.");

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITranslationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Translation)}: '{Translation}', {nameof(TargetLanguage)}: '{TargetLanguage.Name} ({TargetLanguage.ISO6391})', {nameof(SourceLanguage)}: '{SourceLanguage?.Name ?? "Unknown"} ({SourceLanguage?.ISO6391 ?? "N/A"})', {nameof(Service)}: {Service}";
}