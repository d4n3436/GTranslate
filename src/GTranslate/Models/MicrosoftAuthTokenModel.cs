using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal class MicrosoftAuthTokenModel
{
    [JsonPropertyName("t")]
    public required string Token { get; set; }

    [JsonPropertyName("r")]
    public required string Region { get; set; }
}