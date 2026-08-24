using System;
using System.Collections.Generic;
using System.Linq;
using GTranslate.Results;

namespace GTranslate.Models;

internal static class BingDictionaryParser
{
    public static DictionaryResult Parse(IReadOnlyList<BingDictionaryResultModel> models, string source, Language targetLanguage, Language sourceLanguage, string service)
    {
        var model = models.FirstOrDefault();
        var groups = model?.Translations?
            .Where(static x => !string.IsNullOrWhiteSpace(x.DisplayTarget) || !string.IsNullOrWhiteSpace(x.NormalizedTarget))
            .GroupBy(static x => x.PartOfSpeech, StringComparer.OrdinalIgnoreCase)
            .Select(static group => (IDictionaryGroup)new DictionaryGroup(group.Key,
                group.Select(static x => (IDictionaryEntry)new DictionaryEntry(
                    x.DisplayTarget ?? x.NormalizedTarget!,
                    x.Confidence,
                    backTranslations: x.BackTranslations?
                        .Select(static y => y.DisplayText ?? y.NormalizedText)
                        .Where(static y => !string.IsNullOrWhiteSpace(y))
                        .Select(static y => y!)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray(),
                    normalizedText: x.NormalizedTarget,
                    prefix: x.PrefixWord,
                    transliteration: x.Transliteration))
                    .ToArray()))
            .ToArray() ?? Array.Empty<IDictionaryGroup>();

        return new DictionaryResult(source, service, targetLanguage, sourceLanguage,
            model?.DisplaySource ?? model?.NormalizedSource ?? source, groups: groups);
    }
}
