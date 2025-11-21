using System.Text.Json.Serialization;

namespace GTranslate.Common;

[JsonSerializable(typeof(MicrosoftVoice[]))]
internal sealed partial class MicrosoftVoiceContext : JsonSerializerContext;