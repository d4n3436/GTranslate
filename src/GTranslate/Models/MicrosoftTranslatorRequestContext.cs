using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftTranslatorRequest[]))]
internal sealed partial class MicrosoftTranslatorRequestContext : JsonSerializerContext;