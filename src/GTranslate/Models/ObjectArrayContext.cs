using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(object?[][][]))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
internal partial class ObjectArrayContext : JsonSerializerContext;