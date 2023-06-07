// entire file content ...
// ... goes in between

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

# FFmpegWrapper

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

// ... rest of code ...
