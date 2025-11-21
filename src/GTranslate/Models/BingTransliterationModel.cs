using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class BingTransliterationModel
{
    [JsonPropertyName("script")]
    public required string Script { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}