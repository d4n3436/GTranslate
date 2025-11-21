using System;
using System.Diagnostics;
using GTranslate.Translators;

namespace GTranslate.Results;

/// <summary>
/// Represents a transliteration result from Bing Translator.
/// </summary>
public class BingTransliterationResult : ITransliterationResult<Language>, ITransliterationResult
{
    internal BingTransliterationResult(string transliteration, string? sourceTransliteration,
        string source, Language targetLanguage, Language? sourceLanguage, string script)
    {
        Transliteration = transliteration;
        SourceTransliteration = sourceTransliteration;
        Source = source;
        TargetLanguage = targetLanguage;
        SourceLanguage = sourceLanguage;
        Script = script;
    }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Transliteration"/>
    public string Transliteration { get; }

    /// <summary>
    /// Gets the transliteration of the source text (<see cref="Source"/>).
    /// </summary>
    public string? SourceTransliteration { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Source"/>
    public string Source { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Service"/>
    public string Service => nameof(BingTranslator);

    /// <inheritdoc/>
    public Language TargetLanguage { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.SourceLanguage"/>
    /// <remarks>This property returns <see langword="null"/> when the translator can't determine the source language.</remarks>
    public Language? SourceLanguage { get; }

    /// <summary>
    /// Gets the language script.
    /// </summary>
    public string Script { get; }

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Language ITransliterationResult<Language>.SourceLanguage => SourceLanguage ?? throw new NotSupportedException("The source language is not available for this result.");

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITransliterationResult<ILanguage>.SourceLanguage => SourceLanguage ?? throw new NotSupportedException("The source language is not available for this result.");

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITransliterationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Transliteration)}: '{Transliteration}', {nameof(TargetLanguage)}: '{TargetLanguage.Name} ({TargetLanguage.ISO6391})', {nameof(SourceLanguage)}: '{SourceLanguage?.Name ?? "Unknown"} ({SourceLanguage?.ISO6391 ?? "N/A"})', {nameof(Service)}: {Service}";
}