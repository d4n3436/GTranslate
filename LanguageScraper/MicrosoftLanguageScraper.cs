using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GTranslate;

namespace LanguageScraper;

public class MicrosoftLanguageScraper : ILanguageScraper
{
    private readonly HttpClient _httpClient = new();

    public MicrosoftLanguageScraper()
    {
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");
    }

    public TranslationServices TranslationService => TranslationServices.Microsoft;

    public IReadOnlyCollection<ILanguage> ExistingTtsLanguages => Array.Empty<ILanguage>();

    public async Task<LanguageData> GetLanguageDataAsync()
    {
        var stream = await _httpClient.GetStreamAsync(new Uri("https://api.cognitive.microsofttranslator.com/languages?api-version=3.0&scope=translation"));
        var document = await JsonDocument.ParseAsync(stream);

        var languages = document
            .RootElement
            .GetProperty("translation")
            .EnumerateObject()
            .Select(x => new ScrapedLanguage(x.Value.GetProperty("name").GetString()!, x.Name, string.Empty, x.Value.GetProperty("nativeName").GetString()!))
            .ToArray();

        return new LanguageData { Languages = languages, TtsLanguages = Array.Empty<ILanguage>() };
    }
}