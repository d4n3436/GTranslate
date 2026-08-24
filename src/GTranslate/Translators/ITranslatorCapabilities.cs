using JetBrains.Annotations;

namespace GTranslate.Translators;

/// <summary>
/// Exposes the capabilities of a translator without changing the existing <see cref="ITranslator"/> contract.
/// </summary>
[PublicAPI]
public interface ITranslatorCapabilities
{
    /// <summary>
    /// Gets the capabilities supported natively by this translator.
    /// </summary>
    TranslationServiceCapabilities Capabilities { get; }
}
