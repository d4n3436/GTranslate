﻿using System.Text.Json.Serialization;

namespace GTranslate.Models;

[JsonSerializable(typeof(MicrosoftTransliterationResultModel[]))]
internal sealed partial class MicrosoftTransliterationResultModelContext : JsonSerializerContext;