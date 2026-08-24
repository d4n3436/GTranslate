using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(BingDictionaryResultModel[]))]
internal sealed partial class BingDictionaryResultModelContext : JsonSerializerContext;
