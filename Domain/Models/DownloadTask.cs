// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents a single download and conversion task for a Coub video.
/// </summary>
public class DownloadTask
{
    /// <summary>Unique identifier for the download task</summary>
    [Required]
    public string Id { get; set; } = null!;

    /// <summary>Reference to the Coub video being downloaded</summary>
    [Required]
    public string VideoId { get; set; } = null!;

    /// <summary>Coub video URL to download</summary>
    [Required]
    [Url]
    public string Url { get; set; } = null!;

    /// <summary>Output file path for the converted video</summary>
    [Required]
    public string OutputPath { get; set; } = null!;

    /// <summary>Current processing state</summary>
    public ProcessingState State { get; set; } = ProcessingState.Pending;

    /// <summary>Target output format</summary>
    public VideoFormat Format { get; set; } = VideoFormat.MP4;

    /// <summary>Target video quality</summary>
    public VideoQuality Quality { get; set; } = VideoQuality.High;

    /// <summary>Audio looping strategy</summary>
    public AudioLoopStrategy AudioLoop { get; set; } = AudioLoopStrategy.Repeat;

    /// <summary>Progress percentage (0-100)</summary>
    [Range(0, 100)]
    public int ProgressPercent { get; set; } = 0;

    /// <summary>Downloaded file size in bytes</summary>
    [Range(0, long.MaxValue)]
    public long FileSizeBytes { get; set; } = 0;

    /// <summary>Start time of processing</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Completion time of processing</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Error message if task failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Number of retry attempts made</summary>
    [Range(0, 10)]
    public int RetryCount { get; set; } = 0;

    /// <summary>Maximum number of retries allowed</summary>
    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    /// <summary>Creation timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last update timestamp</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optional batch job ID this task belongs to</summary>
    public string? BatchJobId { get; set; }

    /// <summary>Get elapsed time since task started</summary>
    public TimeSpan? GetElapsedTime()
    {
        if (StartedAt is null) return null;
        var endTime = CompletedAt ?? DateTime.UtcNow;
        return endTime - StartedAt;
    }

    /// <summary>Check if task is still running</summary>
    public bool IsRunning() => State is ProcessingState.Downloading or ProcessingState.Converting or ProcessingState.ProcessingAudio;

    /// <summary>Check if task can be retried</summary>
    public bool CanRetry() => RetryCount < MaxRetries && State == ProcessingState.Failed;

    /// <summary>Validate task properties</summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id)
            && !string.IsNullOrWhiteSpace(VideoId)
            && !string.IsNullOrWhiteSpace(Url)
            && !string.IsNullOrWhiteSpace(OutputPath);
    }
}
