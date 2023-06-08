## WebhookManager

`WebhookManager` is a class responsible for managing webhook subscriptions and sending events to registered webhooks. It provides methods for subscribing to webhooks, unsubscribing, sending events, disabling subscriptions, and retrieving active subscriptions.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Integration;

// Create a new WebhookManager instance
var webhookManager = new WebhookManager(new HttpClient(), new LoggingService());

// Subscribe to a webhook
webhookManager.Subscribe("https://example.com/webhook", WebhookEventType.VideoDownloadStarted);

// Send an event to all subscribers
await webhookManager.SendEventAsync(WebhookEventType.VideoDownloadStarted, new { VideoId = "abc123" });

// Unsubscribe from a webhook
webhookManager.Unsubscribe("subscription-id");

// Disable a subscription
webhookManager.DisableSubscription("subscription-id");

// Get all active subscriptions
var subscriptions = webhookManager.GetSubscriptions();
```

## FFmpegWrapper

`FFmpegWrapper` is a utility class that provides a clean, asynchronous interface for interacting with FFmpeg and FFprobe command-line tools. It handles video conversion, audio extraction, video concatenation, media information retrieval, and other common media processing tasks while managing process execution, timeouts, and progress reporting.

The wrapper supports hardware acceleration, customizable video/audio codecs and bitrates, and provides detailed media information through FFprobe integration.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Middleware;

// Create FFmpeg wrapper instance
var logger = new MemoryLoggingService(); // or any ILoggingService implementation
var ffmpeg = new FFmpegWrapper("ffmpeg", "ffprobe", logger);

// Check if FFmpeg is available
bool isAvailable = await ffmpeg.IsAvailableAsync();
Console.WriteLine($"FFmpeg available: {isAvailable}");

// Get FFmpeg version
string version = await ffmpeg.GetVersionAsync();
Console.WriteLine($"FFmpeg version: {version}");

// Get media information
var mediaInfo = await ffmpeg.GetMediaInfoAsync("input.mp4");
if (mediaInfo != null)
{
    Console.WriteLine($"Duration: {mediaInfo.DurationInSeconds}s");
    Console.WriteLine($"Size: {mediaInfo.Size} bytes");
    Console.WriteLine($"Bitrate: {mediaInfo.BitRate} bps");
}

// Convert video with custom parameters
var conversionParams = new ConversionParameters
{
    VideoCodec = "libx264",
    AudioCodec = "aac",
    VideoBitrate = 5000,
    AudioBitrate = 192,
    FrameRate = 30,
    Width = 1280,
    Height = 720,
    UseHardwareAcceleration = true
};

var result = await ffmpeg.ConvertVideoAsync(
    "input.mp4",
    "output.mp4",
    conversionParams,
    new Progress<int>(percent => Console.WriteLine($"Conversion progress: {percent}%"))
);

if (result.Success)
{
    Console.WriteLine("Conversion completed successfully!");
}
else
{
    Console.WriteLine($"Conversion failed: {result.Error}");
}

// Extract audio from video
var audioResult = await ffmpeg.ExtractAudioAsync("video.mp4", "audio.mp3");

// Concatenate multiple videos
var concatResult = await ffmpeg.ConcatenateVideosAsync(
    new List<string> { "part1.mp4", "part2.mp4" },
    "combined.mp4"
);

// Loop audio to match video duration
var loopResult = await ffmpeg.LoopAudioAsync(
    "background_music.mp3",
    120.5, // target duration in seconds
    "looped_audio.mp3"
);
```

## ICoubApiClient

`ICoubApiClient` is an interface for interacting with the Coub.com API to retrieve video metadata, verify video existence, and search for videos. It abstracts HTTP requests and caching logic to provide a consistent interface for video information retrieval.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Caching;
using CoubDownloader.Infrastructure.Middleware;

// Create an ICoubApiClient instance with dependencies
var httpClient = new HttpClient();
var logger = new MemoryLoggingService();
var cacheService = new MemoryCacheService();
var coubApiClient = new CoubApiClient(httpClient, logger, cacheService);

// Get video info
var videoInfo = await coubApiClient.GetVideoInfoAsync("https://coub.com/view/abc123");
if (videoInfo != null)
{
    Console.WriteLine($"Video ID: {videoInfo.Id}");
    Console.WriteLine($"Title: {videoInfo.Title}");
    Console.WriteLine($"Views: {videoInfo.ViewCount}");
    Console.WriteLine($"Duration: {videoInfo.Duration}s");
    Console.WriteLine($"Has Audio: {videoInfo.HasAudio}");
}

// Verify video exists
bool exists = await coubApiClient.VerifyVideoExistsAsync("https://coub.com/view/xyz456");
Console.WriteLine($"Video exists: {exists}");

// Search videos
var searchResults = await coubApiClient.SearchVideosAsync("funny cats", limit: 5);
foreach (var video in searchResults)
{
    Console.WriteLine($"Found: {video.Title} (Views: {video.ViewCount})");
}
```

## BackgroundWorker

`BackgroundWorker` is an abstract base class for long-running background tasks that can be started and stopped gracefully. It provides periodic execution capabilities through derived classes like `CleanupWorker` (for file cleanup) and `MonitoringWorker` (for system health checks).

### Usage Example

```csharp
using CoubDownloader.Infrastructure.BackgroundJobs;
using CoubDownloader.Infrastructure.Middleware;

// Create and start a cleanup worker
var cleanupWorker = new CleanupWorker(downloadDirectory: "/path/to/downloads", retentionDays: 7);
cleanupWorker.Start();

// Create and start a monitoring worker
var monitoringWorker = new MonitoringWorker();
monitoringWorker.HealthCheckCompleted += (sender, result) =>
{
    Console.WriteLine($"Health Check at {result.Timestamp}");
    Console.WriteLine($"Memory: {result.AvailableMemory} bytes");
    Console.WriteLine($"CPU: {result.ProcessorCount} cores");
    Console.WriteLine($"Disk: {result.AvailableDiskSpace} bytes free");
};
monitoringWorker.Start();

// Stop both workers after some time
await Task.Delay(TimeSpan.FromMinutes(1));
await cleanupWorker.StopAsync();
await monitoringWorker.StopAsync();
```
