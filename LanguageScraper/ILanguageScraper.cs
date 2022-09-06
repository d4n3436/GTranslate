using GTranslate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguageScraper;

public interface ILanguageScraper
{
    TranslationServices TranslationService { get; }

    IReadOnlyCollection<ILanguage> ExistingTtsLanguages { get; }

    Task<LanguageData> GetLanguageDataAsync();

    async Task DisplayMissingLanguagesAsync()
    {
        var data = await GetLanguageDataAsync();

        // Languages
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

        // TTS Languages
        foreach (var language in data.TtsLanguages)
        {
            if (Language.LanguageDictionary.TryGetLanguage(language.ISO6391, out var existing))
            {
                if (ExistingTtsLanguages.All(x => x.ISO6391 != existing.ISO6391))
                {
                    Console.WriteLine($"Missing TTS support for {TranslationService}: {existing}");
                }
            }
            else
            {
                Console.WriteLine($"Missing Language (from {TranslationService} TTS list): {language}");
            }
        }
    }
}