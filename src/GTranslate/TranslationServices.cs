using System;

namespace GTranslate;

/// <summary>
/// Specifies the translation services.
/// </summary>
[Flags]
public enum TranslationServices
{
    /// <summary>
    /// Google Translate.
    /// </summary>
    Google = 1 << 0,

    /// <summary>
    /// Bing Translator.
    /// </summary>
    Bing = 1 << 1,

    /// <summary>
    /// Yandex.Translate.
    /// </summary>
    Yandex = 1 << 2,

    /// <summary>
    /// Microsoft Azure Translator.
    /// </summary>
    Microsoft = 1 << 3
}