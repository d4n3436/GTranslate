using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class BingTranslationResultModel
{
    [JsonPropertyName("detectedLanguage")]
    public BingDetectedLanguageModel? DetectedLanguage { get; set; }

    [JsonPropertyName("translations")]
    public IReadOnlyList<BingTranslationModel>? Translations { get; set; }

    [JsonPropertyName("inputTransliteration")]
    public string? InputTransliteration { get; set; }
}