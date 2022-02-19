namespace GTranslate.Results;

/// <summary>
/// Represents a translation result from Google Translate.
/// </summary>
public class GoogleTranslationResult : ITranslationResult<Language>, ITranslationResult
{
    internal GoogleTranslationResult(string translation, string source, Language targetLanguage,
        Language sourceLanguage, string? transliteration = null, float? confidence = null, string service = "GoogleTranslator")
    {
        Translation = translation;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
        Transliteration = transliteration;
        Confidence = confidence;
        Service = service;
    }

    /// <inheritdoc/>
    public string Translation { get; }

    /// <inheritdoc/>
    public string Source { get; }

    /// <inheritdoc/>
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
    /// Gets the translation confidence.
    /// </summary>
    public float? Confidence { get; }

    /// <inheritdoc />
    ILanguage ITranslationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    ILanguage ITranslationResult<ILanguage>.TargetLanguage => TargetLanguage;
}