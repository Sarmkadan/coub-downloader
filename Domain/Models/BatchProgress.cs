#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents progress information for batch processing
/// </summary>
public class BatchProgress
{
    /// <summary>Total number of tasks</summary>
    public int Total { get; set; }

    /// <summary>Number of completed tasks</summary>
    public int Completed { get; set; }

    /// <summary>Number of failed tasks</summary>
    public int Failed { get; set; }

    /// <summary>Current task being processed (0-based index)</summary>
    public int CurrentItem { get; set; }

    /// <summary>Current task URL being processed</summary>
    public string? CurrentTaskUrl { get; set; }

    /// <summary>Current task index (1-based)</summary>
    public int CurrentItemNumber => CurrentItem + 1;

    /// <summary>Get overall progress percentage</summary>
    public int ProgressPercent
    {
        get
        {
            if (Total == 0) return 0;
            return (int)((Completed + Failed) * 100 / Total);
        }
    }

    /// <summary>Get status message</summary>
    public string StatusMessage
    {
        get
        {
            if (Total == 0) return "No tasks";

            var completedPercent = ProgressPercent;
            var failedPercent = Failed * 100 / Total;

            if (completedPercent >= 100)
                return "Processing complete";

            if (CurrentTaskUrl != null)
                return $"Processing: {CurrentTaskUrl} ({CurrentItemNumber}/{Total}) - {completedPercent}% complete";

            return $"{completedPercent}% complete ({Completed}/{Total} tasks)";
        }
    }
}