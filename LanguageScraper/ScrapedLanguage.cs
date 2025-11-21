using GTranslate;

namespace LanguageScraper;

// ReSharper disable InconsistentNaming
public record ScrapedLanguage(string Name, string ISO6391, string ISO6393, string NativeName) : ILanguage
// ReSharper restore InconsistentNaming
{
    public override string ToString() => $"Name: '{Name}', NativeName: '{NativeName}' ISO6391: {ISO6391}, ISO6393: {ISO6393}";
}