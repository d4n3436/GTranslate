using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(GoogleTranslationResultModel))]
internal sealed partial class GoogleTranslationResultModelContext : JsonSerializerContext;