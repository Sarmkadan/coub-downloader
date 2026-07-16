// README.md
## VideoConversionServiceTests

The `VideoConversionServiceTests` class provides a comprehensive suite of xUnit tests that verify the behavior of the `VideoConversionService` class. It tests all major video conversion operations including converting videos with custom settings, extracting metadata from video files, applying audio tracks to videos, rescaling videos to different dimensions, and converting videos to Shorts format (9:16 aspect ratio with centered content).

### Usage Example

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

public class VideoConversionDemo
{
    public async Task RunAll()
    {
        // Create the service instance
        var videoService = new VideoConversionService();

        // Define paths
        var inputPath = Path.Combine(Path.GetTempPath(), "input.mp4");
        var outputPath = Path.Combine(Path.GetTempPath(), "output.mp4");
        var audioPath = Path.Combine(Path.GetTempPath(), "audio.mp3");
        var shortsOutputPath = Path.Combine(Path.GetTempPath(), "shorts.mp4");

        // Create dummy files for demonstration
        File.WriteAllText(inputPath, "dummy video content");
        File.WriteAllText(audioPath, "dummy audio content");

        // ConvertVideoAsync - Convert video with custom settings
        var settings = new ConversionSettings
        {
            Width = 1280,
            Height = 720,
            FrameRate = 30,
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            VideoBitrate = 2500,
            AudioBitrate = 128
        };

        var convertedPath = await videoService.ConvertVideoAsync(inputPath, outputPath, settings);
        Console.WriteLine($"Converted video to: {convertedPath}");

        // GetVideoMetadataAsync - Extract metadata from video file
        var metadata = await videoService.GetVideoMetadataAsync(outputPath);
        Console.WriteLine($"Video metadata: {metadata.Width}x{metadata.Height}, {metadata.Duration}s, {metadata.VideoCodec}");

        // ApplyAudioTrackAsync - Replace or add audio track to video
        var audioOutputPath = Path.Combine(Path.GetTempPath(), "output_with_audio.mp4");
        var audioAppliedPath = await videoService.ApplyAudioTrackAsync(outputPath, audioPath, audioOutputPath, settings);
        Console.WriteLine($"Video with audio: {audioAppliedPath}");

        // RescaleVideoAsync - Resize video to specific dimensions
        var rescaledPath = Path.Combine(Path.GetTempPath(), "rescaled.mp4");
        var rescaledPathResult = await videoService.RescaleVideoAsync(outputPath, rescaledPath, 640, 480);
        Console.WriteLine($"Rescaled video to: {rescaledPathResult}");

        // ConvertToShortsAsync - Convert video to Shorts format (9:16)
        var shortsPath = Path.Combine(Path.GetTempPath(), "shorts_format.mp4");
        var shortsResult = await videoService.ConvertToShortsAsync(outputPath, shortsPath);
        Console.WriteLine($"Shorts format video: {shortsResult}");

        // Cleanup
        File.Delete(inputPath);
        File.Delete(audioPath);
        File.Delete(outputPath);
        File.Delete(audioOutputPath);
        File.Delete(rescaledPath);
        File.Delete(shortsPath);
    }
}
```

## CoubApiClientTests

The `CoubApiClientTests` class provides a comprehensive suite of xUnit tests that verify the behavior of the `CoubApiClient` class. It tests various scenarios including cache hits, API calls, error handling, and invalid inputs for video information retrieval, video existence verification, and video search operations.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Caching;
using CoubDownloader.Infrastructure.Middleware;
using Moq;

public class CoubApiClientDemo
{
public async Task RunAll()
{
// Create mock dependencies
var mockLogger = new Mock<ILoggingService>();
var mockCache = new Mock<ICacheService>();
var mockHttpMessageHandler = new Mock<System.Net.Http.HttpMessageHandler>(MockBehavior.Strict);
var httpClient = new System.Net.Http.HttpClient(mockHttpMessageHandler.Object)
{
BaseAddress = new Uri("https://coub.com/api/v2/")
};

// Create the API client
var apiClient = new CoubApiClient(httpClient, mockLogger.Object, mockCache.Object);

// GetVideoInfoAsync - Get video info with cache hit (avoids API call)
var videoUrl = "https://coub.com/view/testcoub";
var cachedVideoInfo = new CoubVideoInfo { Id = "testcoub", Title = "Cached Coub", Duration = 10 };
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out cachedVideoInfo!))
.Returns(true);

var videoInfo = await apiClient.GetVideoInfoAsync(videoUrl);
Console.WriteLine(videoInfo?.Title); // "Cached Coub"

// GetVideoInfoAsync - Get video info from API (cache miss)
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out It.Ref<CoubVideoInfo?>.IsAny))
.Returns(false);

// Configure mock HTTP response
mockHttpMessageHandler
.Protected()
.Setup<Task<System.Net.Http.HttpResponseMessage>>(
"SendAsync",
ItExpr.IsAny<System.Net.Http.HttpRequestMessage>(),
ItExpr.IsAny<System.Threading.CancellationToken>())
)
.ReturnsAsync(new System.Net.Http.HttpResponseMessage
{
StatusCode = System.Net.HttpStatusCode.OK,
Content = new System.Net.Http.StringContent(@"{
  
  \"id\": \"api123\",
  \"title\": \"API Fetched Coub\",
  \"duration\": 15,
  \"has_audio\": true
}")
});

mockCache.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<CoubVideoInfo>(), It.IsAny<TimeSpan>()));

var apiVideoInfo = await apiClient.GetVideoInfoAsync(videoUrl);
Console.WriteLine(apiVideoInfo?.Title); // "API Fetched Coub"

// GetVideoInfoAsync - Handle 404 Not Found
var notFoundUrl = "https://coub.com/view/nonexistent";
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out It.Ref<CoubVideoInfo?>.IsAny))
.Returns(false);

mockHttpMessageHandler
.Protected()
.Setup<Task<System.Net.Http.HttpResponseMessage>>(
"SendAsync",
ItExpr.IsAny<System.Net.Http.HttpRequestMessage>(),
ItExpr.IsAny<System.Threading.CancellationToken>())
)
.ReturnsAsync(new System.Net.Http.HttpResponseMessage
{
StatusCode = System.Net.HttpStatusCode.NotFound
});

var notFoundInfo = await apiClient.GetVideoInfoAsync(notFoundUrl);
Console.WriteLine(notFoundInfo); // null

// VerifyVideoExistsAsync - Check if video exists with cache hit
var existsUrl = "https://coub.com/view/existing";
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out true))
.Returns(true);

var exists = await apiClient.VerifyVideoExistsAsync(existsUrl);
Console.WriteLine(exists); // true

// VerifyVideoExistsAsync - Check if video exists from API
var apiExistsUrl = "https://coub.com/view/checkexists";
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out It.Ref<bool>.IsAny))
.Returns(false);

mockHttpMessageHandler
.Protected()
.Setup<Task<System.Net.Http.HttpResponseMessage>>(
"SendAsync",
ItExpr.IsAny<System.Net.Http.HttpRequestMessage>(),
ItExpr.IsAny<System.Threading.CancellationToken>())
)
.ReturnsAsync(new System.Net.Http.HttpResponseMessage
{
StatusCode = System.Net.HttpStatusCode.OK,
Content = new System.Net.Http.StringContent(@"{
  
  \"id\": \"exists123\",
  \"title\": \"Existing Coub\"
}")
});

mockCache.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan>()));

var apiExists = await apiClient.VerifyVideoExistsAsync(apiExistsUrl);
Console.WriteLine(apiExists); // true

// SearchVideosAsync - Search videos with cache hit
var searchQuery = "funny cats";
var cachedVideos = new List<CoubVideoInfo> { new() { Id = "c1", Title = "Funny Cat 1" } };
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out cachedVideos!))
.Returns(true);

var searchResults = await apiClient.SearchVideosAsync(searchQuery, 5);
Console.WriteLine(searchResults.Count); // 1

// SearchVideosAsync - Search videos from API
var apiSearchQuery = "dogs playing";
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out It.Ref<List<CoubVideoInfo>?>.IsAny))
.Returns(false);

mockHttpMessageHandler
.Protected()
.Setup<Task<System.Net.Http.HttpResponseMessage>>(
"SendAsync",
ItExpr.IsAny<System.Net.Http.HttpRequestMessage>(),
ItExpr.IsAny<System.Threading.CancellationToken>())
)
.ReturnsAsync(new System.Net.Http.HttpResponseMessage
{
StatusCode = System.Net.HttpStatusCode.OK,
Content = new System.Net.Http.StringContent(@"{
  
  \"coubs\": [
    {\"id\": \"d1\", \"title\": \"Dog Playing 1\"},
    {\"id\": \"d2\": \"title\": \"Dog Playing 2\"},
    {\"id\": \"d3\", \"title\": \"Dog Playing 3\"}
  ]
}")
});

mockCache.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<List<CoubVideoInfo>>(), It.IsAny<TimeSpan>()));

var apiSearchResults = await apiClient.SearchVideosAsync(apiSearchQuery, 2);
Console.WriteLine(apiSearchResults.Count); // 2
Console.WriteLine(apiSearchResults[0].Id); // "d1"
Console.WriteLine(apiSearchResults[1].Id); // "d2"
}
}
```

## IFileAdapter

The `IFileAdapter` interface provides a minimal file-system abstraction for testing purposes. It allows you to mock file operations such as writing to a file and deleting a file.

### Usage Example

```csharp
using CoubDownloader.Tests;

public class MyClass
{
    private readonly IFileAdapter _fileAdapter;

    public MyClass(IFileAdapter fileAdapter)
    {
        _fileAdapter = fileAdapter;
    }

    public void WriteToFile(string path, string contents)
    {
        _fileAdapter.WriteAllText(path, contents);
    }

    public void DeleteFile(string path)
    {
        _fileAdapter.Delete(path);
    }
}
```

## FileUtilitiesTests

The `FileUtilitiesTests` class provides a suite of xUnit tests that verify the behavior of the `FileUtilities` helper methods, including safe file name generation, file size formatting, directory creation, unique file naming, file copying with progress, and recursive directory deletion.

### Usage Example

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using CoubDownloader.Tests;

public class FileUtilitiesDemo
{
    public void RunAll()
    {
        // GenerateSafeFileName - converts invalid file names to safe versions
        var safeName = FileUtilities.GenerateSafeFileName(
            input: "video?file*name",
            extension: ".mp4");
        Console.WriteLine(safeName); // "videofilename.mp4"

        // FormatFileSize - converts bytes to human-readable format
        var sizeText = FileUtilities.FormatFileSize(1572864);
        Console.WriteLine(sizeText); // "1.50 MB"

        // EnsureDirectory - creates directory if it doesn't exist
        var directoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            "CoubDownloader");
        var ensuredPath = FileUtilities.EnsureDirectory(directoryPath);
        Console.WriteLine(Directory.Exists(ensuredPath)); // true

        // GetUniqueFileName - returns unique filename for existing files
        var basePath = Path.Combine(Path.GetTempPath(), "download.mp4");
        var uniquePath = FileUtilities.GetUniqueFileName(basePath);
        Console.WriteLine(Path.GetFileName(uniquePath)); // "download.mp4" or "download_1.mp4"

        // CopyFileWithProgressAsync - copies file with progress reporting
        var source = Path.Combine(Path.GetTempPath(), "source.txt");
        var destination = Path.Combine(Path.GetTempPath(), "destination.txt");
        File.WriteAllText(source, "test content");

        await FileUtilities.CopyFileWithProgressAsync(source, destination);
        Console.WriteLine(File.Exists(destination)); // true

        // DeleteDirectoryRecursively - deletes directory and all contents
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "file.txt"), "content");

        var deleted = FileUtilities.DeleteDirectoryRecursively(tempDir);
        Console.WriteLine(deleted); // true
    }
}
```

## AudioProcessingServiceTests

The `AudioProcessingServiceTests` class provides a comprehensive suite of xUnit tests that verify the behavior of the `AudioProcessingService` class. It tests audio processing operations including extracting audio duration, looping audio with different strategies, and synchronizing audio with video duration.

### Usage Example

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Enums;

public class AudioProcessingDemo
{
    public async Task RunAll()
    {
        // Create the service instance
        var audioService = new AudioProcessingService();

        // Define paths
        var audioPath = Path.Combine(Path.GetTempPath(), "audio.mp3");
        var outputPath = Path.Combine(Path.GetTempPath(), "looped_audio.mp3");
        var videoPath = Path.Combine(Path.GetTempPath(), "video.mp4");
        var syncedOutputPath = Path.Combine(Path.GetTempPath(), "synced_output.mp4");

        // Create dummy files for demonstration
        File.WriteAllText(audioPath, "dummy audio content");
        File.WriteAllText(videoPath, "dummy video content");

        // GetAudioDurationAsync - Extract duration from audio file
        var duration = await audioService.GetAudioDurationAsync(audioPath);
        Console.WriteLine($"Audio duration: {duration}s");

        // LoopAudioAsync - Loop audio with Repeat strategy
        var loopedPath = Path.Combine(Path.GetTempPath(), "repeated_audio.mp3");
        await audioService.LoopAudioAsync(audioPath, 30.0, loopedPath, AudioLoopStrategy.Repeat);
        Console.WriteLine("Audio looped with Repeat strategy");

        // LoopAudioAsync - Loop audio with Crossfade strategy
        var crossfadedPath = Path.Combine(Path.GetTempPath(), "crossfaded_audio.mp3");
        await audioService.LoopAudioAsync(audioPath, 25.0, crossfadedPath, AudioLoopStrategy.Crossfade);
        Console.WriteLine("Audio looped with Crossfade strategy");

        // SyncAudioWithVideoAsync - Synchronize audio with video duration
        await audioService.SyncAudioWithVideoAsync(audioPath, videoPath, syncedOutputPath, AudioLoopStrategy.Repeat);
        Console.WriteLine("Audio synchronized with video duration");

        // Cleanup
        File.Delete(audioPath);
        File.Delete(videoPath);
        File.Delete(outputPath);
        File.Delete(loopedPath);
        File.Delete(crossfadedPath);
        File.Delete(syncedOutputPath);
    }
}
```

## DateTimeExtensionsTests

The `DateTimeExtensionsTests` class provides a suite of xUnit tests that verify the behavior of extension methods for `DateTime` and `TimeSpan` operations, including relative time formatting, duration formatting, date range validation, and Unix timestamp conversion.

### Usage Example

```csharp
using System;
using CoubDownloader.Infrastructure.Utilities;

public class DateTimeDemo
{
    public void RunAll()
    {
        // GetRelativeTime - formats time spans relative to now
        var tenSecondsAgo = DateTime.UtcNow.AddSeconds(-10);
        Console.WriteLine(tenSecondsAgo.GetRelativeTime()); // "just now"

        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        Console.WriteLine(oneMinuteAgo.GetRelativeTime()); // "1m ago"

        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        Console.WriteLine(oneHourAgo.GetRelativeTime()); // "1h ago"

        // FormatDuration - formats TimeSpan as HH:MM:SS
        var duration = new TimeSpan(1, 2, 3);
        Console.WriteLine(duration.FormatDuration()); // "01:02:03"

        // IsWithinRange - checks if a date falls within a range
        var testDate = new DateTime(2026, 6, 26, 12, 0, 0);
        var rangeStart = new DateTime(2026, 6, 26, 0, 0, 0);
        var rangeEnd = new DateTime(2026, 6, 27, 0, 0, 0);
        Console.WriteLine(testDate.IsWithinRange(rangeStart, rangeEnd)); // true

        // StartOfDay - returns the date with time set to midnight
        var now = DateTime.Now;
        Console.WriteLine(now.StartOfDay()); // "2026-06-26 00:00:00" (date part only)

        // StartOfWeek - returns the date for the start of the week (Monday by default)
        var friday = new DateTime(2026, 6, 26); // Friday
        Console.WriteLine(friday.StartOfWeek(DayOfWeek.Monday)); // "2026-06-22" (previous Monday)

        // ToUnixTimestamp / FromUnixTimestamp - roundtrip conversion
        var utcDate = new DateTime(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = utcDate.ToUnixTimestamp();
        var backToDate = timestamp.FromUnixTimestamp().ToUniversalTime();
        Console.WriteLine(backToDate == utcDate); // true
    }
}
```

## CoubVideoTests

The `CoubVideoTests` class provides a suite of xUnit tests that verify the behavior of the `CoubVideo` class and its extension methods. It tests video validation, aspect ratio calculation, duration categorization, view count formatting, quality detection, and audio duration calculations.

### Usage Example

```csharp
using System;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Extensions;

public class CoubVideoDemo
{
    public void RunAll()
    {
        // Create a valid video
        var video = new CoubVideo
        {
            Id = "abc123",
            Title = "Test Video",
            Url = "https://coub.com/view/abc123",
            Duration = 15.0,
            Width = 1280,
            Height = 720,
            ViewCount = 2500000
        };

        // IsValid - checks if video has all required fields
        Console.WriteLine(video.IsValid()); // true
        
        var invalidVideo = new CoubVideo { Id = "", Duration = 0 };
        Console.WriteLine(invalidVideo.IsValid()); // false

        // GetAspectRatio - calculates width/height ratio
        Console.WriteLine(video.GetAspectRatio()); // 1.777... (16:9)

        // IsVerticalFormat - checks if video is in portrait orientation
        video.Width = 720;
        video.Height = 1280;
        Console.WriteLine(video.IsVerticalFormat()); // true
        
        video.Width = 1280;
        video.Height = 720;
        Console.WriteLine(video.IsVerticalFormat()); // false

        // GetDurationCategory - categorizes video by duration
        video.Duration = 3.0;
        Console.WriteLine(video.GetDurationCategory()); // "Short"
        
        video.Duration = 8.0;
        Console.WriteLine(video.GetDurationCategory()); // "Medium"
        
        video.Duration = 20.0;
        Console.WriteLine(video.GetDurationCategory()); // "Long"
        
        video.Duration = 60.0;
        Console.WriteLine(video.GetDurationCategory()); // "Extra Long"

        // GetFormattedViewCount - formats view count with K/M suffix
        video.ViewCount = 500;
        Console.WriteLine(video.GetFormattedViewCount()); // "500"
        
        video.ViewCount = 1500;
        Console.WriteLine(video.GetFormattedViewCount()); // "1K"
        
        video.ViewCount = 2500000;
        Console.WriteLine(video.GetFormattedViewCount()); // "2M"

        // IsHdQuality - checks if video is HD (720p or higher)
        Console.WriteLine(video.IsHdQuality()); // true (1280x720)

        // Is4kQuality - checks if video is 4K resolution
        video.Width = 3840;
        video.Height = 2160;
        Console.WriteLine(video.Is4kQuality()); // true
        
        video.Width = 1280;
        video.Height = 720;
        Console.WriteLine(video.Is4kQuality()); // false

        // CalculateRequiredAudioDuration - calculates required audio duration for looping
        video.Duration = 10.0;
        video.AudioTrack = new AudioTrack
        {
            Id = "t1",
            VideoId = video.Id,
            Duration = 4.0
        };
        
        var requiredDuration = CoubVideoExtensions.CalculateRequiredAudioDuration(video);
        Console.WriteLine(requiredDuration); // 12.0 (ceil(10/4) * 4)
        
        video.AudioTrack = null;
        Console.WriteLine(CoubVideoExtensions.CalculateRequiredAudioDuration(video)); // 0
    }
}
```

## CoubApiClientTests

The `CoubApiClientTests` class provides a comprehensive suite of xUnit tests that verify the behavior of the `CoubApiClient` class. It tests various scenarios including cache hits, API calls, error handling, and invalid inputs for video information retrieval, video existence verification, and video search operations.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Caching;
using CoubDownloader.Infrastructure.Middleware;
using Moq;

public class CoubApiClientDemo
{
public async Task RunAll()
{
// Create mock dependencies
var mockLogger = new Mock<ILoggingService>();
var mockCache = new Mock<ICacheService>();
var mockHttpMessageHandler = new Mock<System.Net.Http.HttpMessageHandler>(MockBehavior.Strict);
var httpClient = new System.Net.Http.HttpClient(mockHttpMessageHandler.Object)
{
BaseAddress = new Uri("https://coub.com/api/v2/")
};

// Create the API client
var apiClient = new CoubApiClient(httpClient, mockLogger.Object, mockCache.Object);

// GetVideoInfoAsync - Get video info with cache hit (avoids API call)
var videoUrl = "https://coub.com/view/testcoub";
var cachedVideoInfo = new CoubVideoInfo { Id = "testcoub", Title = "Cached Coub", Duration = 10 };
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out cachedVideoInfo!))
.Returns(true);

var videoInfo = await apiClient.GetVideoInfoAsync(videoUrl);
Console.WriteLine(videoInfo?.Title); // "Cached Coub"

// GetVideoInfoAsync - Get video info from API (cache miss)
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out It.Ref<CoubVideoInfo?>.IsAny))
.Returns(false);

// Configure mock HTTP response
mockHttpMessageHandler
.Protected()
.Setup<Task<System.Net.Http.HttpResponseMessage>>(
"SendAsync",
ItExpr.IsAny<System.Net.Http.HttpRequestMessage>(),
ItExpr.IsAny<System.Threading.CancellationToken>())
)
.ReturnsAsync(new System.Net.Http.HttpResponseMessage
{
StatusCode = System.Net.HttpStatusCode.OK,
Content = new System.Net.Http.StringContent(@"{
  
  \"id\": \"api123\",
  \"title\": \"API Fetched Coub\",
  \"duration\": 15,
  \"has_audio\": true
}")
});

mockCache.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<CoubVideoInfo>(), It.IsAny<TimeSpan>()));

var apiVideoInfo = await apiClient.GetVideoInfoAsync(videoUrl);
Console.WriteLine(apiVideoInfo?.Title); // "API Fetched Coub"

// GetVideoInfoAsync - Handle 404 Not Found
var notFoundUrl = "https://coub.com/view/nonexistent";
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out It.Ref<CoubVideoInfo?>.IsAny))
.Returns(false);

mockHttpMessageHandler
.Protected()
.Setup<Task<System.Net.Http.HttpResponseMessage>>(
"SendAsync",
ItExpr.IsAny<System.Net.Http.HttpRequestMessage>(),
ItExpr.IsAny<System.Threading.CancellationToken>())
)
.ReturnsAsync(new System.Net.Http.HttpResponseMessage
{
StatusCode = System.Net.HttpStatusCode.NotFound
});

var notFoundInfo = await apiClient.GetVideoInfoAsync(notFoundUrl);
Console.WriteLine(notFoundInfo); // null

// VerifyVideoExistsAsync - Check if video exists with cache hit
var existsUrl = "https://coub.com/view/existing";
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out true))
.Returns(true);

var exists = await apiClient.VerifyVideoExistsAsync(existsUrl);
Console.WriteLine(exists); // true

// VerifyVideoExistsAsync - Check if video exists from API
var apiExistsUrl = "https://coub.com/view/checkexists";
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out It.Ref<bool>.IsAny))
.Returns(false);

mockHttpMessageHandler
.Protected()
.Setup<Task<System.Net.Http.HttpResponseMessage>>(
"SendAsync",
ItExpr.IsAny<System.Net.Http.HttpRequestMessage>(),
ItExpr.IsAny<System.Threading.CancellationToken>())
)
.ReturnsAsync(new System.Net.Http.HttpResponseMessage
{
StatusCode = System.Net.HttpStatusCode.OK,
Content = new System.Net.Http.StringContent(@"{
  
  \"id\": \"exists123\",
  \"title\": \"Existing Coub\"
}")
});

mockCache.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan>()));

var apiExists = await apiClient.VerifyVideoExistsAsync(apiExistsUrl);
Console.WriteLine(apiExists); // true

// SearchVideosAsync - Search videos with cache hit
var searchQuery = "funny cats";
var cachedVideos = new List<CoubVideoInfo> { new() { Id = "c1", Title = "Funny Cat 1" } };
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out cachedVideos!))
.Returns(true);

var searchResults = await apiClient.SearchVideosAsync(searchQuery, 5);
Console.WriteLine(searchResults.Count); // 1

// SearchVideosAsync - Search videos from API
var apiSearchQuery = "dogs playing";
mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out It.Ref<List<CoubVideoInfo>?>.IsAny))
.Returns(false);

mockHttpMessageHandler
.Protected()
.Setup<Task<System.Net.Http.HttpResponseMessage>>(
"SendAsync",
ItExpr.IsAny<System.Net.Http.HttpRequestMessage>(),
ItExpr.IsAny<System.Threading.CancellationToken>())
)
.ReturnsAsync(new System.Net.Http.HttpResponseMessage
{
StatusCode = System.Net.HttpStatusCode.OK,
Content = new System.Net.Http.StringContent(@"{
  
  \"coubs\": [
    {\"id\": \"d1\", \"title\": \"Dog Playing 1\"},
    {\"id\": \"d2\": \"title\": \"Dog Playing 2\"},
    {\"id\": \"d3\", \"title\": \"Dog Playing 3\"}
  ]
}")
});

mockCache.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<List<CoubVideoInfo>>(), It.IsAny<TimeSpan>()));

var apiSearchResults = await apiClient.SearchVideosAsync(apiSearchQuery, 2);
Console.WriteLine(apiSearchResults.Count); // 2
Console.WriteLine(apiSearchResults[0].Id); // "d1"
Console.WriteLine(apiSearchResults[1].Id); // "d2"
}
}
```

## IFileAdapter