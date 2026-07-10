# DownloadOptions

`DownloadOptions` is a plain data transfer object that holds configuration values controlling the behavior of the coub-downloader library, such as where files are stored, how retries and timeouts are handled, caching policies, parallelism, and default encoding parameters passed to FFmpeg.

## API

| Member | Type | Purpose | Remarks / Validation |
|--------|------|---------|----------------------|
| `OutputPath` | `string` | Directory where downloaded files will be written. If the path does not exist, the library attempts to create it. | Setting to `null`, empty, or whitespace throws `ArgumentException`. |
| `CachePath` | `string` | Directory used for temporary cache files when `EnableCaching` is true. | Same validation rules as `OutputPath`. |
| `MaxRetries` | `int` | Maximum number of retry attempts for a failed download before giving up. | Must be ≥ 0; negative values throw `ArgumentOutOfRangeException`. |
| `TimeoutSeconds` | `int` | Number of seconds after which a download operation is aborted. Appears twice in the source; both refer to the same setting. | Must be > 0; zero or negative throws `ArgumentOutOfRangeException`. |
| `EnableCaching` | `bool` | When true, enables caching of intermediate files to avoid re‑downloading unchanged segments. | No validation; default is `false`. |
| `MaxCacheSizeGb` | `double` | Upper limit (in gigabytes) for the cache directory size. When exceeded, the oldest cache entries are purged. | Must be ≥ 0; negative throws `ArgumentOutOfRangeException`. |
| `ParallelDownloads` | `int` | Number of simultaneous download streams allowed. | Must be ≥ 1; values < 1 throw `ArgumentOutOfRangeException`. |
| `DefaultQuality` | `string` | Preferred video quality (e.g., `"1080p"`, `"720p"`). Used when no quality is explicitly specified for a download. | Must be a non‑empty string; empty or `null` throws `ArgumentException`. |
| `DefaultFormat` | `string` | Desired container format (e.g., `"mp4"`, `"webm"`). | Must be a non‑empty string; empty or `null` throws `ArgumentException`. |
| `DefaultFrameRate` | `int` | Frame rate (frames per second) to request from FFmpeg if not overridden. | Must be > 0; ≤ 0 throws `ArgumentOutOfRangeException`. |
| `EnableHardwareAcceleration` | `bool` | When true, instructs FFmpeg to use hardware‑accelerated encoding/decoding where available. | No validation; default is `false`. |
| `FFmpegPath` | `string` | Filesystem path to the `ffmpeg` executable. If empty, the library searches the system `PATH`. | Must be a valid existing file when non‑empty; otherwise throws `FileNotFoundException`. |
| `FFprobePath` | `string` | Filesystem path to the `ffprobe` executable. Same lookup rules as `FFmpegPath`. | Same validation as `FFmpegPath`. |
| `ThreadCount` | `int` | Number of worker threads used for post‑processing tasks (e.g., transcoding). | Must be ≥ 1; < 1 throws `ArgumentOutOfRangeException`. |
| `DefaultLoopStrategy` | `string` | Strategy for handling looping coubs (e.g., `"none"`, `"pingpong"`, `"repeat"`). Passed to FFmpeg filter chains. | Must be a non‑empty string; empty or `null` throws `ArgumentException`. |
| `DefaultSampleRate` | `int` | Audio sample rate in Hertz to request from FFmpeg. | Must be a standard rate (e.g., 44100, 48000); invalid values throw `ArgumentOutOfRangeException`. |
| `DefaultChannels` | `int` | Number of audio channels (e.g., 1 for mono, 2 for stereo). | Must be ≥ 1; < 1 throws `ArgumentOutOfRangeException`. |
| `DefaultBitrate` | `int` | Target audio/video bitrate in kilobits per second. | Must be > 0; ≤ 0 throws `ArgumentOutOfRangeException`. |
| `CoubBaseUrl` | `string` | Base URL of the Coub service (defaults to `"https://coub.com"`). Allows pointing to a mirror or test instance. | Must be a valid absolute URI; malformed strings throw `UriFormatException`. |

## Usage

```csharp
using CoubDownloader;

// Create and configure options
var opts = new DownloadOptions
{
    OutputPath = @"C:\Media\Coubs",
    CachePath  = @"C:\Media\CoubCache",
    EnableCaching = true,
    MaxCacheSizeGb = 5.0,
    ParallelDownloads = 4,
    TimeoutSeconds = 30,
    MaxRetries = 3,
    DefaultQuality = "720p",
    DefaultFormat = "mp4",
    EnableHardwareAcceleration = true,
    FFmpegPath = @"C:\Tools\ffmpeg.exe",
    FFprobePath = @"C:\Tools\ffprobe.exe"
};

// Pass the options to the downloader
var downloader = new CoubDownloader(opts);
await downloader.DownloadAsync("https://coub.com/view/123abc");
```

```csharp
using CoubDownloader;

// Minimal configuration – rely on defaults and system PATH for FFmpeg
var opts = new DownloadOptions
{
    OutputPath = "./downloads",
    DefaultFormat = "webm",
    DefaultFrameRate = 60
};

var downloader = new CoubDownloader(opts);
// The downloader will use internal defaults for unspecified fields.
```

## Notes

- Setting any path‑related property to an invalid or inaccessible location will cause the constructor or subsequent download operation to throw; the library does **not** attempt to create parent directories beyond the final path component.
- `TimeoutSeconds` appears twice in the source definition; both refer to the same backing field, so assigning to one updates the other.
- The class is immutable after construction only if the consumer treats it as such; the properties have public setters, so concurrent modification from multiple threads without external synchronization can lead to race conditions. For thread‑safe usage, either create a separate instance per thread or synchronize access to a shared instance.
- Numeric limits (e.g., `MaxRetries`, `ParallelDownloads`) are validated only when the property is assigned; changing a value after a download has started does not affect the ongoing operation.
- When `EnableCaching` is `false`, `CachePath` and `MaxCacheSizeGb` are ignored, but they must still be set to valid values if the consumer chooses to assign them.
