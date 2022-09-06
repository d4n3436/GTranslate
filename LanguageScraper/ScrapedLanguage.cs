using GTranslate;

namespace LanguageScraper;

public record ScrapedLanguage(string Name, string ISO6391, string ISO6393, string NativeName) : ILanguage
{
    public override string ToString() => $"Name: '{Name}', NativeName: '{NativeName}' ISO6391: {ISO6391}, ISO6393: {ISO6393}";
}