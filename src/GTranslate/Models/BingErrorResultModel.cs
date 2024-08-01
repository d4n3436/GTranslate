﻿using System.Net;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class BingErrorResultModel
{
    [JsonPropertyName("code")]
    public required HttpStatusCode Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}