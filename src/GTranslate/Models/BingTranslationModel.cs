using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class BingTranslationModel
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("to")]
    public required string To { get; set; }

    [JsonPropertyName("transliteration")]
    public BingTransliterationModel? Transliteration { get; set; }
}