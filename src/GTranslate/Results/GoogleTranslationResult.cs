using System.Diagnostics;
using GTranslate.Translators;

namespace GTranslate.Results;

/// <summary>
/// Represents a translation result from Google Translate.
/// </summary>
public class GoogleTranslationResult : ITranslationResult<Language>, ITranslationResult
{
    internal GoogleTranslationResult(string translation, string source, Language targetLanguage,
        Language sourceLanguage, string? transliteration = null, string? sourceTransliteration = null, float? confidence = null, string service = nameof(GoogleTranslator))
    {
        Translation = translation;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
        Transliteration = transliteration;
        SourceTransliteration = sourceTransliteration;
        Confidence = confidence;
        Service = service;
    }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Translation"/>
    public string Translation { get; }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Source"/>
    public string Source { get; }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Service"/>
    public string Service { get; }

    /// <inheritdoc/>
    public Language TargetLanguage { get; }

    /// <inheritdoc/>
    public Language SourceLanguage { get; }

    /// <summary>
    /// Gets the transliteration of the text.
    /// </summary>
    public string? Transliteration { get; }

    /// <summary>
    /// Gets the transliteration of the source text.
    /// </summary>
    public string? SourceTransliteration { get; }

    /// <summary>
    /// Gets the translation confidence.
    /// </summary>
    public float? Confidence { get; }

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITranslationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITranslationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Translation)}: '{Translation}', {nameof(TargetLanguage)}: '{TargetLanguage.Name} ({TargetLanguage.ISO6391})', {nameof(SourceLanguage)}: '{SourceLanguage.Name} ({SourceLanguage.ISO6391})', {nameof(Service)}: {Service}";
}