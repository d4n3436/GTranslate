using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class BingDetectedLanguageModel
{
    [JsonPropertyName("language")]
    public required string Language { get; set; }

    [JsonPropertyName("score")]
    public float? Score { get; set; }
}