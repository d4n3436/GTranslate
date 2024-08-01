using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class MicrosoftTranslatorRequest
{
    [JsonPropertyName("Text")]
    public required string Text { get; set; }
}