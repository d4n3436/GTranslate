using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal class YandexTranslationResultModel
{
    [JsonPropertyName("code")]
    public required HttpStatusCode Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("text")]
    public IReadOnlyList<string>? Text { get; set; }

    [JsonPropertyName("lang")]
    public string? Lang { get; set; }

    [MemberNotNullWhen(true, nameof(Text), nameof(Lang))]
    public bool IsSuccessful => Code == HttpStatusCode.OK;
}