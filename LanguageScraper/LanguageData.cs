using GTranslate;
using System.Collections.Generic;

namespace LanguageScraper;

public class LanguageData
{
    public required IReadOnlyList<ILanguage> Languages { get; init; }

    public required IReadOnlyList<ILanguage> TtsLanguages { get; init; }
}