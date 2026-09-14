# BatchProcessingService

`BatchProcessingService` (namespace `CoubDownloader.Application.Services`) is the
application-layer service that manages and executes **batch jobs** — groups of
Coub download tasks processed together with a shared output directory and
optional shared conversion settings. It implements `IBatchProcessingService`.

## Responsibilities

- **Batch lifecycle management** — create a batch, add tasks, start processing,
  cancel, query status, and delete completed batches.
- **Orchestration of task execution** — fans out the batch's pending tasks to
  `ICoubDownloadService` with a configurable parallelization limit
  (`BatchJob.MaxParallelTasks`, default `2`).
- **Progress reporting** — emits `BatchProgress` snapshots via an optional
  `IProgress<BatchProgress>` callback at task start, per-task completion/failure,
  and batch start/finish.
- **State bookkeeping** — persists task and batch state transitions through
  `IBatchJobRepository` and `IDownloadTaskRepository`, and tracks
  completed/failed counts.
- **Error handling** — wraps unexpected failures in `BatchProcessingException`
  while letting domain exceptions (`CoubDownloaderException`,
  `ResourceNotFoundException`, `InvalidOperationException`) propagate unchanged.
  Honors `BatchJob.ContinueOnError` to decide whether a task failure aborts the
  whole batch or is tolerated.

## Public API

All methods are `async` and accept an optional `CancellationToken`.

| Method | Signature | Behavior |
| --- | --- | --- |
| `CreateBatchJobAsync` | `Task<BatchJob>(string name, string outputDirectory, ConversionSettings? sharedSettings = null, CancellationToken = default)` | Creates a `BatchJob` in `Pending` state with a new GUID `Id`. Throws `ArgumentException` on blank `name`/`outputDirectory`. |
| `AddTasksAsync` | `Task(string batchJobId, IEnumerable<DownloadTask> tasks, CancellationToken = default)` | Assigns each task to the batch, computes its `OutputPath` as `<outputDirectory>/<url-filename>.mp4`, persists it, and updates `TotalTasks`. Throws `ResourceNotFoundException` if the batch is missing, `InvalidOperationException` if the batch is not `Pending`. |
| `StartBatchAsync` | `Task<BatchJob>(string batchJobId, IProgress<BatchProgress>? progress = null, CancellationToken = default)` | Transitions the batch to `Downloading`, processes all pending tasks in parallel (bounded by `MaxParallelTasks`), then sets the batch to `Completed` (no failures) or `Failed`. Throws `InvalidOperationException` if `CanStart()` is false. |
| `CancelBatchAsync` | `Task(string batchJobId, CancellationToken = default)` | Sets the batch and all running tasks to `Cancelled`. |
| `GetBatchStatusAsync` | `Task<BatchJob>(string batchJobId, CancellationToken = default)` | Returns the current `BatchJob`. Throws `ResourceNotFoundException` if missing. |
| `GetAllBatchesAsync` | `Task<IEnumerable<BatchJob>>(CancellationToken = default)` | Returns all batch jobs. |
| `GetActiveBatchesAsync` | `Task<IEnumerable<BatchJob>>(CancellationToken = default)` | Returns batches in `Pending`, `Downloading`, `Converting`, or `ProcessingAudio` state. |
| `DeleteBatchAsync` | `Task<bool>(string batchJobId, CancellationToken = default)` | Deletes a batch; returns `false` if not found. Throws `InvalidOperationException` if the batch is currently processing. |

### Constructor

```csharp
public BatchProcessingService(
    IBatchJobRepository batchRepository,
    IDownloadTaskRepository taskRepository,
    ICoubDownloadService downloadService,
    ILogger<BatchProcessingService>? logger = null)
```

Dependencies are injected; `logger` defaults to `NullLogger` when omitted.
`ArgumentNullException` is thrown for null repositories or download service.

## Usage

```csharp
var batch = await batchService.CreateBatchJobAsync(
    "My Coub batch",
    "/downloads/coubs",
    sharedSettings: new ConversionSettings { Format = VideoFormat.MP4 });

await batchService.AddTasksAsync(batch.Id, new[]
{
    new DownloadTask { Url = "https://coub.com/view/abc123" },
    new DownloadTask { Url = "https://coub.com/view/def456" },
});

var progress = new Progress<BatchProgress>(p => Console.WriteLine(p.StatusMessage));
var result = await batchService.StartBatchAsync(batch.Id, progress);

if (result.State == ProcessingState.Completed)
    Console.WriteLine($"Done: {result.CompletedTasks} succeeded, {result.FailedTasks} failed");
```

## Notes

- **Lifecycle order matters.** Tasks can only be added while the batch is
  `Pending`, and a batch can only be started when `CanStart()` is true
  (pending and has at least one task). Deleting is blocked while the batch is
  actively processing.
- **Parallelism** is capped by `BatchJob.MaxParallelTasks` (range `1..10`,
  default `2`) via a `SemaphoreSlim`. Tasks are processed concurrently, so
  shared state on `BatchJob` (e.g. `CompletedTasks`, `FailedTasks`) is mutated
  from multiple tasks — the service relies on the repository implementations
  for persistence rather than in-memory thread safety.
- **Failure policy.** When `ContinueOnError` is `true` (the default), a failed
  task is recorded and the batch continues; otherwise the first failure throws
  `BatchProcessingException` and aborts the batch.
- **Cancellation.** Passing a cancelled `CancellationToken` to `StartBatchAsync`
  marks in-flight tasks as `Cancelled` and rethrows
  `OperationCanceledException`.
- **Output naming.** Each task's `OutputPath` is derived from the URL's file
  name (without extension) plus `.mp4` under the batch `OutputDirectory`.