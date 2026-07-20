# Batch Processing Progress Reporting

## Overview

The `BatchProcessingService` now supports progress reporting via the `IProgress<BatchProgress>` interface. This allows you to track batch processing progress in real-time.

## Changes Made

### 1. New `BatchProgress` Class

Location: `Domain/Models/BatchProgress.cs`

The `BatchProgress` class contains:
- `Total`: Total number of tasks in the batch
- `Completed`: Number of completed tasks
- `Failed`: Number of failed tasks
- `CurrentItem`: Current task index (0-based)
- `CurrentTaskUrl`: URL of the currently processing task
- `ProgressPercent`: Calculated progress percentage
- `StatusMessage`: Human-readable status message

### 2. Updated `IBatchProcessingService` Interface

Location: `Application/Services/IBatchProcessingService.cs`

The `StartBatchAsync` method signature has been updated:

```csharp
Task<BatchJob> StartBatchAsync(
    string batchJobId,
    IProgress<BatchProgress>? progress = null,
    CancellationToken cancellationToken = default);
```

The `progress` parameter is optional (nullable), so existing code continues to work without changes.

### 3. Updated `BatchProcessingService` Implementation

Location: `Application/Services/BatchProcessingService.cs`

The service now:
- Reports initial progress when batch starts
- Reports progress for each task as it begins processing
- Reports progress when tasks complete (success or failure)
- Reports final progress when batch completes

### 4. Console Progress Reporter

Location: `ConsoleProgressReporter.cs`

A simple console-based progress reporter is provided for easy integration:

```csharp
var reporter = new ConsoleProgressReporter("My Batch");
await batchService.StartBatchAsync(batchId, reporter);
```

## Usage Examples

### Basic Usage (Console Output)

```csharp
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Models;

// Create a batch
var batchService = new BatchProcessingService(...);
var batch = await batchService.CreateBatchJobAsync("My Download Batch", @"./output");

// Add tasks
var tasks = new List<DownloadTask> { /* your tasks */ };
await batchService.AddTasksAsync(batch.Id, tasks);

// Start with progress reporting
var progressReporter = new ConsoleProgressReporter(batch.Name);
await batchService.StartBatchAsync(batch.Id, progressReporter);
```

### Custom Progress Reporter

```csharp
public class CustomProgressReporter : IProgress<BatchProgress>
{
    public void Report(BatchProgress value)
    {
        // Your custom logic here
        Console.WriteLine($"Progress: {value.ProgressPercent}% - {value.StatusMessage}");
        
        // Send to API, update UI, log to file, etc.
    }
}

// Usage
await batchService.StartBatchAsync(batch.Id, new CustomProgressReporter());
```

### Without Progress Reporting (Backward Compatible)

```csharp
// Existing code continues to work - progress parameter is optional
await batchService.StartBatchAsync(batch.Id);
```

## Progress Events

The progress reporting provides updates at these key points:

1. **Initial**: When batch processing starts (0% complete)
2. **Task Started**: When each task begins downloading
3. **Task Completed**: When each task finishes (success or failure)
4. **Final**: When batch completes (100% or failed state)


## Sample Output

```
[My Download Batch] 0% complete (0/5 tasks)
[My Download Batch] Processing: https://coub.com/view/12345 - 20% complete (1/5 tasks)
[My Download Batch] Processing: https://coub.com/view/67890 - 40% complete (2/5 tasks)
[My Download Batch] Processing: https://coub.com/view/abcde - 60% complete (3/5 tasks)
[My Download Batch] Processing: https://coub.com/view/fghij - 80% complete (4/5 tasks)
[My Download Batch] Processing complete
```

## Integration Notes

- The progress reporting is **thread-safe** and works correctly with parallel task processing
- Progress updates are sent immediately when state changes occur
- The `CurrentTaskUrl` field allows you to track which specific task is being processed
- Progress reporting does not affect batch processing performance
- All existing code continues to work without modifications (backward compatible)
