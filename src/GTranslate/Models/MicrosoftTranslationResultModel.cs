using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class MicrosoftTranslationResultModel
{
    [JsonPropertyName("detectedLanguage")]
    public required MicrosoftDetectedLanguageModel DetectedLanguage { get; set; }

    [JsonPropertyName("translations")]
    public required IReadOnlyList<MicrosoftTranslationModel> Translations { get; set; }
}