using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace GTranslate.Extensions;

internal static class JsonElementExtensions
{
    public static JsonElement FirstOrDefault(this JsonElement element)
        => element.ValueKind == JsonValueKind.Array ? element.EnumerateArray().FirstOrDefault() : default;

    public static JsonElement ElementAtOrDefault(this JsonElement element, int index)
        => element.ValueKind == JsonValueKind.Array ? element.EnumerateArray().ElementAtOrDefault(index) : default;

    public static JsonElement LastOrDefault(this JsonElement element)
        => element.ValueKind == JsonValueKind.Array ? element.EnumerateArray().LastOrDefault() : default;

    public static JsonElement GetPropertyOrDefault(this JsonElement element, string propertyName)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var value) ? value : default;

    [return: NotNullIfNotNull("defaultValue")]
    public static string? GetStringOrDefault(this JsonElement element, string? defaultValue = null)
        => element.ValueKind is JsonValueKind.String or JsonValueKind.Null ? element.GetString() ?? defaultValue : defaultValue;

    public static int GetInt32OrDefault(this JsonElement element, int defaultValue = default)
        => element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out int value) ? value : defaultValue;

    public static bool TryGetInt32(this JsonElement element, string propertyName, out int value)
    {
        value = 0;
        var prop = element.GetPropertyOrDefault(propertyName);
        return prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out value);
    }

    public static bool TryGetSingle(this JsonElement element, string propertyName, out float value)
    {
        value = 0;
        var prop = element.GetPropertyOrDefault(propertyName);
        return prop.ValueKind == JsonValueKind.Number && prop.TryGetSingle(out value);
    }
}