using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class BingTranslationResultModel
{
    [JsonPropertyName("detectedLanguage")]
    public BingDetectedLanguageModel? DetectedLanguage { get; set; }

    [JsonPropertyName("translations")]
    public IReadOnlyList<BingTranslationModel>? Translations { get; set; }

    [JsonPropertyName("inputTransliteration")]
    public string? InputTransliteration { get; set; }
}