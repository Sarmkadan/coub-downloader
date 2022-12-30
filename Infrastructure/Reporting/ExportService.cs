// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Middleware;
using CoubDownloader.Presentation.Formatters;

namespace CoubDownloader.Infrastructure.Reporting;

/// <summary>Export service for generating reports in multiple formats</summary>
public class ExportService
{
    private readonly JsonFormatter _jsonFormatter;
    private readonly CsvFormatter _csvFormatter;
    private readonly ILoggingService _logger;

    public ExportService(ILoggingService logger)
    {
        _logger = logger;
        _jsonFormatter = new JsonFormatter();
        _csvFormatter = new CsvFormatter();
    }

    /// <summary>Export batch job report to file</summary>
    public async Task<bool> ExportBatchReportAsync(BatchJob batch, string outputPath, ExportFormat format)
    {
        try
        {
            var content = format switch
            {
                ExportFormat.Json => _jsonFormatter.FormatBatchJob(batch),
                ExportFormat.Csv => _csvFormatter.FormatBatchTasks(batch),
                ExportFormat.Xml => GenerateXmlReport(batch),
                _ => throw new ArgumentException("Unsupported format")
            };

            await File.WriteAllTextAsync(outputPath, content);
            _logger.LogInfo($"Batch report exported to {outputPath}", "ExportService");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to export report to {outputPath}", ex, "ExportService");
            return false;
        }
    }

    /// <summary>Export download results</summary>
    public async Task<bool> ExportDownloadResultsAsync(
        List<DownloadResult> results,
        string outputPath,
        ExportFormat format)
    {
        try
        {
            var content = format switch
            {
                ExportFormat.Json => GenerateJsonResults(results),
                ExportFormat.Csv => GenerateCsvResults(results),
                ExportFormat.Xml => GenerateXmlResults(results),
                _ => throw new ArgumentException("Unsupported format")
            };

            await File.WriteAllTextAsync(outputPath, content);
            _logger.LogInfo($"Results exported to {outputPath}", "ExportService");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to export results", ex, "ExportService");
            return false;
        }
    }

    /// <summary>Generate HTML report for batch job</summary>
    public string GenerateHtmlReport(BatchJob batch)
    {
        var completedTasks = batch.Tasks.Count(t => t.State == ProcessingState.Completed);
        var failedTasks = batch.Tasks.Count(t => t.State == ProcessingState.Failed);
        var progress = batch.Tasks.Count > 0 ? (completedTasks * 100) / batch.Tasks.Count : 0;

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Batch Report - {batch.Name}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background: #f0f0f0; padding: 10px; border-radius: 5px; }}
        .stats {{ display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; margin: 20px 0; }}
        .stat-box {{ background: #e8f4f8; padding: 15px; border-radius: 5px; }}
        .progress-bar {{ width: 100%; height: 20px; background: #ddd; border-radius: 10px; overflow: hidden; }}
        .progress-fill {{ height: 100%; background: #4CAF50; width: {progress}%; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
        th {{ background: #f0f0f0; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Batch Job Report</h1>
        <p><strong>{batch.Name}</strong> (ID: {batch.Id})</p>
        <p>Created: {batch.CreatedAt:yyyy-MM-dd HH:mm:ss}</p>
    </div>

    <div class='stats'>
        <div class='stat-box'>
            <h3>Total Tasks</h3>
            <p style='font-size: 24px; margin: 0;'>{batch.Tasks.Count}</p>
        </div>
        <div class='stat-box'>
            <h3>Completed</h3>
            <p style='font-size: 24px; margin: 0; color: green;'>{completedTasks}</p>
        </div>
        <div class='stat-box'>
            <h3>Failed</h3>
            <p style='font-size: 24px; margin: 0; color: red;'>{failedTasks}</p>
        </div>
        <div class='stat-box'>
            <h3>Progress</h3>
            <div class='progress-bar'>
                <div class='progress-fill'></div>
            </div>
            <p style='margin: 5px 0;'>{progress}%</p>
        </div>
    </div>

    <h2>Tasks</h2>
    <table>
        <thead>
            <tr>
                <th>Task ID</th>
                <th>State</th>
                <th>Format</th>
                <th>Quality</th>
                <th>Created</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", batch.Tasks.Select(t => $@"
            <tr>
                <td>{t.Id}</td>
                <td>{t.State}</td>
                <td>{t.Format}</td>
                <td>{t.Quality}</td>
                <td>{t.CreatedAt:yyyy-MM-dd HH:mm}</td>
            </tr>"))}
        </tbody>
    </table>
</body>
</html>";

        return html;
    }

    private string GenerateXmlReport(BatchJob batch)
    {
        return $@"<?xml version='1.0' encoding='utf-8'?>
<BatchJob>
    <Id>{batch.Id}</Id>
    <Name>{batch.Name}</Name>
    <State>{batch.State}</State>
    <CreatedAt>{batch.CreatedAt:O}</CreatedAt>
    <TaskCount>{batch.Tasks.Count}</TaskCount>
    <Tasks>
        {string.Join("", batch.Tasks.Select(t => $@"
        <Task>
            <Id>{t.Id}</Id>
            <State>{t.State}</State>
            <Format>{t.Format}</Format>
            <Quality>{t.Quality}</Quality>
        </Task>"))}
    </Tasks>
</BatchJob>";
    }

    private string GenerateCsvResults(List<DownloadResult> results)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("TaskID,Success,OutputPath,FileSize,ProcessingTime,Format,Quality,ErrorMessage");

        foreach (var result in results)
        {
            var path = result.OutputFilePath ?? "";
            var error = result.ErrorMessage ?? "";

            sb.AppendLine(
                $"{result.TaskId},{result.Success},{path},{result.OutputFileSizeBytes}," +
                $"{result.ProcessingTimeMs},{result.Format},{result.Quality},{error}");
        }

        return sb.ToString();
    }

    private string GenerateJsonResults(List<DownloadResult> results)
    {
        var dtos = results.Select(r => new
        {
            r.TaskId,
            r.Success,
            r.OutputFilePath,
            r.OutputFileSizeBytes,
            r.ProcessingTimeMs,
            r.Format,
            r.Quality,
            r.ErrorMessage
        }).ToList();

        return System.Text.Json.JsonSerializer.Serialize(dtos,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    private string GenerateXmlResults(List<DownloadResult> results)
    {
        return $@"<?xml version='1.0' encoding='utf-8'?>
<Results>
    {string.Join("", results.Select(r => $@"
    <Result>
        <TaskId>{r.TaskId}</TaskId>
        <Success>{r.Success}</Success>
        <OutputPath>{r.OutputFilePath}</OutputPath>
        <FileSize>{r.OutputFileSizeBytes}</FileSize>
        <ProcessingTime>{r.ProcessingTimeMs}</ProcessingTime>
        <ErrorMessage>{r.ErrorMessage}</ErrorMessage>
    </Result>"))}
</Results>";
    }
}

/// <summary>Export format types</summary>
public enum ExportFormat
{
    Json,
    Csv,
    Xml,
    Html
}

/// <summary>Report builder for fluent API</summary>
public class ReportBuilder
{
    private readonly List<string> _sections = [];

    public ReportBuilder AddSection(string title, string content)
    {
        _sections.Add($"## {title}\n{content}");
        return this;
    }

    public ReportBuilder AddTable(string title, Dictionary<string, string> data)
    {
        var table = $"## {title}\n\n";
        table += "| Key | Value |\n";
        table += "| --- | --- |\n";

        foreach (var kvp in data)
            table += $"| {kvp.Key} | {kvp.Value} |\n";

        _sections.Add(table);
        return this;
    }

    public string Build()
    {
        return string.Join("\n\n", _sections);
    }
}
