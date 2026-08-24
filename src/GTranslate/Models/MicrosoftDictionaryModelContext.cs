using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftDictionaryResultModel[]))]
[JsonSerializable(typeof(MicrosoftDictionaryExampleRequestModel[]))]
[JsonSerializable(typeof(MicrosoftDictionaryExamplesResultModel[]))]
internal sealed partial class MicrosoftDictionaryModelContext : JsonSerializerContext;
