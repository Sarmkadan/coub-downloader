// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace CoubDownloader.Infrastructure.Statistics;

/// <summary>Performance monitoring and metrics collection</summary>
public class PerformanceMonitor
{
    private readonly Dictionary<string, OperationMetrics> _metrics = [];
    private readonly object _lockObj = new();

    /// <summary>Start tracking an operation</summary>
    public OperationTimer StartOperation(string operationName)
    {
        return new OperationTimer(operationName, this);
    }

    /// <summary>Record operation completion</summary>
    internal void RecordOperation(string operationName, long elapsedMs, bool success)
    {
        lock (_lockObj)
        {
            if (!_metrics.ContainsKey(operationName))
                _metrics[operationName] = new OperationMetrics { Name = operationName };

            var metric = _metrics[operationName];
            metric.TotalCount++;
            metric.TotalTimeMs += elapsedMs;

            if (success)
                metric.SuccessCount++;
            else
                metric.FailureCount++;

            if (elapsedMs > metric.MaxTimeMs)
                metric.MaxTimeMs = elapsedMs;

            if (metric.MinTimeMs == 0 || elapsedMs < metric.MinTimeMs)
                metric.MinTimeMs = elapsedMs;
        }
    }

    /// <summary>Get metrics for an operation</summary>
    public OperationMetrics? GetMetrics(string operationName)
    {
        lock (_lockObj)
        {
            return _metrics.TryGetValue(operationName, out var metrics) ? metrics : null;
        }
    }

    /// <summary>Get all metrics</summary>
    public List<OperationMetrics> GetAllMetrics()
    {
        lock (_lockObj)
        {
            return _metrics.Values.ToList();
        }
    }

    /// <summary>Clear all metrics</summary>
    public void Clear()
    {
        lock (_lockObj)
        {
            _metrics.Clear();
        }
    }

    /// <summary>Get summary report</summary>
    public string GetSummaryReport()
    {
        lock (_lockObj)
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("╔════════════════════════════════════════════════════════════╗");
            report.AppendLine("║           Performance Metrics Report                        ║");
            report.AppendLine("╠════════════════════════════════════════════════════════════╣");

            foreach (var metric in _metrics.Values)
            {
                var avgTime = metric.TotalCount > 0 ? metric.TotalTimeMs / metric.TotalCount : 0;
                var successRate = metric.TotalCount > 0
                    ? (double)metric.SuccessCount / metric.TotalCount * 100
                    : 0;

                report.AppendLine($"║ Operation: {metric.Name,-44} ║");
                report.AppendLine($"║   Count:        {metric.TotalCount,-44} ║");
                report.AppendLine($"║   Avg Time:     {avgTime}ms{new string(' ', 38 - avgTime.ToString().Length)} ║");
                report.AppendLine($"║   Min/Max:      {metric.MinTimeMs}ms / {metric.MaxTimeMs}ms{new string(' ', 30)} ║");
                report.AppendLine($"║   Success Rate: {successRate:F1}%{new string(' ', 44 - successRate.ToString("F1").Length)} ║");
            }

            report.AppendLine("╚════════════════════════════════════════════════════════════╝");
            return report.ToString();
        }
    }
}

/// <summary>Operation metrics data</summary>
public class OperationMetrics
{
    public string Name { get; set; } = "";
    public long TotalCount { get; set; }
    public long SuccessCount { get; set; }
    public long FailureCount { get; set; }
    public long TotalTimeMs { get; set; }
    public long MinTimeMs { get; set; }
    public long MaxTimeMs { get; set; }

    public double AverageTimeMs => TotalCount > 0 ? TotalTimeMs / (double)TotalCount : 0;
    public double SuccessRate => TotalCount > 0 ? SuccessCount / (double)TotalCount : 0;
}

/// <summary>Timer for tracking operation duration</summary>
public class OperationTimer : IDisposable
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly string _operationName;
    private readonly PerformanceMonitor _monitor;
    private bool _success = true;
    private bool _disposed;

    public OperationTimer(string operationName, PerformanceMonitor monitor)
    {
        _operationName = operationName;
        _monitor = monitor;
    }

    /// <summary>Mark operation as failed</summary>
    public void MarkFailed()
    {
        _success = false;
    }

    /// <summary>Mark operation as successful</summary>
    public void MarkSuccess()
    {
        _success = true;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _stopwatch.Stop();
        _monitor.RecordOperation(_operationName, _stopwatch.ElapsedMilliseconds, _success);
        _disposed = true;
    }
}

/// <summary>Real-time performance statistics</summary>
public class RuntimeStatistics
{
    /// <summary>Get current memory usage</summary>
    public static long GetMemoryUsageMb()
    {
        using var process = Process.GetCurrentProcess();
        return process.WorkingSet64 / (1024 * 1024);
    }

    /// <summary>Get CPU usage (not available on all platforms)</summary>
    public static double GetCpuUsagePercent()
    {
        try
        {
            // CPU monitoring requires System.Diagnostics.PerformanceCounter package
            // Simplified version without external dependency
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>Get garbage collection statistics</summary>
    public static GcStatistics GetGcStatistics()
    {
        return new GcStatistics
        {
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            TotalMemoryBytes = GC.GetTotalMemory(false)
        };
    }
}

/// <summary>Garbage collection statistics</summary>
public class GcStatistics
{
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public long TotalMemoryBytes { get; set; }
}
