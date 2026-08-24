using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class MicrosoftDictionaryResultModel
{
    [JsonPropertyName("normalizedSource")]
    public string? NormalizedSource { get; set; }

    [JsonPropertyName("displaySource")]
    public string? DisplaySource { get; set; }

    [JsonPropertyName("translations")]
    public IReadOnlyList<MicrosoftDictionaryTranslationModel>? Translations { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class MicrosoftDictionaryTranslationModel
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
    public IReadOnlyList<MicrosoftBackTranslationModel>? BackTranslations { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class MicrosoftBackTranslationModel
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

internal sealed class MicrosoftDictionaryExampleRequestModel
{
    [JsonPropertyName("Text")]
    public required string Text { get; set; }

    [JsonPropertyName("Translation")]
    public required string Translation { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class MicrosoftDictionaryExamplesResultModel
{
    [JsonPropertyName("normalizedSource")]
    public string? NormalizedSource { get; set; }

    [JsonPropertyName("normalizedTarget")]
    public string? NormalizedTarget { get; set; }

    [JsonPropertyName("examples")]
    public IReadOnlyList<MicrosoftDictionaryExampleModel>? Examples { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class MicrosoftDictionaryExampleModel
{
    [JsonPropertyName("sourcePrefix")]
    public string? SourcePrefix { get; set; }

    [JsonPropertyName("sourceTerm")]
    public string? SourceTerm { get; set; }

    [JsonPropertyName("sourceSuffix")]
    public string? SourceSuffix { get; set; }

    [JsonPropertyName("targetPrefix")]
    public string? TargetPrefix { get; set; }

    [JsonPropertyName("targetTerm")]
    public string? TargetTerm { get; set; }

    [JsonPropertyName("targetSuffix")]
    public string? TargetSuffix { get; set; }
}
