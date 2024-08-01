using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class YandexLanguageDetectionResultModel
{
    [JsonPropertyName("code")]
    public required HttpStatusCode Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("lang")]
    public string? Lang { get; set; }

    [MemberNotNullWhen(true, nameof(Lang))]
    public bool IsSuccessful => Code == HttpStatusCode.OK;
}