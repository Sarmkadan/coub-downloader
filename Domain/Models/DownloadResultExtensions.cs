#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Extension methods for <see cref="DownloadResult"/> providing additional functionality
/// for working with download results.
/// </summary>
public static class DownloadResultExtensions
{
    /// <summary>
    /// Determines whether the download was successful and the output file exists.
    /// </summary>
    /// <param name="result">The download result to check</param>
    /// <returns>True if successful and file exists; otherwise false</returns>
    public static bool IsSuccessfulWithFile(this DownloadResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return result.Success && !string.IsNullOrEmpty(result.OutputFilePath);
    }

    /// <summary>
    /// Gets the formatted file size with quality indicator.
    /// </summary>
    /// <param name="result">The download result</param>
    /// <returns>Formatted string with file size and quality</returns>
    public static string GetFormattedFileInfo(this DownloadResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        var fileSize = result.FormatFileSize(result.OutputFileSizeBytes);
        return $"{fileSize} - {result.Quality}";
    }

    /// <summary>
    /// Creates a deep copy of the DownloadResult to allow safe modifications.
    /// </summary>
    /// <param name="result">The download result to copy</param>
    /// <returns>A new DownloadResult instance with copied values</returns>
    public static DownloadResult Clone(this DownloadResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return new DownloadResult
        {
            Id = result.Id,
            TaskId = result.TaskId,
            Success = result.Success,
            OutputFilePath = result.OutputFilePath,
            OutputFileSizeBytes = result.OutputFileSizeBytes,
            ProcessingTimeMs = result.ProcessingTimeMs,
            Format = result.Format,
            Quality = result.Quality,
            ErrorMessage = result.ErrorMessage,
            ErrorStackTrace = result.ErrorStackTrace,
            ErrorType = result.ErrorType,
            VideoMetadata = result.VideoMetadata,
            AudioSyncInfo = result.AudioSyncInfo,
            Warnings = new List<string>(result.Warnings),
            CompletedAt = result.CompletedAt
        };
    }

    /// <summary>
    /// Determines if the processing time exceeded a specified threshold.
    /// </summary>
    /// <param name="result">The download result</param>
    /// <param name="thresholdMs">Threshold in milliseconds</param>
    /// <returns>True if processing time exceeded threshold</returns>
    public static bool ExceededProcessingTime(this DownloadResult result, long thresholdMs)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return result.ProcessingTimeMs > thresholdMs;
    }

    /// <summary>
    /// Formats the processing time into a human-readable string.
    /// </summary>
    /// <param name="result">The download result</param>
    /// <returns>Formatted time string (e.g., "2.5s", "150ms")</returns>
    public static string FormatProcessingTime(this DownloadResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (result.ProcessingTimeMs < 1000)
        {
            return $"{result.ProcessingTimeMs}ms";
        }
        else
        {
            double seconds = result.ProcessingTimeMs / 1000.0;
            return $"{seconds:0.##}s";
        }
    }

    /// <summary>
    /// Checks if the result has any critical errors (non-null ErrorType).
    /// </summary>
    /// <param name="result">The download result</param>
    /// <returns>True if has critical error</returns>
    public static bool HasCriticalError(this DownloadResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return !string.IsNullOrEmpty(result.ErrorType);
    }

    /// <summary>
    /// Gets a combined warnings string for logging purposes.
    /// </summary>
    /// <param name="result">The download result</param>
    /// <returns>Combined warnings or empty string if none</returns>
    public static string GetWarningsSummary(this DownloadResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (result.Warnings.Count == 0)
        {
            return string.Empty;
        }

        return string.Join("; ", result.Warnings);
    }

    /// <summary>
    /// Determines if the output file size is within expected bounds.
    /// </summary>
    /// <param name="result">The download result</param>
    /// <param name="minSizeBytes">Minimum expected size in bytes</param>
    /// <param name="maxSizeBytes">Maximum expected size in bytes</param>
    /// <returns>True if file size is within bounds</returns>
    public static bool IsFileSizeWithinBounds(this DownloadResult result, long minSizeBytes, long maxSizeBytes)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return result.OutputFileSizeBytes >= minSizeBytes && result.OutputFileSizeBytes <= maxSizeBytes;
    }

    /// <summary>
    /// Gets a status indicator emoji based on the result state.
    /// </summary>
    /// <param name="result">The download result</param>
    /// <returns>Status emoji: ✓ for success, ✗ for failure, ⚠ for warnings</returns>
    public static string GetStatusEmoji(this DownloadResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (result.Success)
        {
            return result.HasWarnings ? "⚠" : "✓";
        }

        return "✗";
    }

    /// <summary>
    /// Helper method to format file size (reused from DownloadResult)
    /// </summary>
    private static string FormatFileSize(this DownloadResult _, long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}