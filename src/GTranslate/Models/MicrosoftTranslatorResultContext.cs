using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftTranslatorResult[]))]
internal partial class MicrosoftTranslatorResultContext : JsonSerializerContext;