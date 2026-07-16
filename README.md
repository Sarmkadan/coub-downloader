// existing content ...

## RetryHelper

`RetryHelper` provides a set of utility methods for executing operations with retry and fallback strategies. It helps to make operations more resilient to transient failures.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Utilities;

// Example 1: Execute with exponential backoff retry
var data = await RetryHelper.ExecuteWithRetryAsync(
    () => FetchDataAsync(),
    maxRetries: 3,
    initialDelayMs: 500,
    shouldRetry: (ex, attempt) => ex is TimeoutException
);

Console.WriteLine($"Fetched data: {data}");

// Example 2: Execute with linear retry
var data2 = await RetryHelper.ExecuteWithLinearRetryAsync(
    () => FetchDataAsync(),
    maxRetries: 5,
    delayMs: 1000
);

Console.WriteLine($"Fetched data: {data2}");

// Example 3: Execute with fallback
var data3 = await RetryHelper.ExecuteWithFallbackAsync(
    () => FetchDataAsync(),
    async () => await FetchFallbackDataAsync()
);

Console.WriteLine($"Fetched data: {data3}");

// Example 4: Execute with timeout
var data4 = await RetryHelper.ExecuteWithTimeoutAsync(
    () => FetchDataAsync(),
    TimeSpan.FromSeconds(10)
);

Console.WriteLine($"Fetched data: {data4}");
```

## DiagnosticsService

The `DiagnosticsService` gathers runtime and application health information, producing a detailed diagnostics report. It checks memory usage, disk space, FFmpeg availability, and aggregates performance metrics, exposing both a structured `DiagnosticsReport` object and a formatted string representation.

### Usage Example

```csharp
using System;
using CoubDownloader.Infrastructure.Diagnostics;
using CoubDownloader.Infrastructure.Middleware;
using CoubDownloader.Infrastructure.Statistics;
using CoubDownloader.Infrastructure.Utilities;

var logger = new MemoryLoggingService();          // In‑memory logger implementation
var performanceMonitor = new PerformanceMonitor(); // Collects operation metrics

var diagnostics = new DiagnosticsService(logger, performanceMonitor);

// Perform a health check and obtain the structured report
DiagnosticsReport report = diagnostics.PerformHealthCheck();

Console.WriteLine("Diagnostics Report:");
Console.WriteLine($"Timestamp: {report.Timestamp}");
Console.WriteLine($"Uptime: {report.UpTime}");
Console.WriteLine($"App Version: {report.AppInfo.AppVersion}");
Console.WriteLine($"Memory Usage: {report.RuntimeStats.MemoryMb} MB");
Console.WriteLine($"GC Collections – Gen0: {report.RuntimeStats.Gen0Collections}, Gen1: {report.RuntimeStats.Gen1Collections}, Gen2: {report.RuntimeStats.Gen2Collections}");
Console.WriteLine($"FFmpeg Available: {report.FFmpegAvailable}");
Console.WriteLine($"Healthy: {report.IsHealthy}");
if (report.Warnings.Count > 0)
{
    Console.WriteLine("Warnings:");
    report.Warnings.ForEach(w => Console.WriteLine($"- {w}"));
}

// Get a formatted string representation of the diagnostics
string diagnosticsString = diagnostics.GetDiagnosticsString();
Console.WriteLine(diagnosticsString);
```
