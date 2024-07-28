using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal class GoogleSentenceModel
{
    [JsonPropertyName("trans")]
    public required string Translation { get; set; }

    [JsonPropertyName("translit")]
    public string? Transliteration { get; set; }
}