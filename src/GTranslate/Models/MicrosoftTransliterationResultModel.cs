using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class MicrosoftTransliterationResultModel
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("script")]
    public required string Script { get; set; }
}