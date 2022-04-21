using GTranslate.Translators;

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

    /// <inheritdoc/>
    public string Translation { get; }

    /// <inheritdoc/>
    public string Source { get; }

    /// <inheritdoc/>
    public string Service => nameof(YandexTranslator);

    /// <inheritdoc/>
    public Language TargetLanguage { get; }

    /// <inheritdoc/>
    public Language SourceLanguage { get; }

    /// <inheritdoc />
    ILanguage ITranslationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    ILanguage ITranslationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Translation)}: '{Translation}', {nameof(TargetLanguage)}: '{TargetLanguage.Name} ({TargetLanguage.ISO6391})', {nameof(SourceLanguage)}: '{SourceLanguage.Name} ({SourceLanguage.ISO6391})', {nameof(Service)}: {Service}";
}