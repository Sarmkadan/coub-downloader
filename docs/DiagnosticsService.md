# DiagnosticsService

The `DiagnosticsService` is a utility class designed to collect and report runtime diagnostics for the Coub Downloader application. It aggregates system metrics, performance counters, application metadata, and health indicators to assist in monitoring and troubleshooting.

## API

### `public DiagnosticsService`

Initializes a new instance of the `DiagnosticsService` class. This constructor collects baseline system information and initializes performance counters.

### `public DiagnosticsReport PerformHealthCheck()`

Executes a comprehensive health check of the application and system environment.

- **Return Value**: A `DiagnosticsReport` object containing aggregated diagnostics data, including health status, system metrics, and any warnings or errors encountered.
- **Exceptions**: May throw `InvalidOperationException` if required system components (e.g., FFmpeg) are unavailable or if critical metrics cannot be collected.

### `public string GetDiagnosticsString()`

Generates a human-readable string representation of the current diagnostics data.

- **Return Value**: A formatted string containing key diagnostics values such as uptime, memory usage, GC collections, and application information.
- **Exceptions**: None.

### `public DateTime Timestamp`

Gets the timestamp when the diagnostics data was collected.

- **Type**: `DateTime`
- **Remarks**: Read-only property reflecting the moment the diagnostics snapshot was taken.

### `public TimeSpan UpTime`

Gets the duration for which the application has been running.

- **Type**: `TimeSpan`
- **Remarks**: Read-only property calculated from process start time.

### `public ApplicationInfo AppInfo`

Gets metadata about the application, including version and build information.

- **Type**: `ApplicationInfo`
- **Remarks**: Read-only property containing immutable application details.

### `public RuntimeStatisticsData RuntimeStats`

Gets aggregated runtime statistics, including CPU usage and thread counts.

- **Type**: `RuntimeStatisticsData`
- **Remarks**: Read-only property populated during health check.

### `public List<OperationMetrics> PerformanceMetrics`

Gets a list of performance metrics for key operations, such as download and conversion tasks.

- **Type**: `List<OperationMetrics>`
- **Remarks**: Read-only property; may be empty if no operations have been tracked.

### `public List<string> Warnings`

Gets a list of warning messages generated during diagnostics collection.

- **Type**: `List<string>`
- **Remarks**: Read-only property; non-critical issues are appended here.

### `public bool FFmpegAvailable`

Indicates whether FFmpeg, a required dependency for media processing, is available on the system.

- **Type**: `bool`
- **Remarks**: Read-only property; `true` if FFmpeg is found in the system path or configured location.

### `public bool IsHealthy`

Indicates whether the application and system are in a healthy state based on collected diagnostics.

- **Type**: `bool`
- **Remarks**: Read-only property; `true` only if all critical checks pass and no fatal warnings exist.

### `public long MemoryMb`

Gets the current memory usage of the application in megabytes.

- **Type**: `long`
- **Remarks**: Read-only property reflecting the working set size.

### `public int Gen0Collections`

Gets the number of generation 0 garbage collections since the application started.

- **Type**: `int`
- **Remarks**: Read-only property; reflects GC activity over short-lived objects.

### `public int Gen1Collections`

Gets the number of generation 1 garbage collections since the application started.

- **Type**: `int`
- **Remarks**: Read-only property; reflects GC activity over medium-lived objects.

### `public int Gen2Collections`

Gets the number of generation 2 garbage collections since the application started.

- **Type**: `int`
- **Remarks**: Read-only property; reflects GC activity over long-lived objects.

### `public long TotalMemoryMb`

Gets the total system memory available in megabytes.

- **Type**: `long`
- **Remarks**: Read-only property reflecting total physical RAM.

## Usage

### Example 1: Basic Health Check and Report
