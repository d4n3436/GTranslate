using System.Text.Json.Serialization;

namespace GTranslate;

[JsonSerializable(typeof(MicrosoftVoice[]))]
internal sealed partial class MicrosoftVoiceContext : JsonSerializerContext;