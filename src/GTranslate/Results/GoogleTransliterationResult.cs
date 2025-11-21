using System.Diagnostics;
using GTranslate.Translators;
using JetBrains.Annotations;

namespace GTranslate.Results;

/// <summary>
/// Represents a transliteration result from Google Translate.
/// </summary>
[PublicAPI]
public class GoogleTransliterationResult : ITransliterationResult<Language>, ITransliterationResult
{
    internal GoogleTransliterationResult(string transliteration, string? sourceTransliteration, string source, Language targetLanguage, Language sourceLanguage, string service = nameof(GoogleTranslator))
    {
        Transliteration = transliteration;
        SourceTransliteration = sourceTransliteration;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
        Service = service;
    }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Transliteration"/>
    public string Transliteration { get; }

    /// <summary>
    /// Gets the transliteration of the source text.
    /// </summary>
    public string? SourceTransliteration { get; }

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