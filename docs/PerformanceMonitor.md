# PerformanceMonitor

The `PerformanceMonitor` class provides a comprehensive mechanism for tracking the execution metrics of operations within the `coub-downloader` application. It facilitates the measurement of operation duration, success/failure rates, and resource consumption, offering both instance-level tracking for specific tasks and static methods for monitoring global system resources such as memory, CPU, and garbage collection statistics.

## API

### Instance Members

#### `public string Name`
Gets or sets the identifier name for this specific monitor instance. This property is used to label reports and distinguish between different monitored operations.

#### `public long TotalCount`
Gets the total number of operations tracked by this monitor instance since the last clear or initialization.

#### `public long SuccessCount`
Gets the cumulative count of operations explicitly marked as successful via the `MarkSuccess` method.

#### `public long FailureCount`
Gets the cumulative count of operations explicitly marked as failed via the `MarkFailed` method.

#### `public long TotalTimeMs`
Gets the aggregate duration in milliseconds of all tracked operations.

#### `public long MinTimeMs`
Gets the shortest duration in milliseconds recorded for a single operation. Returns `0` if no operations have been completed.

#### `public long MaxTimeMs`
Gets the longest duration in milliseconds recorded for a single operation. Returns `0` if no operations have been completed.

#### `public int Gen0Collections`
Gets the number of Generation 0 garbage collections that have occurred during the lifetime of this monitor instance or the tracked period.

#### `public OperationTimer StartOperation`
Initiates a new operation timing session. This property acts as a factory or entry point to retrieve an `OperationTimer` instance used to measure the duration of a specific task. The returned timer must be disposed or stopped to record metrics.

#### `public OperationMetrics? GetMetrics`
Retrieves the aggregated metrics calculated up to the current moment.
*   **Return Value**: An `OperationMetrics` object containing current statistics, or `null` if no data is available.
*   **Exceptions**: Does not throw under normal conditions; returns `null` if the internal state is empty.

#### `public List<OperationMetrics> GetAllMetrics`
Retrieves a list containing historical or detailed metric entries recorded by this monitor.
*   **Return Value**: A `List<OperationMetrics>` containing individual or aggregated metric records.
*   **Exceptions**: May throw if the internal list is accessed concurrently without proper synchronization (see Notes).

#### `public void MarkSuccess`
Explicitly increments the `SuccessCount` and finalizes the current operation context as successful. This is typically called after an operation completes without errors.

#### `public void MarkFailed`
Explicitly increments the `FailureCount` and finalizes the current operation context as failed. This is typically called within a catch block or upon validation failure.

#### `public string GetSummaryReport`
Generates a human-readable text report summarizing the current performance statistics, including counts, timing averages, and min/max values.
*   **Return Value**: A formatted `string` containing the summary.
*   **Exceptions**: Does not throw unless string formatting fails due to internal state corruption.

#### `public void Clear`
Resets all counters, timers, and collected metrics to their initial state. After calling this method, `TotalCount`, `SuccessCount`, `FailureCount`, and time-related properties will return to zero.

#### `public void Dispose`
Releases unmanaged resources and stops any active internal timers. This method should be called when the monitor is no longer needed, typically within a `using` statement.

### Static Members

#### `public static long GetMemoryUsageMb`
Retrieves the current working set memory usage of the application in megabytes.
*   **Return Value**: A `long` representing memory usage in MB.
*   **Exceptions**: May throw `PlatformNotSupportedException` if the underlying OS API is unavailable.

#### `public static double GetCpuUsagePercent`
Calculates and returns the current CPU usage percentage for the process.
*   **Return Value**: A `double` representing CPU utilization (0.0 to 100.0+).
*   **Exceptions**: May throw if system performance counters are inaccessible.

#### `public static GcStatistics GetGcStatistics`
Retrieves detailed garbage collection statistics for the current application domain.
*   **Return Value**: A `GcStatistics` object containing GC generation data and pause times.
*   **Exceptions**: Does not throw under normal conditions.

## Usage

### Example 1: Tracking a Download Operation
This example demonstrates how to use `StartOperation` to time a specific download task and mark the result accordingly.

```csharp
using var monitor = new PerformanceMonitor { Name = "CoubDownload" };

try
{
    // Start timing the operation
    using var timer = monitor.StartOperation; 
    
    // Simulate download logic
    await DownloadService.FetchVideoAsync("videoId");
    
    // Mark as success upon completion
    monitor.MarkSuccess();
}
catch (Exception ex)
{
    // Mark as failure if an exception occurs
    monitor.MarkFailed();
    Console.WriteLine($"Download failed: {ex.Message}");
}

// Output individual stats
Console.WriteLine($"Total Attempts: {monitor.TotalCount}");
Console.WriteLine($"Failures: {monitor.FailureCount}");
```

### Example 2: Generating a System Health Report
This example illustrates retrieving global system metrics and generating a summary report after a batch of operations.

```csharp
var monitor = new PerformanceMonitor { Name = "BatchProcessor" };

// Simulate batch processing
for (int i = 0; i < 100; i++)
{
    using (monitor.StartOperation)
    {
        ProcessItem(i);
        monitor.MarkSuccess();
    }
}

// Retrieve system-wide static metrics
long memoryMb = PerformanceMonitor.GetMemoryUsageMb();
double cpuPercent = PerformanceMonitor.GetCpuUsagePercent();
var gcStats = PerformanceMonitor.GetGcStatistics();

// Generate and display the summary
string report = monitor.GetSummaryReport();
Console.WriteLine(report);
Console.WriteLine($"System Memory: {memoryMb} MB | CPU: {cpuPercent:F2}%");
Console.WriteLine($"GC Gen 0 Collections: {gcStats.Gen0Collections}");

monitor.Dispose();
```

## Notes

*   **Thread Safety**: The instance members of `PerformanceMonitor` (e.g., `MarkSuccess`, `StartOperation`, `Clear`) are not guaranteed to be thread-safe. If multiple threads access the same monitor instance concurrently, external synchronization (e.g., `lock` statement) is required to prevent race conditions on counters and the internal metrics list. The static methods (`GetMemoryUsageMb`, `GetCpuUsagePercent`, `GetGcStatistics`) are generally safe to call from multiple threads as they query system state.
*   **Timer Disposal**: The `OperationTimer` returned by `StartOperation` implements `IDisposable`. It is critical to dispose of this timer (preferably via a `using` block) to ensure the elapsed time is correctly recorded in `TotalTimeMs` and associated min/max calculations. Failure to dispose may result in incomplete metrics.
*   **Zero-State Behavior**: If no operations have been executed, `MinTimeMs` and `MaxTimeMs` will return `0`. Consumers should handle this case if calculating averages or variances to avoid division by zero errors or misleading data interpretation.
*   **Resource Overhead**: While designed for monitoring, frequent calls to static system resource methods (`GetCpuUsagePercent`) in tight loops may introduce measurable overhead due to OS interop costs. These should be sampled at reasonable intervals rather than per-operation.
