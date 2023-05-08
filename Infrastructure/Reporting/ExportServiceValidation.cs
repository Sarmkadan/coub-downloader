#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Infrastructure.Reporting;

/// <summary>
/// Provides validation helpers for ExportService operations and related models.
/// </summary>
public static class ExportServiceValidation
{
    /// <summary>
    /// Validates an ExportService instance and its dependencies.
    /// </summary>
    /// <param name="value">The ExportService instance to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this ExportService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // ExportService has no public properties to validate
        // The logger dependency is validated at construction time

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an ExportService instance is valid.
    /// </summary>
    /// <param name="value">The ExportService instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this ExportService? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures an ExportService instance is valid, throwing if not.
    /// </summary>
    /// <param name="value">The ExportService instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid with detailed problems</exception>
    public static void EnsureValid(this ExportService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ExportService is invalid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }

    /// <summary>
    /// Validates parameters for ExportBatchReportAsync operation.
    /// </summary>
    /// <param name="batch">The batch job to export</param>
    /// <param name="outputPath">Output file path</param>
    /// <param name="format">Export format</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> ValidateExportBatchReportAsync(
        this BatchJob? batch,
        string? outputPath,
        ExportFormat format)
    {
        var problems = new List<string>();

        if (batch is null)
        {
            problems.Add("BatchJob cannot be null");
        }
        else
        {
            problems.AddRange(batch.Validate());
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            problems.Add("Output path cannot be null or whitespace");
        }
        else if (!Path.IsPathRooted(outputPath) && !outputPath.Contains(Path.DirectorySeparatorChar))
        {
            problems.Add("Output path should be an absolute path or contain directory separators");
        }

        // ExportFormat is an enum, always valid

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for ExportDownloadResultsAsync operation.
    /// </summary>
    /// <param name="results">List of download results to export</param>
    /// <param name="outputPath">Output file path</param>
    /// <param name="format">Export format</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> ValidateExportDownloadResultsAsync(
        this List<DownloadResult>? results,
        string? outputPath,
        ExportFormat format)
    {
        var problems = new List<string>();

        if (results is null || results.Count == 0)
        {
            problems.Add("Results list cannot be null or empty");
        }
        else
        {
            var resultProblems = new List<string>();
            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                if (result is null)
                {
                    resultProblems.Add($"Result at index {i} is null");
                }
                else
                {
                    resultProblems.AddRange(result.Validate());
                }
            }
            problems.AddRange(resultProblems);
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            problems.Add("Output path cannot be null or whitespace");
        }
        else if (!Path.IsPathRooted(outputPath) && !outputPath.Contains(Path.DirectorySeparatorChar))
        {
            problems.Add("Output path should be an absolute path or contain directory separators");
        }

        // ExportFormat is an enum, always valid

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for GenerateHtmlReport operation.
    /// </summary>
    /// <param name="batch">The batch job to generate report for</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> ValidateGenerateHtmlReport(this BatchJob? batch)
    {
        var problems = new List<string>();

        if (batch is null)
        {
            problems.Add("BatchJob cannot be null");
        }
        else
        {
            problems.AddRange(batch.Validate());
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for ReportBuilder.AddSection operation.
    /// </summary>
    /// <param name="title">Section title</param>
    /// <param name="content">Section content</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> ValidateAddSection(string? title, string? content)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(title))
        {
            problems.Add("Title cannot be null or whitespace");
        }

        // Content can be null or empty (optional)

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for ReportBuilder.AddTable operation.
    /// </summary>
    /// <param name="title">Table title</param>
    /// <param name="data">Table data dictionary</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> ValidateAddTable(string? title, Dictionary<string, string>? data)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(title))
        {
            problems.Add("Title cannot be null or whitespace");
        }

        if (data is null || data.Count == 0)
        {
            problems.Add("Data dictionary cannot be null or empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a BatchJob instance.
    /// </summary>
    /// <param name="value">The BatchJob to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> Validate(this BatchJob? value)
    {
        var problems = new List<string>();

        if (value is null)
        {
            problems.Add("BatchJob cannot be null");
            return problems.AsReadOnly();
        }

        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("BatchJob.Id cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("BatchJob.Name cannot be null or whitespace");
        }
        else if (value.Name.Length > 255)
        {
            problems.Add("BatchJob.Name exceeds maximum length of 255 characters");
        }

        if (string.IsNullOrWhiteSpace(value.OutputDirectory))
        {
            problems.Add("BatchJob.OutputDirectory cannot be null or whitespace");
        }

        if (value.TotalTasks < 0)
        {
            problems.Add("BatchJob.TotalTasks cannot be negative");
        }

        if (value.MaxParallelTasks < 1 || value.MaxParallelTasks > 10)
        {
            problems.Add("BatchJob.MaxParallelTasks must be between 1 and 10");
        }

        if (value.CreatedAt == default)
        {
            problems.Add("BatchJob.CreatedAt cannot be default(DateTime)");
        }

        if (value.UpdatedAt == default)
        {
            problems.Add("BatchJob.UpdatedAt cannot be default(DateTime)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a DownloadResult instance.
    /// </summary>
    /// <param name="value">The DownloadResult to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> Validate(this DownloadResult? value)
    {
        var problems = new List<string>();

        if (value is null)
        {
            problems.Add("DownloadResult cannot be null");
            return problems.AsReadOnly();
        }

        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("DownloadResult.Id cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.TaskId))
        {
            problems.Add("DownloadResult.TaskId cannot be null or whitespace");
        }

        if (value.OutputFileSizeBytes < 0)
        {
            problems.Add("DownloadResult.OutputFileSizeBytes cannot be negative");
        }

        if (value.ProcessingTimeMs < 0)
        {
            problems.Add("DownloadResult.ProcessingTimeMs cannot be negative");
        }

        if (value.CompletedAt == default)
        {
            problems.Add("DownloadResult.CompletedAt cannot be default(DateTime)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a ConversionSettings instance.
    /// </summary>
    /// <param name="value">The ConversionSettings to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> Validate(this ConversionSettings? value)
    {
        var problems = new List<string>();

        if (value is null)
        {
            problems.Add("ConversionSettings cannot be null");
            return problems.AsReadOnly();
        }

        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("ConversionSettings.Id cannot be null or whitespace");
        }

        if (value.VideoBitrate < 500 || value.VideoBitrate > 20000)
        {
            problems.Add("ConversionSettings.VideoBitrate must be between 500 and 20000 kbps");
        }

        if (value.AudioBitrate < 32 || value.AudioBitrate > 320)
        {
            problems.Add("ConversionSettings.AudioBitrate must be between 32 and 320 kbps");
        }

        if (value.FrameRate < 15 || value.FrameRate > 120)
        {
            problems.Add("ConversionSettings.FrameRate must be between 15 and 120 fps");
        }

        if (value.Width < 100 || value.Width > 7680)
        {
            problems.Add("ConversionSettings.Width must be between 100 and 7680 pixels");
        }

        if (value.Height < 100 || value.Height > 7680)
        {
            problems.Add("ConversionSettings.Height must be between 100 and 7680 pixels");
        }

        if (value.ThreadCount < 1 || value.ThreadCount > 32)
        {
            problems.Add("ConversionSettings.ThreadCount must be between 1 and 32");
        }

        if (value.FadeInMs < 0 || value.FadeInMs > 5000)
        {
            problems.Add("ConversionSettings.FadeInMs must be between 0 and 5000 ms");
        }

        if (value.FadeOutMs < 0 || value.FadeOutMs > 5000)
        {
            problems.Add("ConversionSettings.FadeOutMs must be between 0 and 5000 ms");
        }

        if (value.CreatedAt == default)
        {
            problems.Add("ConversionSettings.CreatedAt cannot be default(DateTime)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a BatchJob instance is valid.
    /// </summary>
    /// <param name="value">The BatchJob instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this BatchJob? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a BatchJob instance is valid, throwing if not.
    /// </summary>
    /// <param name="value">The BatchJob instance to validate</param>
    /// <exception cref="ArgumentException">Thrown if value is invalid with detailed problems</exception>
    public static void EnsureValid(this BatchJob? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BatchJob is invalid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }

    /// <summary>
    /// Checks if a DownloadResult instance is valid.
    /// </summary>
    /// <param name="value">The DownloadResult to validate</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this DownloadResult? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a DownloadResult instance is valid, throwing if not.
    /// </summary>
    /// <param name="value">The DownloadResult to validate</param>
    /// <exception cref="ArgumentException">Thrown if value is invalid with detailed problems</exception>
    public static void EnsureValid(this DownloadResult? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DownloadResult is invalid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }

    /// <summary>
    /// Checks if a ConversionSettings instance is valid.
    /// </summary>
    /// <param name="value">The ConversionSettings to validate</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this ConversionSettings? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a ConversionSettings instance is valid, throwing if not.
    /// </summary>
    /// <param name="value">The ConversionSettings to validate</param>
    /// <exception cref="ArgumentException">Thrown if value is invalid with detailed problems</exception>
    public static void EnsureValid(this ConversionSettings? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ConversionSettings is invalid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }
}
