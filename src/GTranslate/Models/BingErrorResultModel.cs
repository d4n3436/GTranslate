using System.Net;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class BingErrorResultModel
{
    [JsonPropertyName("statusCode")]
    public required HttpStatusCode StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}