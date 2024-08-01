using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class MicrosoftTransliterationResultModel
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("script")]
    public required string Script { get; set; }
}