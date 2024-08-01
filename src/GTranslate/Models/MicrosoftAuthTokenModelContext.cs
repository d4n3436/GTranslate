using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftAuthTokenModel))]
internal sealed partial class MicrosoftAuthTokenModelContext : JsonSerializerContext;