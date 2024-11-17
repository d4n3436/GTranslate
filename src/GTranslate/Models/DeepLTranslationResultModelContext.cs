using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(DeepLTranslationResultModel))]
internal sealed partial class DeepLTranslationResultModelContext : JsonSerializerContext;