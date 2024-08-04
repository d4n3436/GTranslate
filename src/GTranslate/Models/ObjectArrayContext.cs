using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(object?[][][]))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
internal sealed partial class ObjectArrayContext : JsonSerializerContext;