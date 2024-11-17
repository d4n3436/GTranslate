using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GTranslate.Models;

internal sealed class DeepLTranslatorRequest
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    [JsonPropertyName("jsonrpc")]
    public required string JsonRpc { get; set; }

    [JsonPropertyName("method")]
    public required string Method { get; set; }

    [JsonPropertyName("params")]
    public required DeepLParams Params { get; set; }
}

internal sealed class DeepLParams
{
    [JsonPropertyName("jobs")]
    public required IReadOnlyList<DeepLJob> Jobs { get; set; }

    [JsonPropertyName("commonJobParams")]
    public required DeepLCommonJobParams CommonJobParams { get; set; }

    [JsonPropertyName("lang")]
    public required DeepLLang Lang { get; set; }

    [JsonPropertyName("priority")]
    public required int Priority { get; set; }

    [JsonPropertyName("timestamp")]
    public required long Timestamp { get; set; }
}

internal sealed class DeepLJob
{
    [JsonPropertyName("kind")]
    public required string Kind { get; set; }

    [JsonPropertyName("preferred_num_beams")]
    public required int PreferredNumBeams { get; set; }

    [JsonPropertyName("raw_en_context_after")]
    public required IReadOnlyList<string> RawEnContextAfter { get; set; }

    [JsonPropertyName("raw_en_context_before")]
    public required IReadOnlyList<string> RawEnContextBefore { get; set; }

    [JsonPropertyName("sentences")]
    public required IReadOnlyList<DeepLSentence> Sentences { get; set; }
}

internal sealed class DeepLSentence
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    [JsonPropertyName("prefix")]
    public required string Prefix { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

internal sealed class DeepLCommonJobParams
{
    [JsonPropertyName("mode")]
    public required string Mode { get; set; }

    [JsonPropertyName("regionalVariant")]
    public required string RegionalVariant { get; set; }
}

internal sealed class DeepLLang
{
    [JsonPropertyName("target_lang")]
    public required string TargetLang { get; set; }

    [JsonPropertyName("source_lang_user_selected")]
    public required string SourceLangUserSelected { get; set; }
}
