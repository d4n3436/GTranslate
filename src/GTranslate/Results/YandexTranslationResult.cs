using GTranslate.Translators;
using System.Diagnostics;

namespace GTranslate.Results;

/// <summary>
/// Represents a translation result from Yandex.Translate.
/// </summary>
public class YandexTranslationResult : ITranslationResult<Language>, ITranslationResult
{
    internal YandexTranslationResult(string translation, string source, Language targetLanguage, Language sourceLanguage)
    {
        Translation = translation;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
    }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Translation"/>
    public string Translation { get; }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Source"/>
    public string Source { get; }

    /// <inheritdoc cref="ITranslationResult{TLanguage}.Service"/>
    public string Service => nameof(YandexTranslator);

    /// <inheritdoc/>
    public Language TargetLanguage { get; }

    /// <inheritdoc/>
    public Language SourceLanguage { get; }

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITranslationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITranslationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Translation)}: '{Translation}', {nameof(TargetLanguage)}: '{TargetLanguage.Name} ({TargetLanguage.ISO6391})', {nameof(SourceLanguage)}: '{SourceLanguage.Name} ({SourceLanguage.ISO6391})', {nameof(Service)}: {Service}";
}