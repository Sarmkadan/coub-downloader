# EventHandlingExample

The `EventHandlingExample` type serves as a demonstration and utility class for handling download progress, errors, and lifecycle events in the `coub-downloader` project. It tracks metadata about video downloads, including URLs, file paths, progress states, and error conditions, while exposing properties that can be observed or logged during asynchronous download operations.

## API

### `public static async Task Main`
The entry point for executing a download operation. This method orchestrates the download process, invoking other members to track progress, handle errors, and manage retries. It does not accept parameters and returns `Task` to support asynchronous execution.

### `public string VideoUrl`
The URL of the video being downloaded. This property is read-only during the download process and is set at initialization. Throws `ArgumentNullException` if assigned `null` or empty.

### `public string VideoTitle`
The title or identifier of the video, derived from the `VideoUrl` or metadata. Used for logging and output file naming. Throws `ArgumentNullException` if assigned `null` or empty.

### `public DateTime Timestamp`
Records the time at which a specific event (e.g., progress update, error, or retry) occurred. Updated automatically during operations. Throws `ArgumentOutOfRangeException` if assigned a future date.

### `public int ProgressPercent`
The current download progress as a percentage (0–100). Updated during the download process. Throws `ArgumentOutOfRangeException` if assigned a value outside the valid range.

### `public long DownloadedBytes`
The number of bytes downloaded so far. Updated incrementally during the download process. Throws `ArgumentOutOfRangeException` if assigned a negative value.

### `public long TotalBytes`
The total size of the video file in bytes, as reported by the server. May be `0` if the server does not provide this information. Throws `ArgumentOutOfRangeException` if assigned a negative value.

### `public string OutputPath`
The filesystem path where the downloaded video will be saved. Validated for existence and write permissions during initialization. Throws `ArgumentNullException` if assigned `null` or empty, and `IOException` if the path is invalid or inaccessible.

### `public long FileSizeBytes`
The size of the downloaded file on disk, in bytes. Updated after the download completes. Throws `ArgumentOutOfRangeException` if assigned a negative value.

### `public double Duration`
The duration of the video in seconds, derived from metadata. May be `0` if metadata is unavailable. Throws `ArgumentOutOfRangeException` if assigned a negative value.

### `public string Error`
A description of the last error encountered during the download process. Empty if no error has occurred. Throws `ArgumentNullException` if assigned `null`.

### `public int RetryAttempt`
The current attempt number for retrying a failed download (1-based). Reset to `0` on success. Throws `ArgumentOutOfRangeException` if assigned a value less than `0`.

### `public string InputPath`
The source path or identifier used to initiate the download (e.g., a URL or local file path). Throws `ArgumentNullException` if assigned `null` or empty.

### `public VideoQuality Quality`
The selected video quality for the download (e.g., `High`, `Medium`, `Low`). Throws `ArgumentNullException` if assigned `null`.

### `public long DurationMs`
The duration of the video in milliseconds. Updated after metadata is parsed. Throws `ArgumentOutOfRangeException` if assigned a negative value.

## Usage

### Example 1: Basic Download with Progress Tracking
