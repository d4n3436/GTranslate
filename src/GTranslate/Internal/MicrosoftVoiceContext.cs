using System.Text.Json.Serialization;

namespace GTranslate;


[JsonSerializable(typeof(MicrosoftVoice[]))]
internal partial class MicrosoftVoiceContext : JsonSerializerContext
{
}

[JsonSerializable(typeof(string))]
internal partial class StringContext : JsonSerializerContext
{
}