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
/// Provides validation helpers for <see cref="CoubVideo"/> instances.
/// </summary>
public static class CoubVideoValidation
{
    /// <summary>
    /// Validates a CoubVideo instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The video to validate</param>
    /// <returns>An empty list if valid, otherwise a list of error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this CoubVideo? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id must be a non-empty string");
        }

        if (string.IsNullOrWhiteSpace(value.Title))
        {
            errors.Add("Title must be a non-empty string");
        }

        if (string.IsNullOrWhiteSpace(value.Url))
        {
            errors.Add("Url must be a non-empty string");
        }
        else if (!Uri.IsWellFormedUriString(value.Url, UriKind.Absolute))
        {
            errors.Add("Url must be a well-formed absolute URI");
        }

        // Validate numeric properties
        if (value.Duration <= 0)
        {
            errors.Add("Duration must be greater than 0 seconds");
        }

        if (value.Width <= 0)
        {
            errors.Add("Width must be greater than 0 pixels");
        }

        if (value.Height <= 0)
        {
            errors.Add("Height must be greater than 0 pixels");
        }

        if (value.ViewCount < 0)
        {
            errors.Add("ViewCount cannot be negative");
        }

        // Validate optional string properties
        if (!string.IsNullOrWhiteSpace(value.SourceUrl) && !Uri.IsWellFormedUriString(value.SourceUrl, UriKind.Absolute))
        {
            errors.Add("SourceUrl must be a well-formed absolute URI or null/empty");
        }

        if (!string.IsNullOrWhiteSpace(value.ThumbnailUrl) && !Uri.IsWellFormedUriString(value.ThumbnailUrl, UriKind.Absolute))
        {
            errors.Add("ThumbnailUrl must be a well-formed absolute URI or null/empty");
        }

        if (value.CreatorName?.Length > 255)
        {
            errors.Add("CreatorName cannot exceed 255 characters");
        }

        if (value.Description?.Length > 1000)
        {
            errors.Add("Description cannot exceed 1000 characters");
        }

        // Validate date properties
        if (value.UploadedDate.HasValue && value.UploadedDate.Value > DateTime.UtcNow.AddDays(1))
        {
            errors.Add("UploadedDate cannot be in the future");
        }

        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime");
        }

        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt must be set to a valid DateTime");
        }

        // Validate audio track if present
        if (value.AudioTrack is not null)
        {
            errors.AddRange(value.AudioTrack.Validate());
        }

        // Validate video sections
        if (value.Sections is null)
        {
            errors.Add("Sections collection cannot be null");
        }
        else if (value.Sections.Count == 0)
        {
            errors.Add("Sections collection cannot be empty");
        }
        else
        {
            for (int i = 0; i < value.Sections.Count; i++)
            {
                var section = value.Sections[i];
                if (section is null)
                {
                    errors.Add($"Sections[{i}] cannot be null");
                    continue;
                }

                errors.AddRange(ValidateSection(section, i));
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a VideoSection instance and returns a list of validation problems.
    /// </summary>
    private static IReadOnlyList<string> ValidateSection(VideoSection section, int sectionIndex)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(section.Id))
        {
            errors.Add($"Sections[{sectionIndex}].Id must be a non-empty string");
        }

        if (string.IsNullOrWhiteSpace(section.VideoId))
        {
            errors.Add($"Sections[{sectionIndex}].VideoId must be a non-empty string");
        }

        if (section.Index < 0)
        {
            errors.Add($"Sections[{sectionIndex}].Index cannot be negative");
        }

        if (section.StartTime < 0)
        {
            errors.Add($"Sections[{sectionIndex}].StartTime cannot be negative");
        }

        if (section.EndTime <= section.StartTime)
        {
            errors.Add($"Sections[{sectionIndex}].EndTime must be greater than StartTime (duration: {section.GetDuration():0.###}s)");
        }

        if (section.Description?.Length > 200)
        {
            errors.Add($"Sections[{sectionIndex}].Description cannot exceed 200 characters");
        }

        if (section.TransitionDurationMs < 0 || section.TransitionDurationMs > 5000)
        {
            errors.Add($"Sections[{sectionIndex}].TransitionDurationMs must be between 0 and 5000 milliseconds");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates an AudioTrack instance and returns a list of validation problems.
    /// </summary>
    private static IReadOnlyList<string> ValidateAudioTrack(AudioTrack audioTrack)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(audioTrack.Id))
        {
            errors.Add("AudioTrack.Id must be a non-empty string");
        }

        if (string.IsNullOrWhiteSpace(audioTrack.VideoId))
        {
            errors.Add("AudioTrack.VideoId must be a non-empty string");
        }

        if (audioTrack.Duration <= 0)
        {
            errors.Add("AudioTrack.Duration must be greater than 0 seconds");
        }

        if (audioTrack.SampleRate < 8000 || audioTrack.SampleRate > 192000)
        {
            errors.Add("AudioTrack.SampleRate must be between 8000 and 192000 Hz");
        }

        if (audioTrack.Channels < 1 || audioTrack.Channels > 8)
        {
            errors.Add("AudioTrack.Channels must be between 1 and 8");
        }

        if (audioTrack.Bitrate < 16 || audioTrack.Bitrate > 320)
        {
            errors.Add("AudioTrack.Bitrate must be between 16 and 320 kbps");
        }

        if (string.IsNullOrWhiteSpace(audioTrack.Codec))
        {
            errors.Add("AudioTrack.Codec must be a non-empty string");
        }
        else if (audioTrack.Codec.Length > 50)
        {
            errors.Add("AudioTrack.Codec cannot exceed 50 characters");
        }

        if (audioTrack.LoopCount < 1 || audioTrack.LoopCount > 1000)
        {
            errors.Add("AudioTrack.LoopCount must be between 1 and 1000");
        }

        if (audioTrack.FadeInMs < 0 || audioTrack.FadeInMs > 5000)
        {
            errors.Add("AudioTrack.FadeInMs must be between 0 and 5000 milliseconds");
        }

        if (audioTrack.FadeOutMs < 0 || audioTrack.FadeOutMs > 5000)
        {
            errors.Add("AudioTrack.FadeOutMs must be between 0 and 5000 milliseconds");
        }

        if (audioTrack.VolumeLevel < 0.0 || audioTrack.VolumeLevel > 2.0)
        {
            errors.Add("AudioTrack.VolumeLevel must be between 0.0 and 2.0");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a CoubVideo instance is valid.
    /// </summary>
    /// <param name="value">The video to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this CoubVideo? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a CoubVideo instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The video to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when value is invalid with detailed error messages</exception>
    public static void EnsureValid(this CoubVideo? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"CoubVideo is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Validates an AudioTrack instance.
    /// </summary>
    /// <param name="audioTrack">The audio track to validate</param>
    /// <returns>An empty list if valid, otherwise a list of error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when audioTrack is null</exception>
    public static IReadOnlyList<string> Validate(this AudioTrack? audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);
        return ValidateAudioTrack(audioTrack);
    }

    /// <summary>
    /// Determines whether an AudioTrack instance is valid.
    /// </summary>
    /// <param name="audioTrack">The audio track to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when audioTrack is null</exception>
    public static bool IsValid(this AudioTrack? audioTrack)
    {
        return audioTrack?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an AudioTrack instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="audioTrack">The audio track to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when audioTrack is null</exception>
    /// <exception cref="ArgumentException">Thrown when audioTrack is invalid with detailed error messages</exception>
    public static void EnsureValid(this AudioTrack? audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);
        var errors = ValidateAudioTrack(audioTrack);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AudioTrack is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

}