using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GTranslate;
using JetBrains.Annotations;

namespace LanguageScraper;

[UsedImplicitly]
public class YandexLanguageScraper : ILanguageScraper
{
    private static ReadOnlySpan<byte> TranslatorLanguagesStart => "TRANSLATOR_LANGS: "u8;

    private static ReadOnlySpan<byte> TranslatorLanguagesEnd => ",\n"u8;

    private static readonly Uri YandexTranslateUri = new("https://translate.yandex.com/");

    private readonly HttpClient _httpClient = new();

    public YandexLanguageScraper()
    {
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");
    }

    public TranslationServices TranslationService => TranslationServices.Yandex;

    public IReadOnlyCollection<ILanguage> ExistingTtsLanguages => [];

    public async Task<LanguageData> GetLanguageDataAsync()
    {
        byte[] bytes = await _httpClient.GetByteArrayAsync(YandexTranslateUri);
        var span = bytes.AsSpan();

        int start = span.IndexOf(TranslatorLanguagesStart) + TranslatorLanguagesStart.Length;
        int length = span[start..].IndexOf(TranslatorLanguagesEnd);

        var languages = JsonDocument.Parse(bytes.AsMemory(start, length))
            .RootElement
            .EnumerateObject()
            .Select(x => new ScrapedLanguage(x.Value.GetString()!, x.Name, "?", "?"))
            .ToArray();

        return new LanguageData { Languages = languages, TtsLanguages = [] };
    }
}