using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftTranslatorRequest[]))]
internal partial class MicrosoftTranslatorRequestContext : JsonSerializerContext;