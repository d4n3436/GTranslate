namespace GTranslate.Results;

/// <summary>
/// Represents a transliteration result from Google Translate.
/// </summary>
public class GoogleTransliterationResult : ITransliterationResult<Language>, ITransliterationResult
{
    internal GoogleTransliterationResult(string transliteration, string source, Language targetLanguage, Language? sourceLanguage)
    {
        Transliteration = transliteration;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
    }

    /// <inheritdoc/>
    public string Transliteration { get; }

    /// <inheritdoc/>
    public string Source { get; }

    /// <inheritdoc/>
    public string Service => "GoogleTranslator";

    /// <inheritdoc/>
    public Language TargetLanguage { get; }

    /// <inheritdoc/>
    public Language? SourceLanguage { get; }

    /// <inheritdoc />
    ILanguage? ITransliterationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    ILanguage ITransliterationResult<ILanguage>.TargetLanguage => TargetLanguage;
}