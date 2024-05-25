using System.Text.Json.Serialization;

namespace GTranslate;


[JsonSerializable(typeof(MicrosoftVoice[]))]
internal sealed partial class MicrosoftVoiceContext : JsonSerializerContext;

[JsonSerializable(typeof(string))]
internal sealed partial class StringContext : JsonSerializerContext;