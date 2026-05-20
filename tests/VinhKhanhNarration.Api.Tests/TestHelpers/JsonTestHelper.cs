using System.Text.Json;

namespace VinhKhanhNarration.Api.Tests.TestHelpers;

public static class JsonTestHelper
{
    public static long? FindFirstLong(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetPropertyCaseInsensitive(element, name, out var property))
            {
                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var longValue))
                {
                    return longValue;
                }

                if (property.ValueKind == JsonValueKind.String && long.TryParse(property.GetString(), out var parsed))
                {
                    return parsed;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var nested = FindFirstLong(property.Value, names);
                if (nested.HasValue)
                {
                    return nested;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = FindFirstLong(item, names);
                if (nested.HasValue)
                {
                    return nested;
                }
            }
        }

        return null;
    }

    public static string? FindFirstString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetPropertyCaseInsensitive(element, name, out var property))
            {
                if (property.ValueKind == JsonValueKind.String)
                {
                    return property.GetString();
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var nested = FindFirstString(property.Value, names);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = FindFirstString(item, names);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        return null;
    }

    public static JsonElement? FindFirstObjectWhereStringEquals(JsonElement element, string propertyName, string expectedValue)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (TryGetPropertyCaseInsensitive(item, propertyName, out var property)
                    && property.ValueKind == JsonValueKind.String
                    && string.Equals(property.GetString(), expectedValue, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var nested = FindFirstObjectWhereStringEquals(property.Value, propertyName, expectedValue);
                if (nested.HasValue)
                {
                    return nested.Value;
                }
            }
        }

        return null;
    }

    private static bool TryGetPropertyCaseInsensitive(JsonElement element, string name, out JsonElement property)
    {
        property = default;

        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        return false;
    }
}
