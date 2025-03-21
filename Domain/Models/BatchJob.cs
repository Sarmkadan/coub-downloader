// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents a batch job containing multiple download and conversion tasks.
/// </summary>
public class BatchJob
{
    /// <summary>Unique identifier for the batch job</summary>
    [Required]
    public string Id { get; set; } = null!;

    /// <summary>Descriptive name for the batch job</summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    /// <summary>Description of the batch job</summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>List of download tasks in this batch</summary>
    public List<DownloadTask> Tasks { get; set; } = new();

    /// <summary>Shared conversion settings for all tasks</summary>
    public ConversionSettings? SharedSettings { get; set; }

    /// <summary>Current processing state</summary>
    public ProcessingState State { get; set; } = ProcessingState.Pending;

    /// <summary>Total number of tasks</summary>
    [Range(0, int.MaxValue)]
    public int TotalTasks { get; set; }

    /// <summary>Number of completed tasks</summary>
    [Range(0, int.MaxValue)]
    public int CompletedTasks { get; set; } = 0;

    /// <summary>Number of failed tasks</summary>
    [Range(0, int.MaxValue)]
    public int FailedTasks { get; set; } = 0;

    /// <summary>Output directory for all converted videos</summary>
    [Required]
    public string OutputDirectory { get; set; } = null!;

    /// <summary>Whether to continue on error or stop immediately</summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>Start time of batch processing</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Completion time of batch processing</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Parallel task execution limit</summary>
    [Range(1, 10)]
    public int MaxParallelTasks { get; set; } = 2;

    /// <summary>Creation timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last update timestamp</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Get overall progress percentage</summary>
    public int GetProgressPercent()
    {
        if (TotalTasks == 0) return 0;
        return (int)((CompletedTasks + FailedTasks) * 100 / TotalTasks);
    }

    /// <summary>Get elapsed time since batch started</summary>
    public TimeSpan? GetElapsedTime()
    {
        if (StartedAt is null) return null;
        var endTime = CompletedAt ?? DateTime.UtcNow;
        return endTime - StartedAt;
    }

    /// <summary>Check if all tasks are completed</summary>
    public bool IsCompleted() => CompletedTasks + FailedTasks == TotalTasks && TotalTasks > 0;

    /// <summary>Get count of pending tasks</summary>
    public int GetPendingTaskCount() => TotalTasks - CompletedTasks - FailedTasks;

    /// <summary>Check if batch can be started</summary>
    public bool CanStart() => State == ProcessingState.Pending && TotalTasks > 0;

    /// <summary>Validate batch job</summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id)
            && !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(OutputDirectory)
            && TotalTasks > 0;
    }
}
