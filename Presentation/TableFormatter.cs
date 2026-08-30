#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Presentation;

using System.Text;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

// Re-export for convenience
public class TableFormatter : CoubDownloader.Presentation.Formatters.TableFormatter
{
    /// <summary>
    /// Formats a batch job status as an ASCII table. Null task elements are skipped.
    /// </summary>
    /// <param name="batch">The batch job to format.</param>
    /// <returns>The formatted batch status table.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="batch"/> or its task collection is <see langword="null"/>.
    /// </exception>
    public new string FormatBatchStatus(BatchJob batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(batch.Tasks);

        var tasks = batch.Tasks.Where(task => task is not null).ToList();
        var sb = new StringBuilder();
        var totalTasks = tasks.Count;
        var completedTasks = tasks.Count(task => task.State == ProcessingState.Completed);
        var failedTasks = tasks.Count(task => task.State == ProcessingState.Failed);
        var pendingTasks = tasks.Count(task => task.State == ProcessingState.Pending);

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

    /// <summary>
    /// Formats a task collection as an ASCII table. Null task elements are skipped.
    /// </summary>
    /// <param name="tasks">The tasks to format.</param>
    /// <returns>The formatted tasks table.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="tasks"/> is <see langword="null"/>.
    /// </exception>
    public new string FormatTasksTable(IEnumerable<DownloadTask> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        var taskList = tasks.Where(task => task is not null).ToList();
        if (taskList.Count == 0) return "No tasks to display";

        var sb = new StringBuilder();
        sb.AppendLine("╔═══════════════════════════════════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║ Task ID                  │ State       │ Format   │ Quality  │ Created At         │ Retries  ║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════════════════════════════════════╣");

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

        sb.AppendLine("╚═════════════════════════════════════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }

    /// <summary>Builds a progress bar from non-negative task counts.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="completed"/> or <paramref name="total"/> is negative.
    /// </exception>
    private static string GetProgressBar(int completed, int total)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(completed);
        ArgumentOutOfRangeException.ThrowIfNegative(total);

        if (total == 0) return "0%";

        var percentage = (completed * 100) / total;
        var filled = percentage / 10;
        var empty = 10 - filled;

        return $"[{new string('█', filled)}{new string('░', empty)}] {percentage}%";
    }

    /// <summary>Truncates or pads a required string to the requested width.</summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxLength"/> is less than three.
    /// </exception>
    private static string TruncateString(string value, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 3);

        if (value.Length <= maxLength) return value.PadRight(maxLength);
        return value[..(maxLength - 3)] + "...";
    }
}
