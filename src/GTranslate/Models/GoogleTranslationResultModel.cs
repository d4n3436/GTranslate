using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class GoogleTranslationResultModel
{
    [JsonPropertyName("sentences")]
    public IReadOnlyList<GoogleSentenceModel>? Sentences { get; set; }

    [JsonPropertyName("src")]
    public required string Source { get; set; }

    [JsonPropertyName("confidence")]
    public float? Confidence { get; set; }
}