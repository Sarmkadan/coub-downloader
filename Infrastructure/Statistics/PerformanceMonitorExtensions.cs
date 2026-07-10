#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace CoubDownloader.Infrastructure.Statistics;

/// <summary>Extension methods for PerformanceMonitor providing additional functionality</summary>
public static class PerformanceMonitorExtensions
{
    /// <summary>Get formatted summary report for a specific operation</summary>
    /// <param name="monitor">The performance monitor instance</param>
    /// <param name="operationName">Name of the operation to get report for</param>
    /// <returns>Formatted report string or null if operation not found</returns>
    public static string? GetOperationReport(this PerformanceMonitor monitor, string operationName)
    {
        var metrics = monitor.GetMetrics(operationName);
        if (metrics == null)
            return null;

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

    /// <summary>Get performance metrics as CSV format</summary>
    /// <param name="monitor">The performance monitor instance</param>
    /// <returns>CSV formatted string with all metrics</returns>
    public static string GetCsvReport(this PerformanceMonitor monitor)
    {
        var metrics = monitor.GetAllMetrics();
        if (metrics.Count == 0)
            return "OperationName,TotalCount,SuccessCount,FailureCount,SuccessRate,AvgTimeMs,MinTimeMs,MaxTimeMs,TotalTimeMs\nNo data available";

        var csv = new StringBuilder();
        csv.AppendLine("OperationName,TotalCount,SuccessCount,FailureCount,SuccessRate,AvgTimeMs,MinTimeMs,MaxTimeMs,TotalTimeMs");

        foreach (var metric in metrics)
        {
            csv.AppendLine($"\"{metric.Name}\",{metric.TotalCount},{metric.SuccessCount},{metric.FailureCount},{metric.SuccessRate * 100:F2},{metric.AverageTimeMs:F2},{metric.MinTimeMs},{metric.MaxTimeMs},{metric.TotalTimeMs}");
        }

        return csv.ToString();
    }

    /// <summary>Get top N slowest operations by average execution time</summary>
    /// <param name="monitor">The performance monitor instance</param>
    /// <param name="count">Number of operations to return (default: 5)</param>
    /// <returns>List of operation metrics sorted by average time, descending</returns>
    public static List<OperationMetrics> GetSlowestOperations(this PerformanceMonitor monitor, int count = 5)
    {
        return monitor.GetAllMetrics()
            .OrderByDescending(m => m.AverageTimeMs)
            .Take(count)
            .ToList();
    }

    /// <summary>Get operations that exceed a specific failure rate threshold</summary>
    /// <param name="monitor">The performance monitor instance</param>
    /// <param name="failureRateThreshold">Failure rate threshold (0.0 to 1.0)</param>
    /// <returns>List of operation metrics with failure rate above threshold</returns>
    public static List<OperationMetrics> GetOperationsWithHighFailureRate(this PerformanceMonitor monitor, double failureRateThreshold = 0.3)
    {
        return monitor.GetAllMetrics()
            .Where(m => m.TotalCount > 0 && (double)m.FailureCount / m.TotalCount > failureRateThreshold)
            .OrderByDescending(m => (double)m.FailureCount / m.TotalCount)
            .ToList();
    }
}