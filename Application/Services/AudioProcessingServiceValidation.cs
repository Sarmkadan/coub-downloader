#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Provides validation helpers for <see cref="AudioProcessingService"/> instances.
/// </summary>
public static class AudioProcessingServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="AudioProcessingService"/> instance.
    /// </summary>
    /// <param name="value">The audio processing service to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AudioProcessingService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate that the FFmpeg wrapper is not null
        // Note: This is validated in the constructor, so we don't need to check it here
        // as the service would fail to instantiate if it were null

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AudioProcessingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The audio processing service to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this AudioProcessingService? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="AudioProcessingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The audio processing service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this AudioProcessingService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"AudioProcessingService is not valid. Errors: {string.Join(", ", errors)}",
            nameof(value));
    }
}