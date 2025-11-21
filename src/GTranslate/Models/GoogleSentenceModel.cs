using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleSentenceModel
{
    [JsonPropertyName("trans")]
    public required string Translation { get; set; }

    [JsonPropertyName("translit")]
    public string? Transliteration { get; set; }
}