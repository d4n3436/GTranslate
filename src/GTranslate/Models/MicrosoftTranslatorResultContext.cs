using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftTranslationResultModel[]))]
internal partial class MicrosoftTranslationResultModelContext : JsonSerializerContext;