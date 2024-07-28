using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(GoogleTranslationResultModel))]
internal partial class GoogleTranslationResultModelContext : JsonSerializerContext;