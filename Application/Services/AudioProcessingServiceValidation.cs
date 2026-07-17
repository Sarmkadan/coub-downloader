#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Provides validation helpers for <see cref="AudioProcessingService"/> instances.
/// </summary>
public static class AudioProcessingServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="AudioProcessingService"/> instance.
    /// </summary>
    /// <remarks>
    /// AudioProcessingService has no public properties to validate.
    /// The IFFmpegWrapper dependency is validated at construction time.
    /// </remarks>
    /// <param name="value">The audio processing service to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate([NotNull] this AudioProcessingService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AudioProcessingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The audio processing service to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this AudioProcessingService? value) => value is not null && Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="AudioProcessingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The audio processing service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid([NotNull] this AudioProcessingService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AudioProcessingService is invalid. Problems:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}",
                nameof(value));
        }
    }
}