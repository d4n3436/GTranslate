using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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