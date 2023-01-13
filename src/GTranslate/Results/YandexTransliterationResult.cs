using GTranslate.Translators;
using System.Diagnostics;

namespace GTranslate.Results;

/// <summary>
/// Represents a transliteration result from Yandex.Translate.
/// </summary>
public class YandexTransliterationResult : ITransliterationResult<Language>, ITransliterationResult
{
    internal YandexTransliterationResult(string transliteration, string source, Language targetLanguage, Language sourceLanguage)
    {
        Transliteration = transliteration;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
    }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Transliteration"/>
    public string Transliteration { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Source"/>
    public string Source { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Service"/>
    public string Service => nameof(YandexTranslator);

    /// <inheritdoc/>
    public Language TargetLanguage { get; }

    /// <inheritdoc/>
    public Language SourceLanguage { get; }

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITransliterationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITransliterationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Transliteration)}: '{Transliteration}', {nameof(TargetLanguage)}: '{TargetLanguage.Name} ({TargetLanguage.ISO6391})', {nameof(SourceLanguage)}: '{SourceLanguage.Name} ({SourceLanguage.ISO6391})', {nameof(Service)}: {Service}";
}