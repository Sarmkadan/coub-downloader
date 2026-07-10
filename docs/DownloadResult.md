# DownloadResult

Represents the result of a video download operation, containing success status, output metadata, processing metrics, and any errors or warnings encountered during processing.

## API

### `public string Id`
Unique identifier for the download task. Used to correlate results with originating requests.

### `public string TaskId`
Identifier of the original task that produced this result. May differ from `Id` when the operation is retried or split.

### `public bool Success`
Indicates whether the download and processing completed without critical errors. `true` when `ErrorMessage` is `null`; `false` otherwise.

### `public string? OutputFilePath`
Absolute filesystem path to the generated output file, if successful. `null` when `Success` is `false` or no file was produced.

### `public long OutputFileSizeBytes`
Size of the output file in bytes. Valid only when `Success` is `true` and `OutputFilePath` is not `null`.

### `public long ProcessingTimeMs`
Total processing duration in milliseconds, including download and post-processing steps.

### `public VideoFormat Format`
Container format of the output video (e.g., MP4, WebM). Undefined if `Success` is `false`.

### `public VideoQuality Quality`
Target quality preset used for encoding (e.g., SD, HD, UHD). Undefined if `Success` is `false`.

### `public string? ErrorMessage`
Human-readable error description, if the operation failed. `null` on success.

### `public string? ErrorStackTrace`
Full stack trace of the exception that caused failure, if available. `null` on success or when not captured.

### `public string? ErrorType`
Qualified type name of the exception that caused failure (e.g., `System.IO.IOException`). `null` on success.

### `public string? VideoMetadata`
JSON-serialized metadata about the source video (resolution, duration, codec, etc.), if available. `null` when not captured.

### `public string? AudioSyncInfo`
JSON-serialized synchronization diagnostics for audio/video streams, if processed. `null` when not applicable.

### `public List<string> Warnings`
Collection of non-fatal issues encountered during processing (e.g., network timeouts, codec fallbacks). Empty on success.

### `public DateTime CompletedAt`
Timestamp marking when the operation finished, regardless of success or failure.

### `public string GetStatusMessage()`
Returns a localized human-readable status summary combining `Success`, `ErrorMessage`, and `Warnings`. No parameters. Throws `InvalidOperationException` if `CompletedAt` is not set (i.e., operation never started).

### `public void AddWarning(string warning)`
Appends a warning message to the `Warnings` collection. Argument must not be `null` or whitespace; throws `ArgumentException` otherwise.

## Usage

```csharp
// Example 1: Successful download with warnings
var result = new DownloadResult
{
    Id = "dl_abc123",
    TaskId = "task_xyz789",
    Success = true,
    OutputFilePath = "/data/videos/dl_abc123.mp4",
    OutputFileSizeBytes = 42_949_672,
    ProcessingTimeMs = 2145,
    Format = VideoFormat.Mp4,
    Quality = VideoQuality.Hd,
    VideoMetadata = "{\"resolution\":\"1920x1080\",\"duration\":12.45}",
    Warnings = new List<string> { "Network latency > 500ms detected" },
    CompletedAt = DateTime.UtcNow
};

Console.WriteLine(result.GetStatusMessage());
// Output: "Download completed with warnings: Network latency > 500ms detected"
```

```csharp
// Example 2: Failed download with error details
var result = new DownloadResult
{
    Id = "dl_def456",
    TaskId = "task_uvw321",
    Success = false,
    ErrorMessage = "Remote server returned 404",
    ErrorType = "System.Net.Http.HttpRequestException",
    ErrorStackTrace = "at CoubDownloader.HttpClient.DownloadAsync(...)",
    CompletedAt = DateTime.UtcNow.AddSeconds(-10)
};

Console.WriteLine(result.GetStatusMessage());
// Output: "Download failed: Remote server returned 404 (System.Net.Http.HttpRequestException)"
```

## Notes

- Thread safety: All public members are read-only except `Warnings` and `AddWarning`. `Warnings` is a `List<string>` and must be externally synchronized if accessed concurrently. `AddWarning` performs argument validation but is not atomic; wrap calls in a lock if used from multiple threads.
- `GetStatusMessage` throws if `CompletedAt` is unset, which can occur if the result is constructed but the operation never started.
- `OutputFileSizeBytes` and `Format`/`Quality` are undefined on failure; consumers should check `Success` before using them.
- `ErrorMessage` and related error fields are mutually exclusive with success; only one set will contain meaningful data in any given instance.
