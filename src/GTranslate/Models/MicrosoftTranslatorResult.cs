using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal class MicrosoftTranslatorResult
{
    [JsonPropertyName("detectedLanguage")]
    public required MicrosoftTranslatorDetectedLanguage DetectedLanguage { get; set; }

    [JsonPropertyName("translations")]
    public required IReadOnlyList<MicrosoftTranslatorTranslation> Translations { get; set; }
}