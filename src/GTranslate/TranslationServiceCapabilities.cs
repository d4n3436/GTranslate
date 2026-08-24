using System;
using JetBrains.Annotations;

namespace GTranslate;

/// <summary>
/// Specifies the capabilities exposed by a translation service.
/// </summary>
[Flags]
[PublicAPI]
public enum TranslationServiceCapabilities
{
    /// <summary>
    /// No capabilities are available.
    /// </summary>
    None = 0,

    /// <summary>
    /// Text translation is supported.
    /// </summary>
    Translation = 1 << 0,

    /// <summary>
    /// Language detection is supported.
    /// </summary>
    Detection = 1 << 1,

    /// <summary>
    /// Transliteration is supported.
    /// </summary>
    Transliteration = 1 << 2,

    /// <summary>
    /// Text-to-speech is supported.
    /// </summary>
    TextToSpeech = 1 << 3,

    /// <summary>
    /// Dictionary lookup is supported.
    /// </summary>
    Dictionary = 1 << 4,

    /// <summary>
    /// Alternative translations are included in dictionary results.
    /// </summary>
    AlternativeTranslations = 1 << 5,

    /// <summary>
    /// Parts of speech are included in dictionary results.
    /// </summary>
    PartOfSpeech = 1 << 6,

    /// <summary>
    /// Back translations are included in dictionary results.
    /// </summary>
    BackTranslations = 1 << 7,

    /// <summary>
    /// Confidence scores are included in dictionary results.
    /// </summary>
    Confidence = 1 << 8,

    /// <summary>
    /// Definitions are included in dictionary results.
    /// </summary>
    Definitions = 1 << 9,

    /// <summary>
    /// Synonyms are included in dictionary results.
    /// </summary>
    Synonyms = 1 << 10,

    /// <summary>
    /// Usage examples are included in dictionary results.
    /// </summary>
    Examples = 1 << 11,

    /// <summary>
    /// Pronunciation or transliteration is included in dictionary results.
    /// </summary>
    Pronunciation = 1 << 12
}
