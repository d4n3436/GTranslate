using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(object?[][][]))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
internal sealed partial class ObjectArrayContext : JsonSerializerContext;