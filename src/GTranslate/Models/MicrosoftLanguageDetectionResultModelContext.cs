using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftLanguageDetectionResultModel[]))]
internal partial class MicrosoftLanguageDetectionResultModelContext : JsonSerializerContext;