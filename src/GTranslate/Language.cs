using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GTranslate;

/// <summary>
/// Represents the default implementation of <see cref="ILanguage"/> used in GTranslate.
/// </summary>
/// <remarks>Due to the way GTranslate handles the supported languages,
///  custom translators should use a custom language class instead of this.
/// </remarks>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public sealed class Language : ILanguage, IEquatable<Language>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class, using a valid language name or code.
    /// </summary>
    /// <param name="nameOrCode">The language name or code. It can be a ISO 639-1 code, a ISO 639-3 code, a language name or a language alias.</param>
    /// <remarks>It is recommended to use <see cref="GetLanguage(string)"/> or <see cref="TryGetLanguage(string, out Language)"/> instead.</remarks>
    public Language(string nameOrCode)
    {
        TranslatorGuards.NotNull(nameOrCode);
        TranslatorGuards.LanguageFound(nameOrCode, out var language);

        Name = language.Name;
        NativeName = language.NativeName;
        ISO6391 = language.ISO6391;
        ISO6393 = language.ISO6393;
        SupportedServices = language.SupportedServices;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class, using the specified name, ISO codes and supported services.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="nativeName">The native name.</param>
    /// <param name="iso6391">The ISO 639-1 code.</param>
    /// <param name="iso6393">The ISO 639-3 code.</param>
    /// <param name="supportedServices">The supported services.</param>
    internal Language(string name, string nativeName, string iso6391, string iso6393,
        TranslationServices supportedServices = TranslationServices.Google | TranslationServices.Bing |
                                                TranslationServices.Yandex | TranslationServices.Microsoft)
    {
        Name = name;
        NativeName = nativeName;
        ISO6391 = iso6391;
        ISO6393 = iso6393;
        SupportedServices = supportedServices;
    }

    /// <summary>
    /// Gets the default language dictionary.
    /// </summary>
    public static LanguageDictionary LanguageDictionary { get; } = new();

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Gets the native name of this language.
    /// </summary>
    public string NativeName { get; }

    /// <inheritdoc/>
    public string ISO6391 { get; }

    /// <inheritdoc/>
    public string ISO6393 { get; }

    /// <summary>
    /// Gets supported translation services of this language.
    /// </summary>
    public TranslationServices SupportedServices { get; }

    /// <summary>
    /// Gets a language from a language code, name or alias.
    /// </summary>
    /// <param name="code">The language name or code. It can be a ISO 639-1 code, a ISO 639-3 code, a language name or a language alias.</param>
    /// <returns>The language, or null if the language was not found.</returns>
    public static Language GetLanguage(string code) => LanguageDictionary.GetLanguage(code);

    /// <summary>
    /// Tries to get a language from a language code, name or alias.
    /// </summary>
    /// <param name="code">The language name or code. It can be a ISO 639-1 code, a ISO 639-3 code, a language name or a language alias.</param>
    /// <param name="language">The language, if found.</param>
    /// <returns><see langword="true"/> if the language was found, otherwise <see langword="false"/>.</returns>
    public static bool TryGetLanguage(string code, [MaybeNullWhen(false)] out Language language) => LanguageDictionary.TryGetLanguage(code, out language);

    /// <summary>
    /// Returns whether <paramref name="service"/> supports this language.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <returns><see langword="true"/> if <paramref name="service"/> supports this language, otherwise <see langword="false"/>.</returns>
    public bool IsServiceSupported(TranslationServices service) => (SupportedServices & service) == service;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Language);

    /// <inheritdoc/>
    public bool Equals(Language? other) => other != null && ISO6391 == other.ISO6391;

    /// <inheritdoc/>
    public override int GetHashCode() => ISO6391.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(Name)}: '{Name}', {nameof(NativeName)}: '{NativeName}', {nameof(ISO6391)}: {ISO6391}, {nameof(ISO6393)}: {ISO6393}";

    private string DebuggerDisplay => ToString();
}