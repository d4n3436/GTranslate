using System;
using System.Diagnostics;
using GTranslate.Translators;

namespace GTranslate.Results;

/// <summary>
/// Represents a transliteration result from Microsoft Translator.
/// </summary>
public class MicrosoftTransliterationResult : ITransliterationResult<Language>, ITransliterationResult
{
    internal MicrosoftTransliterationResult(string transliteration, string source, string script)
    {
        Transliteration = transliteration;
        Source = source;
        Script = script;
    }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Transliteration"/>
    public string Transliteration { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Source"/>
    public string Source { get; }

    /// <inheritdoc cref="ITransliterationResult{TLanguage}.Service"/>
    public string Service => nameof(MicrosoftTranslator);

    /// <inheritdoc/>
    public Language TargetLanguage => throw new NotSupportedException("Microsoft Translator does not provide the target language.");

    /// <inheritdoc/>
    public Language SourceLanguage => throw new NotSupportedException("Microsoft Translator does not provide the source language.");

    /// <summary>
    /// Gets the language script.
    /// </summary>
    public string Script { get; }

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITransliterationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ILanguage ITransliterationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Transliteration)}: '{Transliteration}', {nameof(Script)}: '{Script}', {nameof(Service)}: {Service}";
}