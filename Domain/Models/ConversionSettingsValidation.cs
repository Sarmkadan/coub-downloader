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
    /// <param name="value">The conversion settings to validate</param>
    /// <returns>An enumerable of validation messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this ConversionSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id cannot be null or whitespace.");
        }

        // Validate Format (enum has valid values by design)
        // No validation needed - enum is strongly typed

        // Validate Quality (enum has valid values by design)
        // No validation needed - enum is strongly typed

        // Validate VideoBitrate
        if (value.VideoBitrate < 500 || value.VideoBitrate > 20000)
        {
            errors.Add("VideoBitrate must be between 500 and 20000 kbps.");
        }

        // Validate AudioBitrate
        if (value.AudioBitrate < 32 || value.AudioBitrate > 320)
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

        // Validate FrameRate
        if (value.FrameRate < 15 || value.FrameRate > 120)
        {
            errors.Add("FrameRate must be between 15 and 120 fps.");
        }

        // Validate Width
        if (value.Width < 100 || value.Width > 7680)
        {
            errors.Add("Width must be between 100 and 7680 pixels.");
        }

        // Validate Height
        if (value.Height < 100 || value.Height > 7680)
        {
            errors.Add("Height must be between 100 and 7680 pixels.");
        }

        // Validate AudioLoopStrategy (enum has valid values by design)
        // No validation needed - enum is strongly typed

        // Validate ThreadCount
        if (value.ThreadCount < 1 || value.ThreadCount > 32)
        {
            errors.Add("ThreadCount must be between 1 and 32.");
        }

        // Validate FadeInMs
        if (value.FadeInMs < 0 || value.FadeInMs > 5000)
        {
            errors.Add("FadeInMs must be between 0 and 5000 milliseconds.");
        }

        // Validate FadeOutMs
        if (value.FadeOutMs < 0 || value.FadeOutMs > 5000)
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
    /// <param name="value">The conversion settings to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this ConversionSettings value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified conversion settings are valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The conversion settings to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing the list of problems</exception>
    public static void EnsureValid(this ConversionSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "ConversionSettings validation failed. Problems:\n" + string.Join("\n", errors),
                nameof(value));
        }
    }
}