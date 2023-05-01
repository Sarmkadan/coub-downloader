# BatchJob

Represents a configurable batch of download and conversion tasks with progress tracking and state management.

## API

### `public string Id`
Unique identifier for the batch job. Must be set when creating a new job and must not change afterward.

### `public string Name`
Human-readable name for the batch job. Used for display and identification purposes.

### `public string? Description`
Optional descriptive text providing additional context about the batch job.

### `public List<DownloadTask> Tasks`
Collection of download tasks included in this batch. The list is mutable and may be modified before the job starts.

### `public ConversionSettings? SharedSettings`
Global conversion settings applied to all tasks in the batch. If null, each task must define its own settings.

### `public ProcessingState State`
Current execution state of the batch job (e.g., Pending, Running, Completed, Failed). Updated automatically during processing.

### `public int TotalTasks`
Total number of tasks in the batch. Set at initialization and immutable afterward.

### `public int CompletedTasks`
Number of tasks that have completed successfully. Updated during processing.

### `public int FailedTasks`
Number of tasks that failed during execution. Updated during processing.

### `public string OutputDirectory`
Filesystem path where output files are written. Must be a valid directory and writable.

### `public bool ContinueOnError`
If true, the batch continues processing remaining tasks even if one fails. If false, the batch stops on first error.

### `public DateTime? StartedAt`
Timestamp when the batch job started execution. Null if the job has not started.

### `public DateTime? CompletedAt`
Timestamp when the batch job completed (successfully or with errors). Null if the job is still running or pending.

### `public int MaxParallelTasks`
Maximum number of tasks allowed to run concurrently. Limits resource usage and prevents system overload.

### `public DateTime CreatedAt`
Timestamp when the batch job was created. Immutable after creation.

### `public DateTime UpdatedAt`
Timestamp of the last modification to the job state or properties. Updated automatically on changes.

### `public int GetProgressPercent()`
Calculates the percentage of tasks completed relative to `TotalTasks`.

- **Returns**: Integer between 0 and 100 representing completion progress.
- **Throws**: `InvalidOperationException` if `TotalTasks` is zero.

### `public TimeSpan? GetElapsedTime()`
Calculates the time elapsed since the job started.

- **Returns**: `TimeSpan` representing elapsed time, or null if the job has not started.
- **Throws**: `InvalidOperationException` if called before `StartedAt` is set.

### `public bool IsCompleted`
Indicates whether the batch job has finished processing (either successfully or with errors).

- **Returns**: `true` if `State` is `Completed` or `Failed`; otherwise, `false`.

### `public int GetPendingTaskCount()`
Returns the number of tasks that have not yet completed.

- **Returns**: Number of pending tasks (i.e., `TotalTasks - CompletedTasks - FailedTasks`).

## Usage

### Example 1: Creating and starting a batch job
