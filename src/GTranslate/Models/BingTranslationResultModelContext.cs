using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(BingTranslationResultModel[]))]
internal sealed partial class BingTranslationResultModelContext : JsonSerializerContext;