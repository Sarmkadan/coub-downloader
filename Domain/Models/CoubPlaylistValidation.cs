#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="CoubPlaylist"/> instances.
/// </summary>
public static class CoubPlaylistValidation
{
    /// <summary>
    /// Validates the playlist and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The playlist to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CoubPlaylist? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Playlist Id cannot be null or whitespace.");
        }

        // Validate Title
        if (string.IsNullOrWhiteSpace(value.Title))
        {
            errors.Add("Playlist Title cannot be null or whitespace.");
        }
        else if (value.Title.Length > 255)
        {
            errors.Add("Playlist Title cannot exceed 255 characters.");
        }

        // Validate PlaylistUrl
        if (string.IsNullOrWhiteSpace(value.PlaylistUrl))
        {
            errors.Add("PlaylistUrl cannot be null or whitespace.");
        }
        else if (!Uri.IsWellFormedUriString(value.PlaylistUrl, UriKind.Absolute))
        {
            errors.Add("PlaylistUrl must be a well-formed absolute URI.");
        }

        // Validate Description length if present
        if (value.Description is not null && value.Description.Length > 500)
        {
            errors.Add("Playlist Description cannot exceed 500 characters.");
        }

        // Validate VideoUrls list
        if (value.VideoUrls is null)
        {
            errors.Add("VideoUrls collection cannot be null.");
        }
        else
        {
            if (value.VideoUrls.Count > 500)
            {
                errors.Add("VideoUrls collection cannot exceed 500 items.");
            }

            for (int i = 0; i < value.VideoUrls.Count; i++)
            {
                var url = value.VideoUrls[i];
                if (string.IsNullOrWhiteSpace(url))
                {
                    errors.Add($"VideoUrls[{i}] cannot be null or whitespace.");
                }
                else if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    errors.Add($"VideoUrls[{i}] must be a well-formed absolute URI. Actual: '{url}'");
                }
            }
        }

        // Validate MaxVideos
        if (value.MaxVideos.HasValue)
        {
            if (value.MaxVideos.Value < 1)
            {
                errors.Add("MaxVideos, if set, must be at least 1.");
            }
            else if (value.MaxVideos.Value > 500)
            {
                errors.Add("MaxVideos, if set, cannot exceed 500.");
            }
        }

        // Validate CreatedAt (should not be default/minimum)
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("CreatedAt must be in UTC timezone.");
        }

        // Validate FetchedAt
        if (value.FetchedAt.HasValue)
        {
            if (value.FetchedAt.Value == default)
            {
                errors.Add("FetchedAt cannot be the default DateTime value.");
            }
            else if (value.FetchedAt.Value.Kind != DateTimeKind.Utc)
            {
                errors.Add("FetchedAt must be in UTC timezone when set.");
            }

            // FetchedAt should not be in the future
            if (value.FetchedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("FetchedAt cannot be in the future.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the playlist is valid according to business rules.
    /// </summary>
    /// <param name="value">The playlist to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CoubPlaylist? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the playlist is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The playlist to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the playlist has validation errors.</exception>
    public static void EnsureValid(this CoubPlaylist? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"Playlist validation failed with {errors.Count} error(s):{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", errors)
            }",
            nameof(value));
    }
}
