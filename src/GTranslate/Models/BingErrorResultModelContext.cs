using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(BingErrorResultModel))]
internal sealed partial class BingErrorResultModelContext : JsonSerializerContext;