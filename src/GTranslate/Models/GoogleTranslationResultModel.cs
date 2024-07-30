using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal class GoogleTranslationResultModel
{
    [JsonPropertyName("sentences")]
    public IReadOnlyList<GoogleSentenceModel>? Sentences { get; set; }

    [JsonPropertyName("src")]
    public required string Source { get; set; }

    [JsonPropertyName("confidence")]
    public required float Confidence { get; set; }
}