#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>System.Text.Json serialization helpers for <see cref="ValidationHelper"/> type metadata</summary>
public static class ValidationHelperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>Serialize validation helper type metadata to JSON string</summary>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON representation of validation helper type information</returns>
    public static string ToJson(bool indented = false)
    {
        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        var typeInfo = new
        {
            Type = nameof(ValidationHelper),
            Assembly = typeof(ValidationHelper).Assembly.GetName().Name,
            Methods = new[]
            {
                nameof(ValidationHelper.IsValidEmail),
                nameof(ValidationHelper.IsValidUrl),
                nameof(ValidationHelper.IsValidIpAddress),
                nameof(ValidationHelper.IsValidFilePath),
                nameof(ValidationHelper.SanitizeFileName),
                nameof(ValidationHelper.IsValidCoubUrl),
                nameof(ValidationHelper.IsValidBitrate),
                nameof(ValidationHelper.IsValidResolution),
                nameof(ValidationHelper.IsValidFrameRate),
                nameof(ValidationHelper.IsValidDuration),
                nameof(ValidationHelper.IsSafeDirectoryPath),
                nameof(ValidationHelper.IsValidBatchSize),
                nameof(ValidationHelper.MatchesPattern)
            }
        };

        return JsonSerializer.Serialize(typeInfo, options);
    }

    /// <summary>Deserialize JSON string to validation helper type information</summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>A simple object with type information, or <see langword="null"/> if <paramref name="json"/> is empty or whitespace</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    /// <exception cref="JsonException">Thrown when JSON is malformed or cannot be deserialized</exception>
    public static object? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
    }

    /// <summary>Attempt to deserialize JSON string to validation helper type information</summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized object if successful</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}