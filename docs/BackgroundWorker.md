# BackgroundWorker

The `BackgroundWorker` class in the `coub-downloader` project serves as a foundational component for managing long-running system monitoring and cleanup operations without blocking the main application thread. It aggregates real-time system resource metrics—such as available memory, disk space, and processor count—and orchestrates background tasks like periodic cleanup and health monitoring. By inheriting from a timed base class, it ensures that monitoring intervals are strictly enforced, while providing asynchronous control mechanisms to start and stop these background processes safely.

## API

### `Start()`
Initiates the background operations defined within the worker, including the cleanup and monitoring loops.
*   **Parameters**: None.
*   **Return Value**: `void`.
*   **Exceptions**: May throw an `InvalidOperationException` if the worker is already running or if underlying system resources required to start the threads are unavailable.

### `StopAsync()`
Asynchronously signals the worker to cease all background operations and waits for any current iteration of the cleanup or monitoring tasks to complete gracefully.
*   **Parameters**: None.
*   **Return Value**: `Task`. The returned task completes when the worker has fully stopped.
*   **Exceptions**: May throw if the internal cancellation token cannot be triggered or if the task enters an unrecoverable state during shutdown.

### `CleanupWorker`
A public member representing the specific worker instance or delegate responsible for performing garbage collection, temporary file removal, or cache invalidation tasks.
*   **Parameters**: N/A (Member access).
*   **Return Value**: N/A.
*   **Exceptions**: Accessing this member does not throw, but invoking its underlying logic may throw I/O exceptions depending on the implementation of the cleanup routine.

### `MonitoringWorker()`
The constructor for the `BackgroundWorker` class. It initializes the instance and configures the base class timing mechanism.
*   **Parameters**: None (explicitly listed in signature context).
*   **Return Value**: N/A (Constructor).
*   **Behavior**: Calls the base constructor with a fixed interval of `TimeSpan.FromMinutes`, establishing the frequency at which the monitoring loop executes.
*   **Exceptions**: May throw if the base class initialization fails or if the system clock is inaccessible.

### `Timestamp`
Gets the precise date and time when the last monitoring cycle was executed or when the worker was last initialized.
*   **Parameters**: None.
*   **Return Value**: `DateTime`.
*   **Exceptions**: None.

### `AvailableMemory`
Retrieves the current amount of available physical memory on the host system in bytes.
*   **Parameters**: None.
*   **Return Value**: `long`.
*   **Exceptions**: May throw a platform-specific exception if the operating system denies access to performance counters or memory statistics.

### `ProcessorCount`
Retrieves the number of logical processors available on the current machine.
*   **Parameters**: None.
*   **Return Value**: `int`.
*   **Exceptions**: None.

### `AvailableDiskSpace`
Retrieves the total amount of free disk space available to the application in bytes.
*   **Parameters**: None.
*   **Return Value**: `long`.
*   **Exceptions**: May throw an `IOException` or `UnauthorizedAccessException` if the drive is inaccessible or permissions are insufficient.

## Usage

### Example 1: Initializing and Starting the Worker
This example demonstrates how to instantiate the `BackgroundWorker`, which automatically sets the monitoring interval, and start the background processes.

```csharp
using System;
using CoubDownloader.Workers;

public class Program
{
    public static void Main()
    {
        // Initialize the worker; constructor sets the timer interval
        var worker = new BackgroundWorker();

        Console.WriteLine($"System has {worker.ProcessorCount} processors.");
        Console.WriteLine($"Available memory: {worker.AvailableMemory} bytes");

        // Start the background cleanup and monitoring loops
        worker.Start();

        Console.WriteLine($"Monitoring started at: {worker.Timestamp}");
        
        // Application continues running while worker operates in the background
        Console.ReadLine();
    }
}
```

### Example 2: Graceful Shutdown with Resource Checks
This example shows how to check system resources before deciding to stop the worker asynchronously, ensuring no data is lost during the shutdown phase.

```csharp
using System;
using System.Threading.Tasks;
using CoubDownloader.Workers;

public class ShutdownManager
{
    private readonly BackgroundWorker _worker;

    public ShutdownManager()
    {
        _worker = new BackgroundWorker();
        _worker.Start();
    }

    public async Task SafeShutdownAsync()
    {
        // Check disk space before shutting down to ensure logs can be written
        if (_worker.AvailableDiskSpace < 1024 * 1024 * 100) // Less than 100MB
        {
            Console.WriteLine("Critical: Low disk space. Attempting immediate cleanup.");
            // Trigger cleanup logic via the CleanupWorker member if exposed as actionable
            // Note: Specific invocation depends on CleanupWorker's concrete type
        }

        Console.WriteLine("Stopping monitoring worker...");
        
        // Await the asynchronous stop operation
        await _worker.StopAsync();
        
        Console.WriteLine($"Worker stopped. Final timestamp: {_worker.Timestamp}");
    }
}
```

## Notes

*   **Thread Safety**: The properties `AvailableMemory`, `ProcessorCount`, and `AvailableDiskSpace` read system state at the moment of access. While the getters themselves are generally thread-safe, the values may become stale immediately after retrieval if the system load changes rapidly. Do not assume these values remain constant between calls.
*   **Initialization Timing**: The `MonitoringWorker` constructor hardcodes the base interval to minutes. This implies that the first monitoring cycle may not occur immediately upon `Start()`, but rather after the initial time span elapses, depending on the base class implementation.
*   **Asynchronous Stopping**: The `StopAsync` method must be awaited. Calling it without awaiting (fire-and-forget) may result in the application exiting before the cleanup routines finish, potentially leaving temporary files or locked resources behind.
*   **Resource Access Exceptions**: Accessing `AvailableDiskSpace` and `AvailableMemory` relies on OS-level APIs. In restricted environments (e.g., certain containerized setups or sandboxes), these properties may throw exceptions. Callers should wrap access to these properties in try-catch blocks if running in untrusted environments.
*   **CleanupWorker Dependency**: The `CleanupWorker` member is exposed publicly. Its behavior and thread-safety characteristics depend on its underlying implementation. If it shares state with the `MonitoringWorker`, concurrent modification during a `StopAsync` operation should be handled with care.
