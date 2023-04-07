# ILoggingService

The `ILoggingService` interface defines the contract for logging operations within the `coub-downloader` application, providing a standardized mechanism to record informational, warning, error, and debug messages. It facilitates both real-time logging events and retrospective analysis by exposing methods to write log entries and retrieve a read-only collection of historical logs, ensuring consistent observability across the download process and related services.

## API

### Logging Methods

#### `void LogInfo(string message)`
Records an informational message indicating normal operational progress.
*   **Parameters**:
    *   `message`: The string content of the log entry.
*   **Returns**: `void`.
*   **Throws**: No specific exceptions defined by the interface; implementation-dependent behavior may occur if the underlying storage fails.

#### `void LogWarning(string message)`
Records a warning message indicating a potential issue or non-critical anomaly that does not halt execution.
*   **Parameters**:
    *   `message`: The string content of the log entry.
*   **Returns**: `void`.
*   **Throws**: No specific exceptions defined by the interface.

#### `void LogError(string message)`
Records an error message indicating a critical failure or exception that may impact functionality.
*   **Parameters**:
    *   `message`: The string content of the log entry.
*   **Returns**: `void`.
*   **Throws**: No specific exceptions defined by the interface.

#### `void LogDebug(string message)`
Records a detailed debug message intended for development and troubleshooting purposes.
*   **Parameters**:
    *   `message`: The string content of the log entry.
*   **Returns**: `void`.
*   **Throws**: No specific exceptions defined by the interface.

### Data Retrieval

#### `IReadOnlyList<LogEntry> GetLogs()`
Retrieves the current collection of logged entries.
*   **Parameters**: None.
*   **Returns**: An `IReadOnlyList<LogEntry>` containing all captured log entries up to the point of invocation.
*   **Throws**: No specific exceptions defined by the interface.

### LogEntry Properties

The `LogEntry` type, returned by `GetLogs`, exposes the following public members:

#### `DateTime Timestamp`
Gets the precise date and time when the log entry was created.
*   **Returns**: A `DateTime` structure.

#### `string Level`
Gets the severity level of the log entry (e.g., "Info", "Warning", "Error", "Debug").
*   **Returns**: A string representing the log level.

#### `string Category`
Gets the categorical classification of the log source or context.
*   **Returns**: A string identifying the category.

#### `string Message`
Gets the actual text content of the log entry.
*   **Returns**: The log message string.

#### `override string ToString()`
Returns a string representation of the `LogEntry`, typically formatting the timestamp, level, category, and message into a single readable line.
*   **Returns**: A formatted string.
*   **Throws**: No specific exceptions.

### Implementation Note

#### `FileLoggingService`
The concrete implementation of this interface provided in the project is `FileLoggingService`. It adheres to the `ILoggingService` contract and persists logs to the file system.

## Usage

### Example 1: Basic Logging and Retrieval
This example demonstrates injecting the service, logging various severity levels during a hypothetical download operation, and retrieving the logs for verification.

```csharp
public class DownloadManager
{
    private readonly ILoggingService _logger;

    public DownloadManager(ILoggingService logger)
    {
        _logger = logger;
    }

    public void ExecuteDownload(string url)
    {
        _logger.LogInfo($"Starting download for {url}");
        
        try
        {
            // Simulated operation
            _logger.LogDebug("Connecting to stream...");
            
            // Simulated warning scenario
            _logger.LogWarning("Connection latency detected");
            
            _logger.LogInfo("Download completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Download failed: {ex.Message}");
        }

        // Retrieve and inspect logs
        var logs = _logger.GetLogs();
        foreach (var entry in logs)
        {
            Console.WriteLine(entry.ToString());
        }
    }
}
```

### Example 2: Filtering Logs by Level
This example shows how to retrieve logs and filter them specifically for errors to generate a summary report.

```csharp
public class HealthCheck
{
    private readonly ILoggingService _logger;

    public HealthCheck(ILoggingService logger)
    {
        _logger = logger;
    }

    public string GetErrorSummary()
    {
        var allLogs = _logger.GetLogs();
        var errors = allLogs.Where(l => l.Level == "Error").ToList();

        if (!errors.Any())
        {
            return "No errors recorded.";
        }

        return $"Found {errors.Count} errors. Latest: {errors.Last().Message} at {errors.Last().Timestamp}";
    }
}
```

## Notes

*   **Thread Safety**: The interface definition itself does not enforce thread safety. However, given that `GetLogs` returns a list of accumulated entries while `Log*` methods potentially mutate the underlying collection, implementations like `FileLoggingService` must handle concurrent access internally. Consumers should assume that `GetLogs` provides a snapshot at the time of calling, but simultaneous writes during enumeration of the returned list (if cast improperly) should be avoided.
*   **Memory Management**: The `GetLogs` method returns an `IReadOnlyList<LogEntry>`. In long-running applications, unbounded accumulation of log entries in memory could lead to performance degradation. Implementations may impose internal limits or rely on the consumer to periodically process and clear logs if the underlying storage mechanism supports it.
*   **Immutability**: The returned `LogEntry` objects expose properties (`Timestamp`, `Level`, `Category`, `Message`) that appear to be read-only getters. Consumers should treat these entries as immutable records of past events.
*   **String Formatting**: The `ToString()` override on `LogEntry` is the recommended way to render a human-readable version of the log for console output or simple text files, ensuring consistent formatting across the application.
