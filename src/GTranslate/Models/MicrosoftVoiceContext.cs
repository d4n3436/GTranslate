using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftVoice[]))]
internal sealed partial class MicrosoftVoiceContext : JsonSerializerContext;