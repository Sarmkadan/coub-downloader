#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="ConversionSettings"/> instances.
/// </summary>
public static class ConversionSettingsValidation
{
    /// <summary>
    /// Validates the specified conversion settings and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The conversion settings to validate.</param>
    /// <returns>An enumerable of validation messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ConversionSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id cannot be null or whitespace.");
        }

        // Validate VideoBitrate (redundant with [Range] but defensive)
        if (value.VideoBitrate is < 500 or > 20000)
        {
            errors.Add("VideoBitrate must be between 500 and 20000 kbps.");
        }

        // Validate AudioBitrate (redundant with [Range] but defensive)
        if (value.AudioBitrate is < 32 or > 320)
        {
            errors.Add("AudioBitrate must be between 32 and 320 kbps.");
        }

        // Validate VideoCodec
        if (string.IsNullOrWhiteSpace(value.VideoCodec))
        {
            errors.Add("VideoCodec cannot be null or whitespace.");
        }
        else if (value.VideoCodec.Length > 50)
        {
            errors.Add("VideoCodec cannot exceed 50 characters.");
        }

        // Validate AudioCodec
        if (string.IsNullOrWhiteSpace(value.AudioCodec))
        {
            errors.Add("AudioCodec cannot be null or whitespace.");
        }
        else if (value.AudioCodec.Length > 50)
        {
            errors.Add("AudioCodec cannot exceed 50 characters.");
        }

        // Validate FrameRate (redundant with [Range] but defensive)
        if (value.FrameRate is < 15 or > 120)
        {
            errors.Add("FrameRate must be between 15 and 120 fps.");
        }

        // Validate Width (redundant with [Range] but defensive)
        if (value.Width is < 100 or > 7680)
        {
            errors.Add("Width must be between 100 and 7680 pixels.");
        }

        // Validate Height (redundant with [Range] but defensive)
        if (value.Height is < 100 or > 7680)
        {
            errors.Add("Height must be between 100 and 7680 pixels.");
        }

        // Validate ThreadCount (redundant with [Range] but defensive)
        if (value.ThreadCount is < 1 or > 32)
        {
            errors.Add("ThreadCount must be between 1 and 32.");
        }

        // Validate FadeInMs (redundant with [Range] but defensive)
        if (value.FadeInMs is < 0 or > 5000)
        {
            errors.Add("FadeInMs must be between 0 and 5000 milliseconds.");
        }

        // Validate FadeOutMs (redundant with [Range] but defensive)
        if (value.FadeOutMs is < 0 or > 5000)
        {
            errors.Add("FadeOutMs must be between 0 and 5000 milliseconds.");
        }

        // Validate CreatedAt - should not be default(DateTime)
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified conversion settings are valid.
    /// </summary>
    /// <param name="value">The conversion settings to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ConversionSettings value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified conversion settings are valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The conversion settings to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing the list of problems.</exception>
    public static void EnsureValid(this ConversionSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ConversionSettings validation failed. Problems:{Environment.NewLine}" + string.Join(Environment.NewLine, errors),
                nameof(value));
        }
    }
}