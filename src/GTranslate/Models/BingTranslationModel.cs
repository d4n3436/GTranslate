using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal class BingTranslationModel
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("to")]
    public required string To { get; set; }

    [JsonPropertyName("transliteration")]
    public BingTransliterationModel? Transliteration { get; set; }
}