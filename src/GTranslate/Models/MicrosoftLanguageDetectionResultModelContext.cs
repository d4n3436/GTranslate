using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftLanguageDetectionResultModel[]))]
internal sealed partial class MicrosoftLanguageDetectionResultModelContext : JsonSerializerContext;