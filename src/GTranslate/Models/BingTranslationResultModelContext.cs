using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(BingTranslationResultModel[]))]
internal partial class BingTranslationResultModelContext : JsonSerializerContext;