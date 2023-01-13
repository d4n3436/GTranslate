using GTranslate.Translators;
using System.Diagnostics;

namespace GTranslate.Results;

/// <summary>
/// Represents a transliteration result from Bing Translator.
/// </summary>
public class BingTransliterationResult : ITransliterationResult<Language>, ITransliterationResult
{
    internal BingTransliterationResult(string transliteration, string source, Language targetLanguage, Language sourceLanguage, string? script)
    {
        Transliteration = transliteration;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
        Script = script;
    }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Transliteration"/>
    public string Transliteration { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Source"/>
    public string Source { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Service"/>
    public string Service => nameof(BingTranslator);

    /// <inheritdoc/>
    public Language TargetLanguage { get; }

    /// <inheritdoc/>
    public Language SourceLanguage { get; }

    /// <summary>
    /// Gets the language script.
    /// </summary>
    public string? Script { get; }

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITransliterationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITransliterationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Transliteration)}: '{Transliteration}', {nameof(TargetLanguage)}: '{TargetLanguage.Name} ({TargetLanguage.ISO6391})', {nameof(SourceLanguage)}: '{SourceLanguage.Name} ({SourceLanguage.ISO6391})', {nameof(Service)}: {Service}";
}