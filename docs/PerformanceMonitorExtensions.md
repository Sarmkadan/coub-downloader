# PerformanceMonitorExtensions

The `PerformanceMonitorExtensions` class provides a set of static utility methods designed to extract, format, and analyze performance data collected by the application's monitoring subsystem. It serves as an interface for generating human-readable reports, CSV exports for external analysis, and filtered lists of operations based on latency or failure criteria, facilitating diagnostics and performance tuning within the `coub-downloader` project.

## API

### `GetOperationReport`
Generates a summarized text report of the current performance metrics.
- **Signature**: `public static string? GetOperationReport()`
- **Purpose**: Creates a formatted string representation of overall operation statistics, suitable for logging or console output.
- **Parameters**: None.
- **Return Value**: A `string` containing the report if data is available; otherwise, `null`.
- **Exceptions**: This method does not throw exceptions under normal operation; it returns `null` if the underlying monitor has not recorded any data.

### `GetCsvReport`
Exports the current performance metrics into a Comma-Separated Values (CSV) format.
- **Signature**: `public static string GetCsvReport()`
- **Purpose**: Produces a CSV-formatted string including headers and rows for each recorded operation, enabling import into spreadsheet software or data analysis tools.
- **Parameters**: None.
- **Return Value**: A `string` containing the CSV data. If no operations are recorded, returns a string containing only the header row or an empty string depending on implementation specifics.
- **Exceptions**: Does not typically throw exceptions unless memory allocation fails for extremely large datasets.

### `GetSlowestOperations`
Retrieves a list of operations sorted by duration to identify performance bottlenecks.
- **Signature**: `public static List<OperationMetrics> GetSlowestOperations()`
- **Purpose**: Queries the performance store to return operations with the highest execution times.
- **Parameters**: None. (Note: Specific count limits or thresholds are handled internally or via default configuration).
- **Return Value**: A `List<OperationMetrics>` containing the slowest recorded operations. Returns an empty list if no data exists.
- **Exceptions**: Does not throw exceptions; returns an empty collection if the monitor is empty.

### `GetOperationsWithHighFailureRate`
Identifies operations that exhibit a failure rate exceeding a predefined threshold.
- **Signature**: `public static List<OperationMetrics> GetOperationsWithHighFailureRate()`
- **Purpose**: Filters recorded operations to isolate those with significant error rates, aiding in reliability analysis.
- **Parameters**: None. (Thresholds are defined internally).
- **Return Value**: A `List<OperationMetrics>` containing operations flagged for high failure rates. Returns an empty list if no operations meet the criteria.
- **Exceptions**: Does not throw exceptions; returns an empty collection if no data matches the failure criteria.

## Usage

### Generating a Diagnostic Log Entry
The following example demonstrates how to generate a text report and log it only if data is present, preventing null reference issues in the logging pipeline.

```csharp
// Attempt to retrieve the human-readable report
string? report = PerformanceMonitorExtensions.GetOperationReport();

if (!string.IsNullOrEmpty(report))
{
    // Log the report to the diagnostic system
    Logger.LogInformation("Performance Snapshot:\n{Report}", report);
}
else
{
    Logger.LogWarning("No performance data available for reporting.");
}
```

### Exporting Data for Analysis and Bottleneck Detection
This example illustrates exporting metrics to a CSV file for external review and simultaneously fetching the slowest operations to trigger an alert if latency exceeds expectations.

```csharp
// Export full dataset to CSV for archival or deep analysis
string csvData = PerformanceMonitorExtensions.GetCsvReport();
await File.WriteAllTextAsync("performance_export.csv", csvData);

// Retrieve the top slowest operations to check for specific bottlenecks
var slowOps = PerformanceMonitorExtensions.GetSlowestOperations();

if (slowOps.Any() && slowOps.First().DurationMs > 5000)
{
    Console.WriteLine($"Alert: Detected operations exceeding 5s latency. Count: {slowOps.Count}");
    foreach (var op in slowOps.Take(5))
    {
        Console.WriteLine($"- {op.OperationName}: {op.DurationMs}ms");
    }
}
```

## Notes

- **Thread Safety**: As these methods access shared performance data stores, they are expected to be thread-safe for read operations. However, callers should be aware that the returned data represents a snapshot in time; the state of the monitor may change immediately after the method returns.
- **Null Handling**: Only `GetOperationReport` returns a nullable type (`string?`). Consumers must explicitly check for `null` before using the result. The other methods guarantee non-null returns, providing empty strings or empty lists when no data is available.
- **Data Availability**: If the performance monitor has not been initialized or no operations have been executed since the application start, `GetSlowestOperations` and `GetOperationsWithHighFailureRate` will return empty lists rather than throwing errors.
- **Formatting**: The `GetCsvReport` method does not include a Byte Order Mark (BOM) by default. When writing to files for use in legacy Windows applications (e.g., older Excel versions), consumers may need to prepend a BOM manually.
