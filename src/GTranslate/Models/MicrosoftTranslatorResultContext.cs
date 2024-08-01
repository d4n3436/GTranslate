using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftTranslationResultModel[]))]
internal sealed partial class MicrosoftTranslationResultModelContext : JsonSerializerContext;