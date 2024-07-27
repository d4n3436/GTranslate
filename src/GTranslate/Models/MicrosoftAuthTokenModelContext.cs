using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftAuthTokenModel))]
internal partial class MicrosoftAuthTokenModelContext : JsonSerializerContext;