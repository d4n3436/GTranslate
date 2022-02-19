using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GTranslate;

/// <summary>
/// Represents a dictionary of languages.
/// </summary>
/// <typeparam name="TCode">The type of codes (or keys) associated with a language.</typeparam>
/// <typeparam name="TLanguage">The type of values that implements <see cref="ILanguage"/>.</typeparam>
public interface ILanguageDictionary<TCode, TLanguage> : IReadOnlyDictionary<TCode, TLanguage>
    where TLanguage : ILanguage
{
    /// <summary>
    /// Gets a language from a language code.
    /// </summary>
    /// <param name="code">The language code.</param>
    /// <returns>The language, or default{TCode} if the language was not found.</returns>
    TLanguage GetLanguage(TCode code);

    /// <summary>
    /// Tries to get a language from a language code, name or alias.
    /// </summary>
    /// <param name="code">The language code.</param>
    /// <param name="language">The language, if found.</param>
    /// <returns><see langword="true"/> if the language was found, otherwise <see langword="false"/>.</returns>
    bool TryGetLanguage(TCode code, [MaybeNullWhen(false)] out TLanguage language);
}