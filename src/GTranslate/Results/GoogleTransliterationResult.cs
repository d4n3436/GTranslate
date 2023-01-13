using GTranslate.Translators;
using System.Diagnostics;

namespace GTranslate.Results;

/// <summary>
/// Represents a transliteration result from Google Translate.
/// </summary>
public class GoogleTransliterationResult : ITransliterationResult<Language>, ITransliterationResult
{
    internal GoogleTransliterationResult(string transliteration, string source, Language targetLanguage, Language sourceLanguage, string service = nameof(GoogleTranslator))
    {
        Transliteration = transliteration;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
        Service = service;
    }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Transliteration"/>
    public string Transliteration { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Source"/>
    public string Source { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Service"/>
    public string Service { get; }

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