// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents the result of a download and conversion operation.
/// </summary>
public class DownloadResult
{
    /// <summary>Unique identifier for the result record</summary>
    [Required]
    public string Id { get; set; } = null!;

    /// <summary>Reference to the original download task</summary>
    [Required]
    public string TaskId { get; set; } = null!;

    /// <summary>Whether the operation was successful</summary>
    public bool Success { get; set; }

    /// <summary>Path to the resulting output file</summary>
    public string? OutputFilePath { get; set; }

    /// <summary>Size of the output file in bytes</summary>
    [Range(0, long.MaxValue)]
    public long OutputFileSizeBytes { get; set; } = 0;

    /// <summary>Duration of processing in milliseconds</summary>
    [Range(0, long.MaxValue)]
    public long ProcessingTimeMs { get; set; } = 0;

    /// <summary>Actual output format used</summary>
    public VideoFormat Format { get; set; }

    /// <summary>Quality settings that were applied</summary>
    public VideoQuality Quality { get; set; }

    /// <summary>Error message if operation failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Error stack trace if operation failed</summary>
    public string? ErrorStackTrace { get; set; }

    /// <summary>Error type/category</summary>
    [StringLength(100)]
    public string? ErrorType { get; set; }

    /// <summary>Video metadata from processing</summary>
    public string? VideoMetadata { get; set; }

    /// <summary>Audio synchronization details</summary>
    public string? AudioSyncInfo { get; set; }

    /// <summary>Processing warnings (non-fatal issues)</summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>Completion timestamp</summary>
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Get human-readable status message</summary>
    public string GetStatusMessage()
    {
        if (Success)
        {
            var fileSize = FormatFileSize(OutputFileSizeBytes);
            return $"✓ Completed in {ProcessingTimeMs}ms - {fileSize}";
        }
        else
        {
            return $"✗ Failed: {ErrorMessage}";
        }
    }

    /// <summary>Check if result has warnings</summary>
    public bool HasWarnings => Warnings.Count > 0;

    /// <summary>Format file size for display</summary>
    private static string FormatFileSize(long bytes)
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

    /// <summary>Add a warning message</summary>
    public void AddWarning(string warning)
    {
        if (!Warnings.Contains(warning))
        {
            Warnings.Add(warning);
        }
    }
}
