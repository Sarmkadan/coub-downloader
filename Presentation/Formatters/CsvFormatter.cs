// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Presentation.Formatters;

/// <summary>Formats domain models to CSV output</summary>
public class CsvFormatter
{
    /// <summary>Format multiple Coub videos to CSV</summary>
    public string FormatVideos(IEnumerable<CoubVideo> videos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,Title,URL,Duration,Width,Height,Creator,Views,HasAudio,UploadedDate");

        foreach (var video in videos)
        {
            var title = EscapeCSV(video.Title);
            var creator = EscapeCSV(video.CreatorName);
            var description = EscapeCSV(video.Description ?? "");

            sb.AppendLine(
                $"{video.Id},{title},{video.Url},{video.Duration}," +
                $"{video.Width},{video.Height},{creator},{video.ViewCount}," +
                $"{video.HasAudio},{video.UploadedDate:yyyy-MM-dd}");
        }

        return sb.ToString();
    }

    /// <summary>Format batch job tasks to CSV</summary>
    public string FormatBatchTasks(BatchJob batch)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TaskID,VideoID,URL,OutputPath,State,Format,Quality,CreatedAt");

        foreach (var task in batch.Tasks)
        {
            var url = EscapeCSV(task.Url);
            var path = EscapeCSV(task.OutputPath);

            sb.AppendLine(
                $"{task.Id},{task.VideoId},{url},{path}," +
                $"{task.State},{task.Format},{task.Quality},{task.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return sb.ToString();
    }

    /// <summary>Format download results to CSV</summary>
    public string FormatDownloadResults(IEnumerable<DownloadResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TaskID,Success,OutputPath,FileSize,ProcessingTime,Format,Quality,ErrorMessage");

        foreach (var result in results)
        {
            var path = EscapeCSV(result.OutputFilePath ?? "");
            var error = EscapeCSV(result.ErrorMessage ?? "");

            sb.AppendLine(
                $"{result.TaskId},{result.Success},{path},{result.OutputFileSizeBytes}," +
                $"{result.ProcessingTimeMs},{result.Format},{result.Quality},{error}");
        }

        return sb.ToString();
    }

    private string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
