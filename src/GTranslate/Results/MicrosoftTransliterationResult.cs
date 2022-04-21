using System;
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

    /// <inheritdoc/>
    public string Transliteration { get; }

    /// <inheritdoc/>
    public string Source { get; }

    /// <inheritdoc/>
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
    ILanguage ITransliterationResult<ILanguage>.SourceLanguage => SourceLanguage;

    /// <inheritdoc />
    ILanguage ITransliterationResult<ILanguage>.TargetLanguage => TargetLanguage;

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Transliteration)}: '{Transliteration}', {nameof(Script)}: '{Script}', {nameof(Service)}: {Service}";
}