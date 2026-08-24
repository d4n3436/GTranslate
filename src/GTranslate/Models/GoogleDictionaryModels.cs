using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace GTranslate.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleDictionaryGroupModel
{
    [JsonPropertyName("pos")]
    public string? PartOfSpeech { get; set; }

    [JsonPropertyName("entry")]
    public IReadOnlyList<GoogleDictionaryEntryModel>? Entries { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleDictionaryEntryModel
{
    [JsonPropertyName("word")]
    public string? Word { get; set; }

    [JsonPropertyName("reverse_translation")]
    public IReadOnlyList<string>? ReverseTranslations { get; set; }

    [JsonPropertyName("score")]
    public double? Score { get; set; }

    [JsonPropertyName("frequency")]
    public long? Frequency { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleAlternativeTranslationGroupModel
{
    [JsonPropertyName("alternative")]
    public IReadOnlyList<GoogleAlternativeTranslationModel>? Alternatives { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleAlternativeTranslationModel
{
    [JsonPropertyName("word_postproc")]
    public string? Word { get; set; }

    [JsonPropertyName("score")]
    public double? Score { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleDefinitionGroupModel
{
    [JsonPropertyName("pos")]
    public string? PartOfSpeech { get; set; }

    [JsonPropertyName("entry")]
    public IReadOnlyList<GoogleDefinitionModel>? Entries { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleDefinitionModel
{
    [JsonPropertyName("gloss")]
    public string? Gloss { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleSynsetGroupModel
{
    [JsonPropertyName("pos")]
    public string? PartOfSpeech { get; set; }

    [JsonPropertyName("entry")]
    public IReadOnlyList<GoogleSynsetModel>? Entries { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleSynsetModel
{
    [JsonPropertyName("synonym")]
    public IReadOnlyList<string>? Synonyms { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleExamplesModel
{
    [JsonPropertyName("example")]
    public IReadOnlyList<GoogleExampleModel>? Examples { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class GoogleExampleModel
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
