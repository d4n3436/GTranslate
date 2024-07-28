using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(YandexTranslationResultModel))]
internal partial class YandexTranslationResultModelContext : JsonSerializerContext;