// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Presentation.Formatters;

/// <summary>Formats domain models as ASCII tables for console output</summary>
public class TableFormatter
{
    /// <summary>Format batch job status as ASCII table</summary>
    public string FormatBatchStatus(BatchJob batch)
    {
        var sb = new StringBuilder();
        var totalTasks = batch.Tasks.Count;
        var completedTasks = batch.Tasks.Count(t => t.State == ProcessingState.Completed);
        var failedTasks = batch.Tasks.Count(t => t.State == ProcessingState.Failed);
        var pendingTasks = batch.Tasks.Count(t => t.State == ProcessingState.Pending);

        sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
        sb.AppendLine($"║ Batch Job: {batch.Name,-54} ║");
        sb.AppendLine("╠════════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║ ID              │ {batch.Id,-52} ║");
        sb.AppendLine($"║ Output Directory│ {TruncateString(batch.OutputDirectory, 52)} ║");
        sb.AppendLine($"║ State           │ {batch.State,-52} ║");
        sb.AppendLine($"║ Created         │ {batch.CreatedAt:yyyy-MM-dd HH:mm:ss,-52} ║");
        sb.AppendLine("╠════════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║ Total Tasks     │ {totalTasks,-52} ║");
        sb.AppendLine($"║ Completed       │ {completedTasks,-52} ║");
        sb.AppendLine($"║ Failed          │ {failedTasks,-52} ║");
        sb.AppendLine($"║ Pending         │ {pendingTasks,-52} ║");
        sb.AppendLine($"║ Progress        │ {GetProgressBar(completedTasks, totalTasks),-52} ║");
        sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }

    /// <summary>Format task list as ASCII table</summary>
    public string FormatTasksTable(IEnumerable<DownloadTask> tasks)
    {
        var taskList = tasks.ToList();
        if (taskList.Count == 0) return "No tasks to display";

        var sb = new StringBuilder();
        sb.AppendLine("╔═══════════════════════════════════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║ Task ID                  │ State       │ Format   │ Quality  │ Created At         │ Retries  ║");
        sb.AppendLine("╠═══════════════════════════════════════════════════════════════════════════════════════════════╣");

        foreach (var task in taskList.Take(10))
        {
            var taskId = TruncateString(task.Id, 23);
            var state = TruncateString(task.State.ToString(), 11);
            var format = TruncateString(task.Format.ToString(), 8);
            var quality = TruncateString(task.Quality.ToString(), 8);
            var created = task.CreatedAt.ToString("yyyy-MM-dd HH:mm");

            sb.AppendLine($"║ {taskId,-23} │ {state,-11} │ {format,-8} │ {quality,-8} │ {created,-18} │ {task.MaxRetries,-8} ║");
        }

        if (taskList.Count > 10)
        {
            sb.AppendLine($"║ ... and {taskList.Count - 10} more tasks{new string(' ', 70)} ║");
        }

        sb.AppendLine("╚═══════════════════════════════════════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }

    private string GetProgressBar(int completed, int total)
    {
        if (total == 0) return "0%";

        var percentage = (completed * 100) / total;
        var filled = percentage / 10;
        var empty = 10 - filled;

        return $"[{new string('█', filled)}{new string('░', empty)}] {percentage}%";
    }

    private string TruncateString(string value, int maxLength)
    {
        if (value.Length <= maxLength) return value.PadRight(maxLength);
        return value[..(maxLength - 3)] + "...";
    }
}
