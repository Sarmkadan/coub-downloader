# PerformanceMonitor

`PerformanceMonitor` collects per-operation timing and success/failure metrics for the Coub Downloader, and `RuntimeStatistics` exposes process-level memory, CPU and garbage-collection figures. Both live in `Infrastructure/Statistics/PerformanceMonitor.cs`; the report helpers are extension methods in `Infrastructure/Statistics/PerformanceMonitorExtensions.cs`.

The monitor is registered as a singleton in `Infrastructure/DependencyInjectionExtended.cs` and is consumed by `DiagnosticsService`.

## Types

| Type | Kind | Purpose |
|------|------|---------|
| `PerformanceMonitor` | class | Holds metrics per operation name; creates timers; produces reports. |
| `OperationTimer` | class (`IDisposable`) | Measures one operation and records it on `Dispose()`. |
| `OperationMetrics` | class | Aggregated data for one operation name. |
| `RuntimeStatistics` | class (static members) | Process memory, CPU and GC snapshot. |
| `GcStatistics` | class | GC collection counts and managed heap size. |
| `PerformanceMonitorExtensions` | static class | Per-operation report, CSV export, slowest-operations and high-failure-rate queries. |

## Metrics tracked

Metrics are kept per operation name. Each `OperationMetrics` instance holds:

| Member | Type | Meaning |
|--------|------|---------|
| `Name` | `string` | Operation name passed to `StartOperation`. |
| `TotalCount` | `long` | Number of recorded completions (success + failure). |
| `SuccessCount` | `long` | Completions marked successful. |
| `FailureCount` | `long` | Completions marked failed (`MarkFailed()`). |
| `TotalTimeMs` | `long` | Sum of elapsed milliseconds across all completions. |
| `MinTimeMs` | `long` | Smallest recorded elapsed time (see caveat below). |
| `MaxTimeMs` | `long` | Largest recorded elapsed time. |
| `AverageTimeMs` | `double` (computed) | `TotalTimeMs / TotalCount`, or `0` when `TotalCount` is `0`. |
| `SuccessRate` | `double` (computed) | `SuccessCount / TotalCount` as a fraction (0.0–1.0), or `0` when `TotalCount` is `0`. |

Elapsed time comes from a `Stopwatch` started when the `OperationTimer` is created and stopped on `Dispose()`. Elapsed time is stored in whole milliseconds (`ElapsedMilliseconds`, truncated).

Every timer counts as a success unless `MarkFailed()` is called. `MarkSuccess()` resets the flag back to success.

`RuntimeStatistics` and `GcStatistics` are not stored anywhere; each call returns a fresh snapshot.

## Public API

### `PerformanceMonitor`

#### `public OperationTimer StartOperation(string operationName)`

Starts timing an operation. Use with a `using` statement so the result is recorded even when the operation throws.

- **Parameters**: `operationName` — key under which the metrics are aggregated.
- **Return Value**: A new `OperationTimer`. Nothing is recorded until it is disposed.

#### `public OperationMetrics? GetMetrics(string operationName)`

Returns the live `OperationMetrics` object for an operation, or `null` if nothing has been recorded under that name.

- **Remarks**: The returned object is the stored instance, not a copy. Reading it while other threads record into the same operation may show values that are not yet consistent with each other.

#### `public List<OperationMetrics> GetAllMetrics()`

Returns a new list containing every recorded operation. The list is a snapshot, but the `OperationMetrics` objects inside it are the stored instances.

#### `public void Clear()`

Removes all recorded metrics. Timers that have not yet been disposed will still record into the monitor when they are disposed.

#### `public string GetSummaryReport()`

Returns a boxed plain-text report with one block per operation: count, average time, min/max time and success rate.

- **Remarks**: Average time is shown as an integer (`TotalTimeMs / TotalCount` with integer division). An empty monitor produces a header and footer with no operation rows.

### `OperationTimer`

#### `public OperationTimer(string operationName, PerformanceMonitor monitor)`

Creates a timer and starts its stopwatch. Normally created via `PerformanceMonitor.StartOperation`; the constructor is public, so it can also be called directly.

#### `public void MarkFailed()`

Marks the operation as failed. Sets the outcome for the record that is written on `Dispose()`.

#### `public void MarkSuccess()`

Marks the operation as successful (the default).

#### `public void Dispose()`

Stops the stopwatch and records the elapsed time and outcome into the owning monitor. Idempotent: only the first call records anything.

### `RuntimeStatistics` (static)

#### `public static long GetMemoryUsageMb()`

Returns the process working set (`Process.WorkingSet64`) in whole mebibytes (MiB).

#### `public static double GetCpuUsagePercent()`

Returns the CPU usage of the current process as a percentage of total machine capacity (`Environment.ProcessorCount` cores), clamped to 0–100.

- **Remarks**: The value is averaged over the interval since the previous call. The first call measures from process start time. State is static and shared across the application, so calls from different callers affect each other's intervals. Any exception (for example, from reading process start time) is swallowed and `0` is returned.

#### `public static GcStatistics GetGcStatistics()`

Returns a snapshot with `GC.CollectionCount(0)`, `(1)` and `(2)`, plus `GC.GetTotalMemory(false)`. No collection is forced.

### `GcStatistics`

| Property | Type | Meaning |
|----------|------|---------|
| `Gen0Collections` | `int` | Gen 0 collections since process start. |
| `Gen1Collections` | `int` | Gen 1 collections since process start. |
| `Gen2Collections` | `int` | Gen 2 collections since process start. |
| `TotalMemoryBytes` | `long` | Managed heap size in bytes, as reported by `GC.GetTotalMemory(false)`. |

## Extension methods

All extension methods are on `PerformanceMonitor` and live in the `CoubDownloader.Infrastructure.Statistics` namespace.

#### `public static string? GetOperationReport(this PerformanceMonitor monitor, string operationName)`

Formatted single-operation report (count, success/failure counts, success rate, average/min/max/total time). Returns `null` if the operation is unknown.

- **Exceptions**: `ArgumentNullException` if `monitor` is `null`; `ArgumentException` if `operationName` is `null` or empty.

#### `public static string GetCsvReport(this PerformanceMonitor monitor)`

CSV export with the header `OperationName,TotalCount,SuccessCount,FailureCount,SuccessRate,AvgTimeMs,MinTimeMs,MaxTimeMs,TotalTimeMs`, one row per operation. `SuccessRate` is a percentage with two decimals; `AvgTimeMs` has two decimals. Names are quoted, with embedded quotes doubled. With no data, the output is the header followed by `No data available`.

- **Exceptions**: `ArgumentNullException` if `monitor` is `null`.

#### `public static List<OperationMetrics> GetSlowestOperations(this PerformanceMonitor monitor, int count = 5)`

Top `count` operations ordered by `AverageTimeMs`, descending.

- **Exceptions**: `ArgumentNullException` if `monitor` is `null`; `ArgumentOutOfRangeException` if `count` is negative.

#### `public static List<OperationMetrics> GetOperationsWithHighFailureRate(this PerformanceMonitor monitor, double failureRateThreshold = 0.3)`

Operations whose failure rate (`FailureCount / TotalCount`) is strictly greater than the threshold, ordered by failure rate, descending.

- **Exceptions**: `ArgumentNullException` if `monitor` is `null`; `ArgumentOutOfRangeException` if the threshold is below 0.0 or above 1.0.

## Thread safety

- `PerformanceMonitor` methods lock on a private object. Recording, querying, clearing and building the summary report are serialized.
- `OperationTimer` is not thread-safe; use one instance per operation.
- `RuntimeStatistics.GetCpuUsagePercent()` uses its own lock around its static sampling state.
- `OperationMetrics` has no locking of its own. The objects returned by `GetMetrics` and `GetAllMetrics` are the live stored instances and can be mutated by callers; treat them as read-only.

## Usage

```csharp
public sealed class CoubFetcher
{
    private readonly PerformanceMonitor _monitor;

    public CoubFetcher(PerformanceMonitor monitor) => _monitor = monitor;

    public async Task<string> FetchAsync(string url)
    {
        using var timer = _monitor.StartOperation("coub.fetch");
        try
        {
            return await DownloadAsync(url);
        }
        catch
        {
            timer.MarkFailed();
            throw;
        }
    }
}

// Reporting
Console.WriteLine(monitor.GetSummaryReport());
var slowest = monitor.GetSlowestOperations(3);
var flaky = monitor.GetOperationsWithHighFailureRate(0.1);
File.WriteAllText("metrics.csv", monitor.GetCsvReport());

// Process snapshot
long mb = RuntimeStatistics.GetMemoryUsageMb();
double cpu = RuntimeStatistics.GetCpuUsagePercent();
GcStatistics gc = RuntimeStatistics.GetGcStatistics();
```

## Caveats

- **`MinTimeMs` after a zero sample.** The update rule treats a stored minimum of `0` as "unset". If one recorded sample is `0 ms`, the next sample overwrites the minimum even when it is larger, so `MinTimeMs` can report a value that is not the true minimum. Non-zero samples are unaffected.
- **Unmarked failures.** Exceptions are not recorded automatically. Call `MarkFailed()` in a `catch` block, or the operation is counted as a success.
- **Timer lifetime.** A timer that is never disposed records nothing.
- **Millisecond resolution.** Sub-millisecond operations are recorded as `0`.
- **CPU sampling.** `GetCpuUsagePercent()` depends on how often it is called and shares state across callers.
