using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class MicrosoftAuthTokenModel
{
    [JsonPropertyName("t")]
    public required string Token { get; set; }

    [JsonPropertyName("r")]
    public required string Region { get; set; }
}