using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GTranslate;
using GTranslate.Translators;

namespace LanguageScraper;

// For GoogleTranslator and GoogleTranslator2
public class GoogleLanguageScraper : ILanguageScraper
{
    private static ReadOnlySpan<byte> LanguagesId => Encoding.UTF8.GetBytes("n9wk7");

    private static ReadOnlySpan<byte> TtsLanguagesId => Encoding.UTF8.GetBytes("ycyxUb");

    private static ReadOnlySpan<byte> LanguagesStart => Encoding.UTF8.GetBytes("data:");

    private static ReadOnlySpan<byte> LanguagesEnd => Encoding.UTF8.GetBytes(", sideChannel");

    private static ReadOnlySpan<byte> NativeNamesStart => Encoding.UTF8.GetBytes("window.LanguageDisplays.nativeNames = ");

    private static ReadOnlySpan<byte> NativeNamesEnd => Encoding.UTF8.GetBytes(";window.LanguageDisplays.localNames");

    private readonly HttpClient _httpClient = new();

    public GoogleLanguageScraper()
    {
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");
    }

    public TranslationServices TranslationService => TranslationServices.Google;

    public IReadOnlyCollection<ILanguage> ExistingTtsLanguages => GoogleTranslator.TextToSpeechLanguages;

    public async Task<LanguageData> GetLanguageDataAsync()
    {
        byte[] bytes = await _httpClient.GetByteArrayAsync(new Uri("https://translate.google.com/"));

        var languages = await GetLanguagesAsync(bytes);
        var ttsLanguages = GetTtsLanguages(bytes);

        return new LanguageData { Languages = languages, TtsLanguages = ttsLanguages };
    }

    public async Task DisplayMissingLanguagesAsync()
    {
        var data = await GetLanguageDataAsync();

        foreach (var language in data.Languages)
        {
            if (Language.LanguageDictionary.TryGetLanguage(language.ISO6391, out var existing))
            {
                if (!existing.IsServiceSupported(TranslationService))
                {
                    Console.WriteLine($"Missing support for {TranslationService}: {existing}");
                }
            }
            else
            {
                Console.WriteLine($"Missing Language (from {TranslationService}): {language}");
            }
        }

        foreach (var language in data.TtsLanguages)
        {
            if (Language.LanguageDictionary.TryGetLanguage(language.ISO6391, out var existing))
            {
                if (ExistingTtsLanguages.All(x => x.ISO6391 != existing.ISO6391))
                {
                    Console.WriteLine($"Missing TTS support for {TranslationService}: {existing}");
                }
                else if (GoogleTranslator2.TextToSpeechLanguages.All(x => x.ISO6391 != existing.ISO6391))
                {
                    Console.WriteLine($"Missing TTS support for {nameof(GoogleTranslator2)}: {existing}");
                }
            }
            else
            {
                Console.WriteLine($"Missing Language (from {TranslationService} TTS list): {language}");
            }
        }
    }

    private static IReadOnlyList<ILanguage> GetTtsLanguages(byte[] htmlBytes)
    {
        return GetLanguageData(htmlBytes, TtsLanguagesId)
            .RootElement[0]
            .EnumerateArray()
            .Select(x => new ScrapedLanguage("?", x[0].ToString(), "?", "?"))
            .ToArray();
    }

    private async Task<IReadOnlyList<ILanguage>> GetLanguagesAsync(byte[] htmlBytes)
    {
        // Get language list (ISO code and English name)
        var dict = GetLanguageData(htmlBytes, LanguagesId)
            .RootElement[1]
            .Deserialize<string[][]>()!
            .ToDictionary(x => x[0], x => x[1]);

        // Get native names
        byte[] bytes = await _httpClient.GetByteArrayAsync(new Uri("https://ssl.gstatic.com/inputtools/js/ln/17/en.js"));

        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == (byte)'\'')
            {
                bytes[i] = (byte)'"'; // Replace single quotes with double quotes so it can be parsed as JSON.
            }
        }

        int start = bytes.AsSpan().IndexOf(NativeNamesStart) + NativeNamesStart.Length;
        int end = bytes.AsSpan().IndexOf(NativeNamesEnd);

        var nativeNames = JsonDocument.Parse(bytes.AsMemory(start..end))
            .Deserialize<Dictionary<string, string>>()!;

        return dict.Select(x => new ScrapedLanguage(
                Name: x.Value,
                ISO6391: x.Key,
                ISO6393: "?",
                NativeName: nativeNames.TryGetValue(x.Key, out string? nativeName) ? nativeName : "?"))
            .ToArray();
    }

    private static JsonDocument GetLanguageData(byte[] bytes, ReadOnlySpan<byte> id)
    {
        int idIndex = bytes.AsSpan().IndexOf(id);
        byte[] callbackStart = Encoding.UTF8.GetBytes($"AF_initDataCallback({{key: 'ds:{bytes[idIndex - 10] - '0'}'");

        int callbackIndex = bytes.AsSpan().IndexOf(callbackStart);

        int start = bytes.AsSpan(callbackIndex).IndexOf(LanguagesStart) + LanguagesStart.Length;
        int length = bytes.AsSpan(start + callbackIndex).IndexOf(LanguagesEnd);

        return JsonDocument.Parse(bytes.AsMemory(start + callbackIndex, length));
    }
}