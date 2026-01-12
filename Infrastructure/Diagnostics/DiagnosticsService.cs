// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Infrastructure.Middleware;
using CoubDownloader.Infrastructure.Statistics;
using CoubDownloader.Infrastructure.Utilities;

namespace CoubDownloader.Infrastructure.Diagnostics;

/// <summary>Application diagnostics and health check service</summary>
public class DiagnosticsService
{
    private readonly ILoggingService _logger;
    private readonly PerformanceMonitor _performanceMonitor;
    private DateTime _startTime = DateTime.UtcNow;

    public DiagnosticsService(ILoggingService logger, PerformanceMonitor performanceMonitor)
    {
        _logger = logger;
        _performanceMonitor = performanceMonitor;
    }

    /// <summary>Perform comprehensive health check</summary>
    public DiagnosticsReport PerformHealthCheck()
    {
        var report = new DiagnosticsReport
        {
            Timestamp = DateTime.UtcNow,
            UpTime = DateTime.UtcNow - _startTime,
            AppInfo = VersionHelper.GetApplicationInfo(),
            RuntimeStats = GetRuntimeStatistics(),
            PerformanceMetrics = _performanceMonitor.GetAllMetrics(),
            IsHealthy = true
        };

        // Check memory pressure
        var memMb = RuntimeStatistics.GetMemoryUsageMb();
        if (memMb > 1000) // 1GB
        {
            report.Warnings.Add($"High memory usage: {memMb}MB");
            _logger.LogWarning($"High memory usage detected: {memMb}MB", "Diagnostics");
        }

        // Check disk space
        var diskspaceMb = GetAvailableDiskSpaceMb();
        if (diskspaceMb < 500) // 500MB
        {
            report.Warnings.Add($"Low disk space: {diskspaceMb}MB");
            _logger.LogWarning($"Low disk space available: {diskspaceMb}MB", "Diagnostics");
            report.IsHealthy = false;
        }

        // Check FFmpeg availability
        var ffmpegAvailable = CheckFFmpegAvailability();
        report.FFmpegAvailable = ffmpegAvailable;
        if (!ffmpegAvailable)
        {
            report.Warnings.Add("FFmpeg is not available");
        }

        return report;
    }

    /// <summary>Get detailed diagnostics report as string</summary>
    public string GetDiagnosticsString()
    {
        var report = PerformHealthCheck();
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("╔═══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║              APPLICATION DIAGNOSTICS REPORT                   ║");
        sb.AppendLine("╠═══════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║ Status: {(report.IsHealthy ? "✓ HEALTHY" : "✗ UNHEALTHY"),-52} ║");
        sb.AppendLine($"║ Timestamp: {report.Timestamp:yyyy-MM-dd HH:mm:ss,-48} ║");
        sb.AppendLine($"║ Uptime: {report.UpTime.ToString(@"dd\:hh\:mm\:ss"),-54} ║");
        sb.AppendLine("╠═══════════════════════════════════════════════════════════════╣");

        sb.AppendLine("║ SYSTEM INFORMATION:");
        sb.AppendLine($"║ - App Version: {report.AppInfo.AppVersion,-44} ║");
        sb.AppendLine($"║ - Runtime: {report.AppInfo.RuntimeVersion,-50} ║");
        sb.AppendLine($"║ - OS: {report.AppInfo.OperatingSystem,-56} ║");
        sb.AppendLine($"║ - Processors: {report.AppInfo.ProcessorCount,-48} ║");
        sb.AppendLine($"║ - Architecture: {(report.AppInfo.Is64BitProcess ? "x64" : "x86"),-45} ║");

        sb.AppendLine("║ RUNTIME STATISTICS:");
        sb.AppendLine($"║ - Memory Usage: {report.RuntimeStats.MemoryMb}MB{new string(' ', 41)} ║");
        sb.AppendLine($"║ - GC Collections (Gen0): {report.RuntimeStats.Gen0Collections,-36} ║");
        sb.AppendLine($"║ - GC Collections (Gen1): {report.RuntimeStats.Gen1Collections,-36} ║");
        sb.AppendLine($"║ - GC Collections (Gen2): {report.RuntimeStats.Gen2Collections,-36} ║");

        if (report.Warnings.Count > 0)
        {
            sb.AppendLine("║ WARNINGS:");
            foreach (var warning in report.Warnings)
            {
                var truncated = warning.Length > 50 ? warning.Substring(0, 47) + "..." : warning;
                sb.AppendLine($"║ - {truncated,-52} ║");
            }
        }

        sb.AppendLine("║ TOOLS:");
        sb.AppendLine($"║ - FFmpeg Available: {(report.FFmpegAvailable ? "✓ Yes" : "✗ No"),-40} ║");

        sb.AppendLine("╚═══════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }

    private RuntimeStatisticsData GetRuntimeStatistics()
    {
        var gcStats = RuntimeStatistics.GetGcStatistics();

        return new RuntimeStatisticsData
        {
            MemoryMb = RuntimeStatistics.GetMemoryUsageMb(),
            Gen0Collections = gcStats.Gen0Collections,
            Gen1Collections = gcStats.Gen1Collections,
            Gen2Collections = gcStats.Gen2Collections,
            TotalMemoryMb = gcStats.TotalMemoryBytes / (1024 * 1024)
        };
    }

    private bool CheckFFmpegAvailability()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null) return false;

            var completed = process.WaitForExit(5000);
            return completed && process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private long GetAvailableDiskSpaceMb()
    {
        try
        {
            var drive = System.IO.DriveInfo.GetDrives().FirstOrDefault();
            return drive?.AvailableFreeSpace / (1024 * 1024) ?? 0;
        }
        catch
        {
            return 0;
        }
    }
}

/// <summary>Diagnostics report data</summary>
public class DiagnosticsReport
{
    public DateTime Timestamp { get; set; }
    public TimeSpan UpTime { get; set; }
    public ApplicationInfo AppInfo { get; set; } = new();
    public RuntimeStatisticsData RuntimeStats { get; set; } = new();
    public List<OperationMetrics> PerformanceMetrics { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public bool FFmpegAvailable { get; set; }
    public bool IsHealthy { get; set; }
}

/// <summary>Runtime statistics data</summary>
public class RuntimeStatisticsData
{
    public long MemoryMb { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public long TotalMemoryMb { get; set; }
}
