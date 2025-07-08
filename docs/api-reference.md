# API Reference

## Services

### ICoubDownloadService

Main service for downloading Coub videos.

#### DownloadAsync

```csharp
Task<DownloadResult> DownloadAsync(
    string url,
    ConversionSettings? settings = null,
    CancellationToken cancellationToken = default)
```

Downloads a single Coub video with optional conversion settings.

**Parameters:**
- `url` (string): Coub video URL (e.g., `https://coub.com/view/2a3b4c5d`)
- `settings` (ConversionSettings, optional): Custom conversion settings
- `cancellationToken` (CancellationToken, optional): Cancellation token

**Returns:** `DownloadResult` with download information

**Throws:** `CoubDownloaderException` on failure

**Example:**
```csharp
var result = await downloadService.DownloadAsync("https://coub.com/view/2a3b4c5d");
Console.WriteLine($"Downloaded: {result.OutputPath}");
```

#### DownloadMultipleAsync

```csharp
Task<List<DownloadResult>> DownloadMultipleAsync(
    IEnumerable<string> urls,
    int maxConcurrency = 4,
    CancellationToken cancellationToken = default)
```

Downloads multiple videos concurrently.

**Parameters:**
- `urls` (IEnumerable<string>): List of Coub URLs
- `maxConcurrency` (int): Maximum concurrent downloads (default: 4)
- `cancellationToken` (CancellationToken, optional): Cancellation token

**Returns:** List of `DownloadResult`

**Example:**
```csharp
var urls = new[] {
    "https://coub.com/view/2a3b4c5d",
    "https://coub.com/view/3b4c5d6e"
};
var results = await downloadService.DownloadMultipleAsync(urls, maxConcurrency: 8);
```

#### GetVideoMetadataAsync

```csharp
Task<CoubVideo> GetVideoMetadataAsync(string url)
```

Fetches video metadata without downloading.

**Parameters:**
- `url` (string): Coub video URL

**Returns:** `CoubVideo` with metadata

**Example:**
```csharp
var video = await downloadService.GetVideoMetadataAsync("https://coub.com/view/2a3b4c5d");
Console.WriteLine($"Title: {video.Title}");
Console.WriteLine($"Duration: {video.Duration}s");
```

---

### IVideoConversionService

Handles video encoding and format conversion using FFmpeg.

#### ConvertVideoAsync

```csharp
Task<string> ConvertVideoAsync(
    string inputPath,
    string outputPath,
    ConversionSettings settings,
    CancellationToken cancellationToken = default)
```

Converts video to specified format and quality.

**Parameters:**
- `inputPath` (string): Path to input video file
- `outputPath` (string): Path for output video file
- `settings` (ConversionSettings): Conversion configuration
- `cancellationToken` (CancellationToken, optional): Cancellation token

**Returns:** Output file path

**Throws:** `CoubDownloaderException` if conversion fails

**Example:**
```csharp
var settings = new ConversionSettings
{
    Format = VideoFormat.MP4,
    Quality = VideoQuality.High,
    Width = 1920,
    Height = 1080
};

var output = await conversionService.ConvertVideoAsync(
    "/tmp/input.webm",
    "/downloads/output.mp4",
    settings
);
```

#### IsFfmpegAvailableAsync

```csharp
Task<bool> IsFfmpegAvailableAsync()
```

Checks if FFmpeg is installed and available.

**Returns:** `true` if FFmpeg is available

**Example:**
```csharp
if (await conversionService.IsFfmpegAvailableAsync())
{
    // FFmpeg is available
}
```

#### GetFfmpegVersionAsync

```csharp
Task<string> GetFfmpegVersionAsync()
```

Retrieves FFmpeg version information.

**Returns:** Version string (e.g., "6.0-full_build-www.gyan.dev")

**Example:**
```csharp
var version = await conversionService.GetFfmpegVersionAsync();
Console.WriteLine($"FFmpeg version: {version}");
```

---

### IAudioProcessingService

Handles audio looping and synchronization.

#### ProcessAudioAsync

```csharp
Task<AudioTrack> ProcessAudioAsync(
    AudioTrack track,
    string videoFilePath,
    double targetDuration)
```

Processes audio to match video duration through looping.

**Parameters:**
- `track` (AudioTrack): Audio track information
- `videoFilePath` (string): Path to video file
- `targetDuration` (double): Target duration in seconds

**Returns:** Modified `AudioTrack` with looping info

**Example:**
```csharp
var audioTrack = new AudioTrack
{
    Duration = 8.5,
    LoopStrategy = AudioLoopStrategy.Repeat
};

var processed = await audioService.ProcessAudioAsync(
    audioTrack,
    "/tmp/video.mp4",
    15.5
);
```

#### SyncAudioWithVideoAsync

```csharp
Task SyncAudioWithVideoAsync(
    string audioPath,
    string videoPath,
    string outputPath)
```

Synchronizes and merges audio with video.

**Parameters:**
- `audioPath` (string): Path to audio file
- `videoPath` (string): Path to video file
- `outputPath` (string): Path for output video with audio

**Throws:** `CoubDownloaderException` on sync failure

**Example:**
```csharp
await audioService.SyncAudioWithVideoAsync(
    "/tmp/audio.aac",
    "/tmp/video.mp4",
    "/downloads/final.mp4"
);
```

---

### IBatchProcessingService

Manages batch processing of multiple videos.

#### CreateBatchJobAsync

```csharp
Task<BatchJob> CreateBatchJobAsync(
    string name,
    string outputDirectory,
    ConversionSettings settings)
```

Creates a new batch job.

**Parameters:**
- `name` (string): Batch job name
- `outputDirectory` (string): Directory for output files
- `settings` (ConversionSettings): Default conversion settings

**Returns:** Created `BatchJob`

**Example:**
```csharp
var batch = await batchService.CreateBatchJobAsync(
    "My Videos",
    "/downloads/batch",
    new ConversionSettings { Quality = VideoQuality.High }
);
```

#### AddTasksAsync

```csharp
Task AddTasksAsync(string batchId, IEnumerable<DownloadTask> tasks)
```

Adds download tasks to a batch.

**Parameters:**
- `batchId` (string): Batch job ID
- `tasks` (IEnumerable<DownloadTask>): Tasks to add

**Example:**
```csharp
var tasks = new[] {
    new DownloadTask { Url = "https://coub.com/view/2a3b4c5d" },
    new DownloadTask { Url = "https://coub.com/view/3b4c5d6e" }
};

await batchService.AddTasksAsync(batch.Id, tasks);
```

#### GetBatchStatusAsync

```csharp
Task<BatchJob> GetBatchStatusAsync(string batchId)
```

Retrieves current batch status.

**Parameters:**
- `batchId` (string): Batch job ID

**Returns:** `BatchJob` with current status

**Example:**
```csharp
var batch = await batchService.GetBatchStatusAsync(batchId);
Console.WriteLine($"Progress: {batch.GetProgressPercent()}%");
```

#### StartBatchAsync

```csharp
Task StartBatchAsync(string batchId)
```

Starts processing a batch job.

**Parameters:**
- `batchId` (string): Batch job ID

**Example:**
```csharp
await batchService.StartBatchAsync(batch.Id);
```

---

## Models

### ConversionSettings

```csharp
public class ConversionSettings
{
    public string Id { get; set; }
    public VideoFormat Format { get; set; } = VideoFormat.MP4;
    public VideoQuality Quality { get; set; } = VideoQuality.High;
    public int Width { get; set; }
    public int Height { get; set; }
    public int FrameRate { get; set; } = 30;
    public int VideoBitrate { get; set; }  // kbps
    public int AudioBitrate { get; set; } = 128;  // kbps
    public string VideoCodec { get; set; } = "libx264";
    public string AudioCodec { get; set; } = "aac";
    public bool PreserveAspectRatio { get; set; } = true;
    public bool EnableHardwareAcceleration { get; set; }
}
```

### CoubVideo

```csharp
public class CoubVideo
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public double Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? SourceUrl { get; set; }
    public string? CreatorName { get; set; }
    public long ViewCount { get; set; }
    public bool HasAudio { get; set; }
    public AudioTrack? AudioTrack { get; set; }
    public List<VideoSection> Sections { get; set; }
}
```

### DownloadResult

```csharp
public class DownloadResult
{
    public string Id { get; set; }
    public string OutputPath { get; set; }
    public long FileSizeBytes { get; set; }
    public double Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Codec { get; set; }
    public DownloadState State { get; set; }
    public DateTime CompletedAt { get; set; }
}
```

### AudioTrack

```csharp
public class AudioTrack
{
    public string Id { get; set; }
    public string VideoId { get; set; }
    public double Duration { get; set; }
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public int Bitrate { get; set; }
    public string Codec { get; set; }
    public AudioLoopStrategy LoopStrategy { get; set; }
    public int LoopCount { get; set; }
    public double VolumeLevel { get; set; } = 1.0;
}
```

---

## Enums

### VideoFormat

```csharp
public enum VideoFormat
{
    MP4,
    WebM,
    Shorts,
    MOV,
    AVI
}
```

### VideoQuality

```csharp
public enum VideoQuality
{
    Low,           // 240p
    Medium,        // 480p
    High,          // 720p
    HighDefinition,// 1080p
    UltraHigh,     // 1440p
    FourK          // 2160p
}
```

### ProcessingState

```csharp
public enum ProcessingState
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled,
    Queued
}
```

### AudioLoopStrategy

```csharp
public enum AudioLoopStrategy
{
    Repeat,        // Simple concatenation
    Fade,          // Crossfade between loops
    Silent         // Pad with silence
}
```

---

## Exceptions

### CoubDownloaderException

Base exception for all Coub Downloader errors.

```csharp
public class CoubDownloaderException : Exception
{
    public string? ErrorCode { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}
```

---

## Extension Methods

### CoubVideo Extensions

```csharp
// Get aspect ratio
decimal aspectRatio = video.GetAspectRatio();

// Check if vertical format (suitable for Shorts)
bool isVertical = video.IsVerticalFormat();

// Validate video
bool isValid = video.IsValid();

// Calculate required audio duration
double audioDuration = video.CalculateRequiredAudioDuration();
```

### AudioTrack Extensions

```csharp
// Get audio specification string
string spec = audioTrack.GetAudioSpec();

// Calculate looped duration
double loopedDuration = audioTrack.CalculateLoopedDuration();
```

### ConversionSettings Extensions

```csharp
// Get FFmpeg codec parameters
string params = settings.GetFFmpegCodecParams();

// Apply quality preset
settings.ApplyQualityPreset();
```
