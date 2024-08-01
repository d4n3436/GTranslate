using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(YandexLanguageDetectionResultModel))]
internal sealed partial class YandexLanguageDetectionResultModelContext : JsonSerializerContext;