using System.Net;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class BingErrorResultModel
{
    [JsonPropertyName("statusCode")]
    public required HttpStatusCode StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}