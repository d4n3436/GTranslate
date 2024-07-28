using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(BingErrorResultModel))]
internal partial class BingErrorResultModelContext : JsonSerializerContext;