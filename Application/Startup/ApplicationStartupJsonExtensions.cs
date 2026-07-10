#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace CoubDownloader.Application.Startup;

/// <summary>System.Text.Json serialization extensions for ApplicationStartup</summary>
public static class ApplicationStartupJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    /// <summary>Serialize ApplicationStartup to JSON string</summary>
    /// <param name="value">The ApplicationStartup instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this ApplicationStartup value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>Deserialize ApplicationStartup from JSON string</summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized ApplicationStartup instance, or null if JSON is empty</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized</exception>
    public static ApplicationStartup? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ApplicationStartup>(json, _jsonOptions);
    }

    /// <summary>Try to deserialize ApplicationStartup from JSON string</summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for deserialized value</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    public static bool TryFromJson(string json, out ApplicationStartup? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ApplicationStartup>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
