using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal class MicrosoftTranslationModel
{
    [JsonPropertyName("to")]
    public required string To { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}