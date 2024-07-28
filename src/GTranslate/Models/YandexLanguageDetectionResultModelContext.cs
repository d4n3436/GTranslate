using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(YandexLanguageDetectionResultModel))]
internal partial class YandexLanguageDetectionResultModelContext : JsonSerializerContext;