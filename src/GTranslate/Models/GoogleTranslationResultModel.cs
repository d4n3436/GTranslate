using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleTranslationResultModel
{
    [JsonPropertyName("sentences")]
    public IReadOnlyList<GoogleSentenceModel>? Sentences { get; set; }

    [JsonPropertyName("src")]
    public required string Source { get; set; }

    [JsonPropertyName("confidence")]
    public float? Confidence { get; set; }
}