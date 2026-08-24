using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleTranslationResultModel
{
    [JsonPropertyName("sentences")]
    public IReadOnlyList<GoogleSentenceModel>? Sentences { get; set; }

    [JsonPropertyName("src")]
    public required string Source { get; set; }

    [JsonPropertyName("confidence")]
    public float? Confidence { get; set; }

    [JsonPropertyName("dict")]
    public IReadOnlyList<GoogleDictionaryGroupModel>? Dictionary { get; set; }

    [JsonPropertyName("alternative_translations")]
    public IReadOnlyList<GoogleAlternativeTranslationGroupModel>? AlternativeTranslations { get; set; }

    [JsonPropertyName("definitions")]
    public IReadOnlyList<GoogleDefinitionGroupModel>? Definitions { get; set; }

    [JsonPropertyName("synsets")]
    public IReadOnlyList<GoogleSynsetGroupModel>? Synsets { get; set; }

    [JsonPropertyName("examples")]
    public GoogleExamplesModel? Examples { get; set; }
}
