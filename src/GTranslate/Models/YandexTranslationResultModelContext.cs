using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(YandexTranslationResultModel))]
internal sealed partial class YandexTranslationResultModelContext : JsonSerializerContext;