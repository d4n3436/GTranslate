using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class MicrosoftTranslationResultModel
{
    [JsonPropertyName("detectedLanguage")]
    public MicrosoftDetectedLanguageModel? DetectedLanguage { get; set; }

    [JsonPropertyName("translations")]
    public required IReadOnlyList<MicrosoftTranslationModel> Translations { get; set; }
}