using System.Threading;
using System.Threading.Tasks;
using GTranslate.Results;
using JetBrains.Annotations;

namespace GTranslate.Translators;

/// <summary>
/// Represents a translator that can perform explicit dictionary lookups.
/// </summary>
/// <remarks>
/// The sections a lookup can return depend on the service; see the documentation of each implementation of
/// <see cref="LookupDictionaryAsync(string, ILanguage, ILanguage, CancellationToken)"/>.
/// </remarks>
[PublicAPI]
public interface IDictionaryTranslator : ITranslator
{
    /// <summary>
    /// Looks up dictionary information for a word or short phrase.
    /// </summary>
    /// <param name="text">The word or short phrase to look up.</param>
    /// <param name="toLanguage">The target language.</param>
    /// <param name="fromLanguage">The source language.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>A task containing the dictionary result. An empty group list means that no dictionary entry exists.</returns>
    Task<IDictionaryResult> LookupDictionaryAsync(string text, string toLanguage, string fromLanguage, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="LookupDictionaryAsync(string, string, string, CancellationToken)"/>
    Task<IDictionaryResult> LookupDictionaryAsync(string text, ILanguage toLanguage, ILanguage fromLanguage, CancellationToken cancellationToken = default);
}
