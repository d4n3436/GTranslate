using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DeepLTranslatorRequest))]
internal sealed partial class DeepLTranslatorRequestContext : JsonSerializerContext;