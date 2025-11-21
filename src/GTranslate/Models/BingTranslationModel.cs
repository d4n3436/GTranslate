using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class BingTranslationModel
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("to")]
    public required string To { get; set; }

    [JsonPropertyName("transliteration")]
    public BingTransliterationModel? Transliteration { get; set; }
}