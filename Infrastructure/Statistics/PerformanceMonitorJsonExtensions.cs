#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoubDownloader.Infrastructure.Statistics;

/// <summary>System.Text.Json serialization extensions for <see cref="PerformanceMonitor"/> instances.</summary>
/// <remarks>Provides JSON serialization and deserialization methods with camelCase naming policy.
/// All methods throw <see cref="ArgumentNullException"/> for null inputs except where explicitly documented otherwise.</remarks>
public static class PerformanceMonitorJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
	};

	/// <summary>Serializes a <see cref="PerformanceMonitor"/> instance to a JSON string.</summary>
	/// <param name="value">The performance monitor instance to serialize. Cannot be null.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the performance monitor.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this PerformanceMonitor value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>Deserializes a <see cref="PerformanceMonitor"/> instance from a JSON string.</summary>
	/// <param name="json">The JSON string to deserialize. Can be null or empty.</param>
	/// <returns>The deserialized <see cref="PerformanceMonitor"/> instance, or null if the JSON is null or empty.</returns>
	/// <exception cref="JsonException">Thrown when the JSON is invalid and cannot be deserialized.</exception>
	public static PerformanceMonitor? FromJson(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return null;
		}

		return JsonSerializer.Deserialize<PerformanceMonitor>(json, _jsonOptions);
	}

	/// <summary>Attempts to deserialize a <see cref="PerformanceMonitor"/> instance from a JSON string.</summary>
	/// <param name="json">The JSON string to deserialize. Can be null or empty.</param>
	/// <param name="value">When this method returns, contains the deserialized instance or null if deserialization failed.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out PerformanceMonitor? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		if (string.IsNullOrEmpty(json))
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<PerformanceMonitor>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}