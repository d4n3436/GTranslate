using GTranslate.Translators;

namespace GTranslate.Results;

/// <summary>
/// Represents a translation result from Bing Translator.
/// </summary>
public class BingTranslationResult : ITranslationResult<Language>, ITranslationResult
{
    internal BingTranslationResult(string translation, string source, Language targetLanguage,
        Language sourceLanguage, string? transliteration, string? script, float score)
    {
        Translation = translation;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
        Transliteration = transliteration;
        Script = script;
        Score = score;
    }

    /// <inheritdoc/>
    public string Translation { get; }

    /// <inheritdoc/>
    public string Source { get; }

    /// <inheritdoc/>
    public string Service => nameof(BingTranslator);

    /// <inheritdoc/>
    public Language TargetLanguage { get; }

    /// <inheritdoc/>
    public Language SourceLanguage { get; }

    /// <summary>
    /// Gets the transliteration of the text.
    /// </summary>
    public string? Transliteration { get; }

    /// <summary>
    /// Gets the language script.
    /// </summary>
    public string? Script { get; }

    /// <summary>
    /// Gets the language detection score.
    /// </summary>
    public float Score { get; }

    /// <inheritdoc />
    ILanguage ITranslationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    ILanguage ITranslationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Translation)}: '{Translation}', {nameof(TargetLanguage)}: '{TargetLanguage.Name} ({TargetLanguage.ISO6391})', {nameof(SourceLanguage)}: '{SourceLanguage.Name} ({SourceLanguage.ISO6391})', {nameof(Service)}: {Service}";
}