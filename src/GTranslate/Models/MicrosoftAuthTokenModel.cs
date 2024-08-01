using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class MicrosoftAuthTokenModel
{
    [JsonPropertyName("t")]
    public required string Token { get; set; }

    [JsonPropertyName("r")]
    public required string Region { get; set; }
}