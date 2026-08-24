using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class BingDictionaryResultModel
{
    [JsonPropertyName("normalizedSource")]
    public string? NormalizedSource { get; set; }

    [JsonPropertyName("displaySource")]
    public string? DisplaySource { get; set; }

    [JsonPropertyName("translations")]
    public IReadOnlyList<BingDictionaryTranslationModel>? Translations { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class BingDictionaryTranslationModel
{
    [JsonPropertyName("normalizedTarget")]
    public string? NormalizedTarget { get; set; }

    [JsonPropertyName("displayTarget")]
    public string? DisplayTarget { get; set; }

    [JsonPropertyName("posTag")]
    public string? PartOfSpeech { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("prefixWord")]
    public string? PrefixWord { get; set; }

    [JsonPropertyName("backTranslations")]
    public IReadOnlyList<BingBackTranslationModel>? BackTranslations { get; set; }

    [JsonPropertyName("transliteration")]
    public string? Transliteration { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class BingBackTranslationModel
{
    [JsonPropertyName("normalizedText")]
    public string? NormalizedText { get; set; }

    [JsonPropertyName("displayText")]
    public string? DisplayText { get; set; }

    [JsonPropertyName("numExamples")]
    public int? NumberOfExamples { get; set; }

    [JsonPropertyName("frequencyCount")]
    public long? FrequencyCount { get; set; }
}
