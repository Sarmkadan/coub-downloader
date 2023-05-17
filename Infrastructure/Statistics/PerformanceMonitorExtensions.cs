#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace CoubDownloader.Infrastructure.Statistics;

/// <summary>Extension methods for <see cref="PerformanceMonitor"/> providing additional functionality for performance analysis and reporting</summary>
public static class PerformanceMonitorExtensions
{
    /// <summary>Get formatted summary report for a specific operation</summary>
    /// <param name="monitor">The performance monitor instance. Cannot be <see langword="null"/></param>
    /// <param name="operationName">Name of the operation to get report for. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>Formatted report string or <see langword="null"/> if operation not found</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="operationName"/> is <see langword="null"/> or empty</exception>
    public static string? GetOperationReport(this PerformanceMonitor monitor, string operationName)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        var metrics = monitor.GetMetrics(operationName);
        return metrics == null
            ? null
            : FormatOperationReport(metrics);

        static string FormatOperationReport(OperationMetrics metrics)
        {
            var report = new StringBuilder();
            report.AppendLine("╔════════════════════════════════════════════════════════════╗");
            report.AppendLine("║ Operation Performance Report ║");
            report.AppendLine("╠════════════════════════════════════════════════════════════╣");

            report.AppendLine($"║ Operation: {metrics.Name,-44} ║");
            report.AppendLine($"║ Total Count: {metrics.TotalCount,-42} ║");
            report.AppendLine($"║ Success Count: {metrics.SuccessCount,-41} ║");
            report.AppendLine($"║ Failure Count: {metrics.FailureCount,-41} ║");
            report.AppendLine($"║ Success Rate: {metrics.SuccessRate * 100:F1}%{new string(' ', 44 - (metrics.SuccessRate * 100).ToString("F1").Length)} ║");
            report.AppendLine($"║ Average Time: {metrics.AverageTimeMs:F2}ms{new string(' ', 42 - metrics.AverageTimeMs.ToString("F2").Length)} ║");
            report.AppendLine($"║ Min Time: {metrics.MinTimeMs}ms{new string(' ', 46 - metrics.MinTimeMs.ToString().Length)} ║");
            report.AppendLine($"║ Max Time: {metrics.MaxTimeMs}ms{new string(' ', 46 - metrics.MaxTimeMs.ToString().Length)} ║");
            report.AppendLine($"║ Total Time: {metrics.TotalTimeMs}ms{new string(' ', 44 - metrics.TotalTimeMs.ToString().Length)} ║");

            report.AppendLine("╚════════════════════════════════════════════════════════════╝");

            return report.ToString();
        }
    }

    /// <summary>Get performance metrics as CSV format</summary>
    /// <param name="monitor">The performance monitor instance. Cannot be <see langword="null"/></param>
    /// <returns>CSV formatted string with all metrics</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/></exception>
    public static string GetCsvReport(this PerformanceMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        var metrics = monitor.GetAllMetrics();
        if (metrics.Count == 0)
            return "OperationName,TotalCount,SuccessCount,FailureCount,SuccessRate,AvgTimeMs,MinTimeMs,MaxTimeMs,TotalTimeMs\nNo data available";

        var csv = new StringBuilder();
        csv.AppendLine("OperationName,TotalCount,SuccessCount,FailureCount,SuccessRate,AvgTimeMs,MinTimeMs,MaxTimeMs,TotalTimeMs");

        foreach (var metric in metrics)
        {
            csv.AppendLine($"\"{EscapeCsv(metric.Name)}\",{metric.TotalCount},{metric.SuccessCount},{metric.FailureCount},{metric.SuccessRate * 100:F2},{metric.AverageTimeMs:F2},{metric.MinTimeMs},{metric.MaxTimeMs},{metric.TotalTimeMs}");
        }

        return csv.ToString();

        static string EscapeCsv(string value)
        {
            return value.Contains('"')
                ? value.Replace("\"", "\"\"")
                : value;
        }
    }

    /// <summary>Get top N slowest operations by average execution time</summary>
    /// <param name="monitor">The performance monitor instance. Cannot be <see langword="null"/></param>
    /// <param name="count">Number of operations to return (default: 5)</param>
    /// <returns>List of operation metrics sorted by average time, descending</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 0</exception>
    public static List<OperationMetrics> GetSlowestOperations(this PerformanceMonitor monitor, int count = 5)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        return monitor.GetAllMetrics()
            .OrderByDescending(m => m.AverageTimeMs)
            .Take(count)
            .ToList();
    }

    /// <summary>Get operations that exceed a specific failure rate threshold</summary>
    /// <param name="monitor">The performance monitor instance. Cannot be <see langword="null"/></param>
    /// <param name="failureRateThreshold">Failure rate threshold (0.0 to 1.0)</param>
    /// <returns>List of operation metrics with failure rate above threshold</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="failureRateThreshold"/> is less than 0.0 or greater than 1.0</exception>
    public static List<OperationMetrics> GetOperationsWithHighFailureRate(this PerformanceMonitor monitor, double failureRateThreshold = 0.3)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentOutOfRangeException.ThrowIfNegative(failureRateThreshold);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(failureRateThreshold, 1.0);

        return monitor.GetAllMetrics()
            .Where(m => m.TotalCount > 0 && (double)m.FailureCount / m.TotalCount > failureRateThreshold)
            .OrderByDescending(m => (double)m.FailureCount / m.TotalCount)
            .ToList();
    }
}