using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal class BingTransliterationModel
{
    [JsonPropertyName("script")]
    public required string Script { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}