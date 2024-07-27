using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftTransliterationResultModel[]))]
internal partial class MicrosoftTransliterationResultModelContext : JsonSerializerContext;