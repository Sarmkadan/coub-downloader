# DownloadTask

Represents a single download operation within the coub-downloader system. It tracks the lifecycle of a download from creation through completion or failure, including metadata such as the source URL, output path, current processing state, progress, and retry information. Instances are typically created by the downloader engine and updated as the download progresses.

## API

- **`Id`** – Gets the unique identifier for this download task.  
  Type: `string`  
  Never throws.

- **`VideoId`** – Gets the identifier of the Coub video being downloaded.  
  Type: `string`  
  Never throws.

- **`Url`** – Gets the source URL from which the video is downloaded.  
  Type: `string`  
  Never throws.

- **`OutputPath`** – Gets or sets the file system path where the downloaded file will be saved.  
  Type: `string`  
  Never throws.

- **`State`** – Gets the current processing state of the download (e.g., Queued, Downloading, Completed, Failed).  
  Type: `ProcessingState` (enum)  
  Never throws.

- **`Format`** – Gets the video format requested for this download (e.g., MP4, WebM).  
  Type: `VideoFormat` (enum)  
  Never throws.

- **`Quality`** – Gets the video quality requested (e.g., Low, Medium, High).  
  Type: `VideoQuality` (enum)  
  Never throws.

- **`AudioLoopStrategy`** – Gets the strategy used for handling audio looping (e.g., None, Loop, Extend).  
  Type: `AudioLoopStrategy` (enum)  
  Never throws.

- **`ProgressPercent`** – Gets the download progress as an integer between 0 and 100.  
  Type: `int`  
  Never throws.

- **`FileSizeBytes`** – Gets the total file size in bytes, if known; otherwise 0.  
  Type: `long`  
  Never throws.

- **`StartedAt`** – Gets the UTC timestamp when the download started, or `null` if not yet started.  
  Type: `DateTime?`  
  Never throws.

- **`CompletedAt`** – Gets the UTC timestamp when the download completed (successfully or with error), or `null` if not yet completed.  
  Type: `DateTime?`  
  Never throws.

- **`ErrorMessage`** – Gets the error message if the download failed, or `null` if no error occurred.  
  Type: `string?`  
  Never throws.

- **`RetryCount`** – Gets the number of times this download has been retried.  
  Type: `int`  
  Never throws.

- **`MaxRetries`** – Gets the maximum number of retries allowed for this download.  
  Type: `int`  
  Never throws.

- **`CreatedAt`** – Gets the UTC timestamp when the task was created.  
  Type: `DateTime`  
  Never throws.

- **`UpdatedAt`** – Gets the UTC timestamp of the last update to this task.  
  Type: `DateTime`  
  Never throws.

- **`BatchJobId`** – Gets the identifier of the batch job that this task belongs to, or `null` if it is a standalone download.  
  Type: `string?`  
  Never throws.

- **`GetElapsedTime`** – Gets the elapsed time since the download started, or `null` if the download has not started.  
  Type: `TimeSpan?`  
  Never throws.

- **`IsRunning`** – Gets a value indicating whether the download is currently in progress (i.e., `State` is `ProcessingState.Downloading`).  
  Type: `bool`  
  Never throws.

## Usage

### Example 1: Monitoring progress of a single download

```csharp
var task = new DownloadTask
{
    Id = Guid.NewGuid().ToString(),
    VideoId = "abc123",
    Url = "https://coub.com/view/abc123",
    OutputPath = @".\downloads\abc123.mp4",
    Format = VideoFormat.Mp4,
    Quality = VideoQuality.High,
    AudioLoop = AudioLoopStrategy.None,
    MaxRetries = 3
};

// Simulate progress updates (in a real scenario these would be set by the engine)
task.State = ProcessingState.Downloading;
task.StartedAt = DateTime.UtcNow;

while (task.IsRunning)
{
    Console.WriteLine($"Progress: {task.ProgressPercent}%");
    System.Threading.Thread.Sleep(500);
}

if (task.State == ProcessingState.Completed)
{
    Console.WriteLine($"Download completed in {task.GetElapsedTime?.TotalSeconds:F1}s");
}
else
{
    Console.WriteLine($"Failed: {task.ErrorMessage}");
}
```

### Example 2: Handling retries and batch context

```csharp
// Assume tasks are collected from a batch job
var batchTasks = new List<DownloadTask>();

foreach (var task in batchTasks)
{
    if (task.State == ProcessingState.Failed && task.RetryCount < task.MaxRetries)
    {
        Console.WriteLine($"Retrying task {task.Id} (attempt {task.RetryCount + 1}/{task.MaxRetries})");
        // Engine would reset state and increment RetryCount
    }
    else if (task.State == ProcessingState.Completed)
    {
        Console.WriteLine($"Task {task.Id} saved to {task.OutputPath} ({task.FileSizeBytes} bytes)");
    }
}
```

## Notes

- **Edge cases:**  
  - `ProgressPercent` may remain at 0 if the file size is unknown (`FileSizeBytes == 0`).  
  - `StartedAt` and `CompletedAt` are `null` until the respective lifecycle events occur.  
  - `ErrorMessage` is `null` for successful downloads and for downloads that have not yet failed.  
  - `RetryCount` is incremented only when a retry is triggered; it never exceeds `MaxRetries`.  
  - `GetElapsedTime` returns `null` if `StartedAt` is `null`; it is computed from `StartedAt` and the current UTC time when accessed.

- **Thread safety:**  
  Instances of `DownloadTask` are not inherently thread-safe. Properties are typically updated by a single background thread (the download engine) and read from one or more UI or monitoring threads. To avoid torn reads or stale values, consider synchronizing access (e.g., using a lock or reading all properties in a single snapshot) when the task is accessed concurrently. The `IsRunning` property is a simple boolean check and is safe to poll, but the underlying `State` may change between reads.
