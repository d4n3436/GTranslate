using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GTranslate;

namespace LanguageScraper;

public class YandexLanguageScraper : ILanguageScraper
{
    private static ReadOnlySpan<byte> TranslatorLangsStart => Encoding.UTF8.GetBytes("TRANSLATOR_LANGS: ");

    private static ReadOnlySpan<byte> TranslatorLangsEnd => Encoding.UTF8.GetBytes(",\n");

    private readonly HttpClient _httpClient = new();

    public YandexLanguageScraper()
    {
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");
    }

    public TranslationServices TranslationService => TranslationServices.Yandex;

    public IReadOnlyCollection<ILanguage> ExistingTtsLanguages => Array.Empty<ILanguage>();

    public async Task<LanguageData> GetLanguageDataAsync()
    {
        byte[] bytes = await _httpClient.GetByteArrayAsync(new Uri("https://translate.yandex.com/"));

        int start = bytes.AsSpan().IndexOf(TranslatorLangsStart) + TranslatorLangsStart.Length;
        int length = bytes.AsSpan(start..).IndexOf(TranslatorLangsEnd);

        var languages = JsonDocument.Parse(bytes.AsMemory(start, length))
            .RootElement
            .EnumerateObject()
            .Select(x => new ScrapedLanguage(x.Value.GetString()!, x.Name, "?", "?"))
            .ToArray();

        return new LanguageData { Languages = languages, TtsLanguages = Array.Empty<ILanguage>() };
    }
}