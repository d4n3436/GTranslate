using GTranslate;
using System.Collections.Generic;

namespace LanguageScraper;

public class LanguageData
{
    public IReadOnlyList<ILanguage> Languages { get; init; } = null!;

    public IReadOnlyList<ILanguage> TtsLanguages { get; init; } = null!;
}