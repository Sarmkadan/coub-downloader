#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="VideoSection"/> instances.
/// </summary>
public static class VideoSectionValidation
{
    /// <summary>
    /// Validates a <see cref="VideoSection"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The video section to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this VideoSection value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id must not be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.VideoId))
        {
            errors.Add("VideoId must not be null or whitespace");
        }

        // Validate Index (should be non-negative based on [Range(0, int.MaxValue)])
        if (value.Index < 0)
        {
            errors.Add("Index must be non-negative");
        }

        // Validate StartTime and EndTime consistency
        if (value.StartTime < 0)
        {
            errors.Add("StartTime must be non-negative");
        }

        if (value.EndTime < 0)
        {
            errors.Add("EndTime must be non-negative");
        }
        else if (value.EndTime <= value.StartTime)
        {
            errors.Add("EndTime must be greater than StartTime");
        }

        // Validate TransitionDurationMs (should be within [0, 5000] range)
        if (value.TransitionDurationMs < 0 || value.TransitionDurationMs > 5000)
        {
            errors.Add("TransitionDurationMs must be between 0 and 5000 milliseconds");
        }

        // Validate string length constraints
        if (value.Description is not null && value.Description.Length > 200)
        {
            errors.Add("Description must not exceed 200 characters");
        }

        if (value.TransitionEffect is not null && value.TransitionEffect.Length > 50)
        {
            errors.Add("TransitionEffect must not exceed 50 characters");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="VideoSection"/> instance is valid.
    /// </summary>
    /// <param name="value">The video section to check</param>
    /// <returns>True if the section is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this VideoSection value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="VideoSection"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The video section to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the section is invalid, with a message listing all problems</exception>
    public static void EnsureValid(this VideoSection value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"VideoSection is invalid:{Environment.NewLine} - {
                string.Join($"{Environment.NewLine} - ", errors)
            }");
    }
}