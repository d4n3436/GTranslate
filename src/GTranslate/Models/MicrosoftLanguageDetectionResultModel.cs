using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal class MicrosoftLanguageDetectionResultModel
{
    [JsonPropertyName("language")]
    public required string Language { get; set; }

    [JsonPropertyName("score")]
    public required float Score { get; set; }
}