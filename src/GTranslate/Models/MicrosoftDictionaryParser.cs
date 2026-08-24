using System;
using System.Collections.Generic;
using System.Linq;
using GTranslate.Results;

namespace GTranslate.Models;

internal static class MicrosoftDictionaryParser
{
    public static DictionaryResult Parse(IReadOnlyList<MicrosoftDictionaryResultModel> models,
        IReadOnlyList<MicrosoftDictionaryExamplesResultModel> exampleModels, string source,
        Language targetLanguage, Language sourceLanguage, string service)
    {
        var model = models.FirstOrDefault();
        var groups = model?.Translations?
            .Where(static x => !string.IsNullOrWhiteSpace(x.DisplayTarget) || !string.IsNullOrWhiteSpace(x.NormalizedTarget))
            .GroupBy(static x => x.PartOfSpeech, StringComparer.OrdinalIgnoreCase)
            .Select(group => (IDictionaryGroup)new DictionaryGroup(group.Key,
                group.Select(x => (IDictionaryEntry)new DictionaryEntry(
                    x.DisplayTarget ?? x.NormalizedTarget!,
                    x.Confidence,
                    backTranslations: x.BackTranslations?
                        .Select(static y => y.DisplayText ?? y.NormalizedText)
                        .Where(static y => !string.IsNullOrWhiteSpace(y))
                        .Select(static y => y!)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray(),
                    examples: FindExamples(exampleModels, x.NormalizedTarget ?? x.DisplayTarget!),
                    normalizedText: x.NormalizedTarget,
                    prefix: x.PrefixWord))
                    .ToArray()))
            .ToArray() ?? Array.Empty<IDictionaryGroup>();

        return new DictionaryResult(source, service, targetLanguage, sourceLanguage,
            model?.DisplaySource ?? model?.NormalizedSource ?? source, groups: groups);
    }

    private static IReadOnlyList<IDictionaryExample> FindExamples(IReadOnlyList<MicrosoftDictionaryExamplesResultModel> models, string target)
    {
        return models
            .Where(x => string.Equals(x.NormalizedTarget, target, StringComparison.OrdinalIgnoreCase))
            .SelectMany(static x => x.Examples ?? Array.Empty<MicrosoftDictionaryExampleModel>())
            .Select(static x => (IDictionaryExample)new DictionaryExample(
                string.Concat(x.SourcePrefix, x.SourceTerm, x.SourceSuffix),
                string.Concat(x.TargetPrefix, x.TargetTerm, x.TargetSuffix)))
            .ToArray();
    }
}
