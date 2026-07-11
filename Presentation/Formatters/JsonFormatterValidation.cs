#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Presentation.Formatters;

/// <summary>
/// Provides validation helpers for <see cref="JsonFormatter"/> to ensure
/// the parameters passed to its formatting methods are valid before serialization.
/// </summary>
public static class JsonFormatterValidation
{
    /// <summary>
    /// Validates a <see cref="JsonFormatter"/> instance and its configuration.
    /// </summary>
    /// <param name="value">The formatter instance to validate</param>
    /// <returns>A list of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this JsonFormatter value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // JsonFormatter itself has no state to validate
        // The validation is for the parameters passed to its methods

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="JsonFormatter.FormatVideo(CoubVideo)"/>
    /// </summary>
    /// <param name="video">The video to validate</param>
    /// <returns>A list of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="video"/> is null</exception>
    public static IReadOnlyList<string> Validate(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(video.Id))
        {
            problems.Add("Video.Id cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(video.Title))
        {
            problems.Add("Video.Title cannot be null or whitespace.");
        }
        else if (video.Title.Length > 500)
        {
            problems.Add("Video.Title exceeds maximum length of 500 characters.");
        }

        if (string.IsNullOrWhiteSpace(video.Url))
        {
            problems.Add("Video.Url cannot be null or whitespace.");
        }

        if (video.Duration <= 0)
        {
            problems.Add("Video.Duration must be greater than 0 seconds.");
        }

        if (video.Width < 100 || video.Width > 7680)
        {
            problems.Add("Video.Width must be between 100 and 7680 pixels.");
        }

        if (video.Height < 100 || video.Height > 7680)
        {
            problems.Add("Video.Height must be between 100 and 7680 pixels.");
        }

        if (video.ViewCount < 0)
        {
            problems.Add("Video.ViewCount cannot be negative.");
        }

        if (video.UploadedDate.HasValue && video.UploadedDate.Value == default)
        {
            problems.Add("Video.UploadedDate cannot be the default DateTime value.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="JsonFormatter.FormatVideos(IEnumerable{CoubVideo})"/>
    /// </summary>
    /// <param name="videos">The videos to validate</param>
    /// <returns>A list of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="videos"/> is null</exception>
    public static IReadOnlyList<string> Validate(this IEnumerable<CoubVideo> videos)
    {
        ArgumentNullException.ThrowIfNull(videos);

        var problems = new List<string>();

        if (!videos.Any())
        {
            problems.Add("Video collection cannot be empty.");
        }

        var index = 0;
        foreach (var video in videos)
        {
            if (video is null)
            {
                problems.Add($"Video at index {index} is null.");
            }
            else
            {
                problems.AddRange(Validate(video).Select(p => $"Video[{index}]: {p}"));
            }

            index++;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="JsonFormatter.FormatBatchJob(BatchJob)"/>
    /// </summary>
    /// <param name="batch">The batch job to validate</param>
    /// <returns>A list of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="batch"/> is null</exception>
    public static IReadOnlyList<string> Validate(this BatchJob batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(batch.Id))
        {
            problems.Add("BatchJob.Id cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(batch.Name))
        {
            problems.Add("BatchJob.Name cannot be null or whitespace.");
        }
        else if (batch.Name.Length > 255)
        {
            problems.Add("BatchJob.Name exceeds maximum length of 255 characters.");
        }

        if (string.IsNullOrWhiteSpace(batch.OutputDirectory))
        {
            problems.Add("BatchJob.OutputDirectory cannot be null or whitespace.");
        }

        if (batch.TotalTasks < 0)
        {
            problems.Add("BatchJob.TotalTasks cannot be negative.");
        }

        if (batch.MaxParallelTasks < 1 || batch.MaxParallelTasks > 10)
        {
            problems.Add("BatchJob.MaxParallelTasks must be between 1 and 10.");
        }

        if (batch.CreatedAt == default)
        {
            problems.Add("BatchJob.CreatedAt cannot be the default DateTime value.");
        }

        if (batch.UpdatedAt == default)
        {
            problems.Add("BatchJob.UpdatedAt cannot be the default DateTime value.");
        }

        if (batch.Tasks is null)
        {
            problems.Add("BatchJob.Tasks collection cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="JsonFormatter.FormatSettings(ConversionSettings)"/>
    /// </summary>
    /// <param name="settings">The conversion settings to validate</param>
    /// <returns>A list of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> is null</exception>
    public static IReadOnlyList<string> Validate(this ConversionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.Id))
        {
            problems.Add("ConversionSettings.Id cannot be null or whitespace.");
        }

        if (settings.VideoBitrate < 500 || settings.VideoBitrate > 20000)
        {
            problems.Add("ConversionSettings.VideoBitrate must be between 500 and 20000 kbps.");
        }

        if (settings.AudioBitrate < 32 || settings.AudioBitrate > 320)
        {
            problems.Add("ConversionSettings.AudioBitrate must be between 32 and 320 kbps.");
        }

        if (string.IsNullOrWhiteSpace(settings.VideoCodec))
        {
            problems.Add("ConversionSettings.VideoCodec cannot be null or whitespace.");
        }
        else if (settings.VideoCodec.Length > 50)
        {
            problems.Add("ConversionSettings.VideoCodec exceeds maximum length of 50 characters.");
        }

        if (string.IsNullOrWhiteSpace(settings.AudioCodec))
        {
            problems.Add("ConversionSettings.AudioCodec cannot be null or whitespace.");
        }
        else if (settings.AudioCodec.Length > 50)
        {
            problems.Add("ConversionSettings.AudioCodec exceeds maximum length of 50 characters.");
        }

        if (settings.FrameRate < 15 || settings.FrameRate > 120)
        {
            problems.Add("ConversionSettings.FrameRate must be between 15 and 120 fps.");
        }

        if (settings.Width < 100 || settings.Width > 7680)
        {
            problems.Add("ConversionSettings.Width must be between 100 and 7680 pixels.");
        }

        if (settings.Height < 100 || settings.Height > 7680)
        {
            problems.Add("ConversionSettings.Height must be between 100 and 7680 pixels.");
        }

        if (settings.ThreadCount < 1 || settings.ThreadCount > 32)
        {
            problems.Add("ConversionSettings.ThreadCount must be between 1 and 32.");
        }

        if (settings.FadeInMs < 0 || settings.FadeInMs > 5000)
        {
            problems.Add("ConversionSettings.FadeInMs must be between 0 and 5000 milliseconds.");
        }

        if (settings.FadeOutMs < 0 || settings.FadeOutMs > 5000)
        {
            problems.Add("ConversionSettings.FadeOutMs must be between 0 and 5000 milliseconds.");
        }

        if (settings.CreatedAt == default)
        {
            problems.Add("ConversionSettings.CreatedAt cannot be the default DateTime value.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="JsonFormatter"/> instance is valid.
    /// </summary>
    /// <param name="value">The formatter instance to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this JsonFormatter value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a <see cref="CoubVideo"/> instance is valid for formatting.
    /// </summary>
    /// <param name="video">The video to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this CoubVideo video)
    {
        return video.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if an enumerable of <see cref="CoubVideo"/> instances is valid for formatting.
    /// </summary>
    /// <param name="videos">The videos to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this IEnumerable<CoubVideo> videos)
    {
        return videos.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a <see cref="BatchJob"/> instance is valid for formatting.
    /// </summary>
    /// <param name="batch">The batch job to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this BatchJob batch)
    {
        return batch.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a <see cref="ConversionSettings"/> instance is valid for formatting.
    /// </summary>
    /// <param name="settings">The conversion settings to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this ConversionSettings settings)
    {
        return settings.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="JsonFormatter"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The formatter instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems</exception>
    public static void EnsureValid(this JsonFormatter value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "JsonFormatter validation failed. Problems:\n" + string.Join("\n", problems),
                nameof(value));
        }
    }

    /// <summary>
    /// Ensures that a <see cref="CoubVideo"/> instance is valid for formatting,
    /// throwing an exception if not.
    /// </summary>
    /// <param name="video">The video to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="video"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems</exception>
    public static void EnsureValid(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);

        var problems = video.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "CoubVideo validation failed. Problems:\n" + string.Join("\n", problems),
                nameof(video));
        }
    }

    /// <summary>
    /// Ensures that an enumerable of <see cref="CoubVideo"/> instances is valid for formatting,
    /// throwing an exception if not.
    /// </summary>
    /// <param name="videos">The videos to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="videos"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems</exception>
    public static void EnsureValid(this IEnumerable<CoubVideo> videos)
    {
        ArgumentNullException.ThrowIfNull(videos);

        var problems = videos.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "CoubVideo collection validation failed. Problems:\n" + string.Join("\n", problems),
                nameof(videos));
        }
    }

    /// <summary>
    /// Ensures that a <see cref="BatchJob"/> instance is valid for formatting,
    /// throwing an exception if not.
    /// </summary>
    /// <param name="batch">The batch job to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="batch"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems</exception>
    public static void EnsureValid(this BatchJob batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        var problems = batch.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "BatchJob validation failed. Problems:\n" + string.Join("\n", problems),
                nameof(batch));
        }
    }

    /// <summary>
    /// Ensures that a <see cref="ConversionSettings"/> instance is valid for formatting,
    /// throwing an exception if not.
    /// </summary>
    /// <param name="settings">The conversion settings to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems</exception>
    public static void EnsureValid(this ConversionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var problems = settings.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "ConversionSettings validation failed. Problems:\n" + string.Join("\n", problems),
                nameof(settings));
        }
    }
}
