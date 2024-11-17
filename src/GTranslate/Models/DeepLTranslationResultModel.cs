using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class DeepLTranslationResultModel
{
    [JsonPropertyName("error")]
    public DeepLErrorModel? Error { get; set; }

    [JsonPropertyName("result")]
    public DeepLResultModel? Result { get; set; }
}

internal sealed class DeepLErrorModel
{
    [JsonPropertyName("code")]
    public required int Code { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }
}

internal sealed class DeepLResultModel
{
    [JsonPropertyName("translations")]
    public required IReadOnlyList<DeepLTranslationModel> Translations { get; set; }

    [JsonPropertyName("source_lang")]
    public required string SourceLang { get; set; }

    [JsonPropertyName("detectedLanguages")]
    public required IReadOnlyDictionary<string, float> DetectedLanguages { get; set; }
}

internal sealed class DeepLTranslationModel
{
    [JsonPropertyName("beams")]
    public required IReadOnlyList<DeepLBeamModel> Beams { get; set; }
}

internal sealed class DeepLBeamModel
{
    [JsonPropertyName("sentences")]
    public required IReadOnlyList<DeepLSentenceModel> Sentences { get; set; }
}

internal sealed class DeepLSentenceModel
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}