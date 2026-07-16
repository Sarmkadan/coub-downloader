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

## BatchProcessingServiceTests

The `BatchProcessingServiceTests` class provides a comprehensive suite of xUnit tests that verify the behavior of the `BatchProcessingService` class. It tests batch job creation, task management, batch processing workflows, cancellation, status retrieval, and batch deletion operations with various edge cases and error scenarios.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Infrastructure.Repositories;
using Moq;

public class BatchProcessingDemo
{
public async Task RunAll()
{
// Create mock repositories
var mockBatchRepo = new Mock<IBatchJobRepository>();
var mockTaskRepo = new Mock<IDownloadTaskRepository>();
var mockDownloadService = new Mock<ICoubDownloadService>();

// Create the batch processing service
var batchService = new BatchProcessingService(
    mockBatchRepo.Object,
    mockTaskRepo.Object,
    mockDownloadService.Object);

// CreateBatchAsync - Create a new batch job
var batchName = "My Video Download Batch";
var outputDir = Path.Combine(Path.GetTempPath(), "coub-downloads");
Directory.CreateDirectory(outputDir);

var newBatch = await batchService.CreateBatchJobAsync(batchName, outputDir);
Console.WriteLine($"Created batch: {newBatch.Id} - {newBatch.Name}");
Console.WriteLine($"State: {newBatch.State}"); // ProcessingState.Pending

// AddTasksAsync - Add download tasks to the batch
var tasks = new List<DownloadTask>
{
    new() { Url = "https://coub.com/view/coub1" },
    new() { Url = "https://coub.com/view/coub2" },
    new() { Url = "https://coub.com/view/coub3" }
};

await batchService.AddTasksAsync(newBatch.Id, tasks);
Console.WriteLine($"Added {tasks.Count} tasks to batch");

// GetBatchStatusAsync - Check batch status
var batchStatus = await batchService.GetBatchStatusAsync(newBatch.Id);
Console.WriteLine($"Batch has {batchStatus.TotalTasks} tasks, {batchStatus.CompletedTasks} completed");

// StartBatchAsync - Process all tasks in the batch
// Note: In real usage, this would actually download videos
// Here we mock the download service to return successful results
mockDownloadService.Setup(ds => ds.DownloadVideoAsync(
    It.IsAny<string>(),
    It.IsAny<System.Threading.CancellationToken>()))
.ReturnsAsync(new CoubVideo { Id = "test1", Url = "https://coub.com/view/coub1" });

var completedBatch = await batchService.StartBatchAsync(newBatch.Id);
Console.WriteLine($"Batch completed: {completedBatch.State}"); // ProcessingState.Completed
Console.WriteLine($"All tasks completed: {completedBatch.Tasks.All(t => t.State == ProcessingState.Completed)}");

// GetActiveBatchesAsync - Get all batches that are not completed
var activeBatches = await batchService.GetActiveBatchesAsync();
Console.WriteLine($"Active batches: {activeBatches.Count}");

// GetAllBatchesAsync - Get all batches
var allBatches = await batchService.GetAllBatchesAsync();
Console.WriteLine($"Total batches: {allBatches.Count}");

// CancelBatchAsync - Cancel a running batch
var runningBatch = new BatchJob
{
    Id = Guid.NewGuid().ToString(),
    Name = "Running Batch",
    State = ProcessingState.Downloading,
    Tasks = new List<DownloadTask>
    {
        new() { Id = "t1", State = ProcessingState.Downloading }
    }
};
mockBatchRepo.Setup(repo => repo.GetByIdAsync(runningBatch.Id)).ReturnsAsync(runningBatch);
mockBatchRepo.Setup(repo => repo.UpdateAsync(It.IsAny<BatchJob>())).ReturnsAsync(runningBatch);
mockTaskRepo.Setup(repo => repo.UpdateAsync(It.IsAny<DownloadTask>())).ReturnsAsync((DownloadTask t) => t);

await batchService.CancelBatchAsync(runningBatch.Id);
Console.WriteLine($"Batch cancelled: {runningBatch.State}"); // ProcessingState.Cancelled

// DeleteBatchAsync - Delete a completed batch
var completedBatchForDeletion = new BatchJob
{
    Id = Guid.NewGuid().ToString(),
    Name = "Completed Batch",
    State = ProcessingState.Completed
};
mockBatchRepo.Setup(repo => repo.GetByIdAsync(completedBatchForDeletion.Id)).ReturnsAsync(completedBatchForDeletion);
mockBatchRepo.Setup(repo => repo.DeleteAsync(completedBatchForDeletion.Id)).ReturnsAsync(true);

var deleted = await batchService.DeleteBatchAsync(completedBatchForDeletion.Id);
Console.WriteLine($"Batch deleted: {deleted}"); // true

// Cleanup
directory.Delete(outputDir, true);
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

## MemoryCacheServiceTests

The `MemoryCacheServiceTests` class provides a comprehensive suite of xUnit tests that verify the behavior of the `MemoryCacheService` class. It tests basic cache operations including setting and retrieving values, handling missing keys, removing entries, clearing the cache, tracking cache statistics, and handling expiration scenarios. The tests also cover complex type serialization/deserialization and remote cache synchronization.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using CoubDownloader.Infrastructure.Caching;
using CoubDownloader.Infrastructure.Caching.Memory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

public class MemoryCacheServiceDemo
{
    public async Task RunAll()
    {
        // Create mock dependencies
        var mockOptions = Options.Create(new MemoryCacheOptions());
        var mockMemoryCache = new Mock<IMemoryCache>();
        var mockRemoteCache = new Mock<IRemoteCache>();

        // Create the cache service
        var cacheService = new MemoryCacheService(mockOptions, mockMemoryCache.Object, mockRemoteCache.Object);

        // Set_ThenGet_ReturnsStoredValue - Store and retrieve a simple value
        await cacheService.SetAsync("test_key", "test_value", TimeSpan.FromMinutes(5));
        var value = await cacheService.GetAsync<string>("test_key");
        Console.WriteLine(value); // "test_value"

        // TryGet_ExistingKey_ReturnsTrueAndValue - Retrieve existing key with TryGet
        var existsResult = await cacheService.TryGetAsync<string>("test_key");
        Console.WriteLine(existsResult.Exists); // true
        Console.WriteLine(existsResult.Value); // "test_value"

        // TryGet_MissingKey_ReturnsFalseAndDefault - TryGet on missing key
        var missingResult = await cacheService.TryGetAsync<string>("missing_key");
        Console.WriteLine(missingResult.Exists); // false
        Console.WriteLine(missingResult.Value); // null

        // Remove_ExistingKey_KeyNoLongerRetrievable - Remove a cached entry
        await cacheService.SetAsync("removable_key", "to_remove");
        await cacheService.RemoveAsync("removable_key");
        var removedValue = await cacheService.GetAsync<string>("removable_key");
        Console.WriteLine(removedValue); // null

        // Clear_AfterMultipleSets_CacheIsEmpty - Clear entire cache
        await cacheService.SetAsync("key1", "value1");
        await cacheService.SetAsync("key2", "value2");
        await cacheService.ClearAsync();
        var clearedValue1 = await cacheService.GetAsync<string>("key1");
        var clearedValue2 = await cacheService.GetAsync<string>("key2");
        Console.WriteLine(clearedValue1); // null
        Console.WriteLine(clearedValue2); // null

        // GetStatistics_AfterHitsAndMisses_TracksAccurately - Check cache statistics
        await cacheService.SetAsync("stats_key", "stats_value");
        await cacheService.GetAsync<string>("stats_key"); // hit
        await cacheService.TryGetAsync<string>("missing_stats_key"); // miss
        
        var stats = cacheService.GetStatistics();
        Console.WriteLine(stats.Hits); // 1
        Console.WriteLine(stats.Misses); // 1
        Console.WriteLine(stats.Total); // 2

        // GetStatistics_EmptyCache_HitRateIsZero - Statistics for empty cache
        var emptyCacheService = new MemoryCacheService(
            Options.Create(new MemoryCacheOptions()),
            new Mock<IMemoryCache>().Object,
            new Mock<IRemoteCache>().Object
        );
        var emptyStats = emptyCacheService.GetStatistics();
        Console.WriteLine(emptyStats.Hits); // 0
        Console.WriteLine(emptyStats.Misses); // 0
        Console.WriteLine(emptyStats.HitRate); // 0

        // Set_ExpiredTtl_EntryNotRetrievable - Set with expiration
        await cacheService.SetAsync("expiring_key", "expiring_value", TimeSpan.FromMilliseconds(100));
        await Task.Delay(200); // Wait for expiration
        var expiredValue = await cacheService.GetAsync<string>("expiring_key");
        Console.WriteLine(expiredValue); // null

        // Set_OverwritesExistingKey - Overwrite existing key
        await cacheService.SetAsync("overwrite_key", "first_value");
        await cacheService.SetAsync("overwrite_key", "second_value");
        var overwrittenValue = await cacheService.GetAsync<string>("overwrite_key");
        Console.WriteLine(overwrittenValue); // "second_value"

        // TryGet_ComplexType_DeserializesCorrectly - Complex type handling
        var complexObject = new TestData { Name = "Test", Value = 42 };
        await cacheService.SetAsync("complex_key", complexObject);
        var retrievedComplex = await cacheService.GetAsync<TestData>("complex_key");
        Console.WriteLine(retrievedComplex?.Name); // "Test"
        Console.WriteLine(retrievedComplex?.Value); // 42

        // Set_PropagatesValueToRemoteCache - Remote cache synchronization
        await cacheService.SetAsync("remote_key", "remote_value");
        mockRemoteCache.Verify(r => r.SetAsync("remote_key", "remote_value", It.IsAny<TimeSpan>()), Times.Once);

        // TryGet_HitOnLocal_DoesNotQueryRemote - Local cache hit optimization
        await cacheService.SetAsync("local_hit_key", "local_value");
        var localHitResult = await cacheService.TryGetAsync<string>("local_hit_key");
        Console.WriteLine(localHitResult.Exists); // true
        mockRemoteCache.Verify(r => r.TryGetAsync<string>(It.IsAny<string>()), Times.Never);

        // TryGet_LocalMissRemoteHit_CachesLocallyAndReturnsValue - Remote fallback
        mockMemoryCache.Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
            .Returns(false);
        mockRemoteCache.Setup(r => r.TryGetAsync<string>("remote_miss_key"))
            .ReturnsAsync((true, "remote_value"));
        
        var remoteHitResult = await cacheService.TryGetAsync<string>("remote_miss_key");
        Console.WriteLine(remoteHitResult.Exists); // true
        Console.WriteLine(remoteHitResult.Value); // "remote_value"
        
        // Verify local cache was populated
        mockMemoryCache.Verify(m => m.Set("remote_miss_key", "remote_value", It.IsAny<TimeSpan>()), Times.Once);

        // Remove_PropagatesDeletionToRemoteCache - Remove from remote cache
        await cacheService.RemoveAsync("remote_remove_key");
        mockRemoteCache.Verify(r => r.RemoveAsync("remote_remove_key"), Times.Once);

        // Clear_PropagatesClearToRemoteCache - Clear remote cache
        await cacheService.ClearAsync();
        mockRemoteCache.Verify(r => r.ClearAsync(), Times.Once);

        // Set_RemoteThrows_DoesNotBubbleException - Error handling
        mockRemoteCache.Setup(r => r.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
            .ThrowsAsync(new Exception("Remote cache error"));
        
        try
        {
            await cacheService.SetAsync("error_key", "error_value");
            Console.WriteLine("No exception thrown");
        }
        catch
        {
            Console.WriteLine("Exception was thrown");
        }
    }

    private class TestData
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }
}
```

## ValidationException

The `ValidationException` class is a custom exception used to indicate validation failures in the application. It extends `System.Exception` and provides additional context about which parameter failed validation, what value was provided, and what the expected behavior should be. This exception is particularly useful for domain validation scenarios where you need to communicate detailed error information to callers.

### Usage Example

```csharp
using System;
using CoubDownloader.Domain.Exceptions;

public class ValidationExample
{
    public void ProcessVideo(string videoId, int duration)
    {
        // Validate video ID
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new ValidationException(
                "Video ID cannot be null or empty.",
                nameof(videoId),
                videoId
            );
        }

        // Validate duration
        if (duration <= 0)
        {
            throw new ValidationException(
                "Video duration must be a positive number.",
                nameof(duration),
                duration
            );
        }

        // Validate video ID format
        if (videoId.Length > 50)
        {
            throw new ValidationException(
                "Video ID exceeds maximum length of 50 characters.",
                nameof(videoId),
                videoId
            );
        }

        // Process video...
    }

    public void ProcessWithInnerException(string filePath)
    {
        try
        {
            // Some operation that might fail
        }
        catch (Exception ex)
        {
            throw new ValidationException(
                "Failed to process video file due to validation error.",
                nameof(filePath),
                filePath,
                ex
            );
        }
    }
}
```

## ConfigurationException

The `ConfigurationException` class is a custom exception used to indicate configuration-related errors in the application. It extends `System.Exception` and provides additional context about which configuration key caused the error. This exception is particularly useful for scenarios where application configuration is invalid or missing, allowing for better error diagnosis and recovery.

### Usage Example

```csharp
using System;
using CoubDownloader.Domain.Exceptions;

public class ConfigurationExample
{
    public void LoadConfiguration(string configKey)
    {
        // Validate configuration
        if (string.IsNullOrEmpty(configKey))
        {
            throw new ConfigurationException(
                "Configuration key cannot be null or empty.",
                configKey
            );
        }

        // Check if configuration exists
        var configValue = Environment.GetEnvironmentVariable(configKey);
        if (string.IsNullOrEmpty(configValue))
        {
            throw new ConfigurationException(
                $"Required configuration '{configKey}' is missing or empty.",
                configKey
            );
        }

        // Validate configuration format
        if (!int.TryParse(configValue, out var parsedValue) || parsedValue <= 0)
        {
            throw new ConfigurationException(
                $"Configuration '{configKey}' must be a positive integer.",
                configKey,
                new FormatException("Invalid format")
            );
        }

        // Use configuration...
    }

    public void ProcessWithInnerException(string filePath)
    {
        try
        {
            // Some operation that might fail due to configuration
        }
        catch (Exception ex)
        {
            throw new ConfigurationException(
                "Failed to process configuration-dependent operation.",
                nameof(filePath),
                ex
            );
        }
    }
}
```

## NetworkException

The `NetworkException` class is a custom exception used to indicate network-related failures in the application, particularly HTTP request failures when interacting with external services. It extends `CoubDownloaderException` and captures detailed information about failed network operations including the target URL, HTTP status code, and whether the failure was due to a timeout. This exception is particularly useful for debugging network issues and provides all necessary context to diagnose failures when downloading videos or communicating with APIs.

### Usage Example

```csharp
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CoubDownloader.Domain.Exceptions;

public class NetworkOperationsDemo
{
    private readonly HttpClient _httpClient = new HttpClient();
    
    public async Task DownloadVideoWithNetworkErrorHandling(string videoUrl)
    {
        try
        {
            // Attempt to download video from URL
            var response = await _httpClient.GetAsync(videoUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                // Throw NetworkException with status code
                throw new NetworkException(
                    "Failed to download video due to HTTP error.",
                    videoUrl,
                    (int)response.StatusCode
                );
            }
            
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Successfully downloaded: {content.Length} bytes");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("timed out"))
        {
            // Timeout-specific error handling
            throw new NetworkException(
                "Video download timed out after 30 seconds.",
                videoUrl,
                ex
            );
        }
        catch (HttpRequestException ex)
        {
            // General network error
            throw new NetworkException(
                "Network error occurred while attempting to download video.",
                videoUrl,
                ex
            );
        }
    }
    
    public async Task CheckApiAvailability(string apiUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(apiUrl);
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NetworkException(
                    "API endpoint not found.",
                    apiUrl,
                    (int)HttpStatusCode.NotFound
                );
            }
            
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new NetworkException(
                    "API service is currently unavailable.",
                    apiUrl,
                    (int)HttpStatusCode.ServiceUnavailable
                );
            }
            
            response.EnsureSuccessStatusCode();
            Console.WriteLine("API is available and responding correctly.");
        }
        catch (NetworkException ex)
        {
            // Log detailed network error information
            Console.WriteLine($"Network error: {ex.Message}");
            Console.WriteLine($"URL: {ex.Url}");
            Console.WriteLine($"HTTP Status: {ex.HttpStatusCode}");
            Console.WriteLine($"Is Timeout: {ex.IsTimeout}");
            Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
            
            // Re-throw with additional context
            throw;
        }
    }
}
```

## CoubDownloaderException

The `CoubDownloaderException` class is the base exception type for all custom exceptions in the CoubDownloader application. It extends `System.Exception` and provides additional context about failed operations including the video URL, HTTP status codes, file paths, and information about underlying tool failures. This exception serves as the foundation for all domain-specific exceptions in the application, allowing for consistent error handling and detailed error reporting when downloading, processing, or converting videos.

### Usage Example

```csharp
using System;
using CoubDownloader.Domain.Exceptions;

public class CoubDownloaderExceptionExample
{
    public void ProcessVideoWithErrorHandling(string videoUrl, string outputPath)
    {
        try
        {
            // Attempt to download and process a video
            DownloadAndProcessVideo(videoUrl, outputPath);
        }
        catch (CoubDownloaderException ex) when (ex.VideoUrl != null)
        {
            // Log detailed error information
            Console.WriteLine($"CoubDownloader error occurred: {ex.Message}");
            Console.WriteLine($"Video URL: {ex.VideoUrl}");
            
            if (ex.HttpStatusCode.HasValue)
            {
                Console.WriteLine($"HTTP Status Code: {ex.HttpStatusCode}");
            }
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            
            // Handle specific exception types
            switch (ex)
            {
                case VideoDownloadException downloadEx:
                    Console.WriteLine($"Video download failed: {downloadEx.Message}");
                    break;
                    
                case VideoConversionException conversionEx:
                    Console.WriteLine($"Video conversion failed: {conversionEx.Message}");
                    if (!string.IsNullOrEmpty(conversionEx.InputPath))
                    {
                        Console.WriteLine($"Input file: {conversionEx.InputPath}");
                    }
                    if (!string.IsNullOrEmpty(conversionEx.OutputPath))
                    {
                        Console.WriteLine($"Output file: {conversionEx.OutputPath}");
                    }
                    break;
                    
                case AudioProcessingException audioEx:
                    Console.WriteLine($"Audio processing failed: {audioEx.Message}");
                    if (!string.IsNullOrEmpty(audioEx.AudioFilePath))
                    {
                        Console.WriteLine($"Audio file: {audioEx.AudioFilePath}");
                    }
                    break;
                    
                case ProcessExecutionException processEx:
                    Console.WriteLine($"External tool failed: {processEx.Message}");
                    Console.WriteLine($"Tool: {processEx.ToolName}");
                    break;
                    
                case MetadataExtractionException metadataEx:
                    Console.WriteLine($"Metadata extraction failed: {metadataEx.Message}");
                    if (!string.IsNullOrEmpty(metadataEx.SourceUrl))
                    {
                        Console.WriteLine($"Source URL: {metadataEx.SourceUrl}");
                    }
                    break;
                    
                case ToolNotFoundException toolEx:
                    Console.WriteLine($"Required tool not found: {toolEx.Message}");
                    Console.WriteLine($"Tool name: {toolEx.ToolName}");
                    break;
                    
                case FileOperationException fileEx:
                    Console.WriteLine($"File operation failed: {fileEx.Message}");
                    Console.WriteLine($"File path: {fileEx.FilePath}");
                    break;
                    
                case NetworkException networkEx:
                    Console.WriteLine($"Network error: {networkEx.Message}");
                    Console.WriteLine($"URL: {networkEx.Url}");
                    if (networkEx.HttpStatusCode.HasValue)
                    {
                        Console.WriteLine($"HTTP Status: {networkEx.HttpStatusCode}");
                    }
                    Console.WriteLine($"Is Timeout: {networkEx.IsTimeout}");
                    break;
            }
            
            throw; // Re-throw the exception
        }
    }
    
    private void DownloadAndProcessVideo(string videoUrl, string outputPath)
    {
        // Implementation that may throw CoubDownloaderException
        // or any of its derived exception types
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

## CoubDownloadServiceTests

The `CoubDownloadServiceTests` class provides a comprehensive suite of xUnit tests that verify the behavior of the `CoubDownloadService` class. It tests various scenarios for downloading, processing, and verifying Coub videos including metadata fetching, video source extraction, file verification, and actual video file downloading with both success and error cases.

### Usage Example

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Repositories;
using Moq;

public class CoubDownloadServiceDemo
{
    public async Task RunAll()
    {
        // Create mock dependencies
        var mockHttpClient = new Mock<System.Net.Http.HttpClient>();
        var mockVideoRepository = new Mock<ICoubVideoRepository>();
        var mockCoubApiClient = new Mock<ICoubApiClient>();

        // Create the download service
        var downloadService = new CoubDownloadService(
            mockHttpClient.Object,
            mockVideoRepository.Object,
            mockCoubApiClient.Object);

        // Define test data
        var coubUrl = "https://coub.com/view/test123";
        var outputDirectory = Path.Combine(Path.GetTempPath(), "coub-downloads");
        Directory.CreateDirectory(outputDirectory);

        // FetchMetadataAsync - Retrieve video metadata from Coub API
        var mockVideoInfo = new CoubVideoInfo
        {
            Id = "test123",
            Title = "Test Coub Video",
            Duration = 15,
            HasAudio = true,
            ChannelUrl = "test_channel",
            ViewCount = 1250
        };

        mockCoubApiClient.Setup(api => api.GetVideoInfoAsync(coubUrl, It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(mockVideoInfo);

        var metadata = await downloadService.FetchMetadataAsync(coubUrl);
        Console.WriteLine($"Fetched metadata: {metadata.Title} ({metadata.Duration}s)");
        Console.WriteLine($"Dimensions: {metadata.Width}x{metadata.Height}");
        Console.WriteLine($"Views: {metadata.ViewCount}");

        // ExtractVideoSourceAsync - Get the video source URL from metadata
        var sourceUrl = await downloadService.ExtractVideoSourceAsync(coubUrl);
        Console.WriteLine($"Video source URL: {sourceUrl}");

        // DownloadVideoAsync - Download a complete Coub video with metadata
        var downloadedVideo = await downloadService.DownloadVideoAsync(coubUrl);
        Console.WriteLine($"Downloaded video: {downloadedVideo.Title}");
        Console.WriteLine($"Saved to repository with ID: {downloadedVideo.Id}");

        // VerifyDownloadAsync - Check if a downloaded file exists and is valid
        var testFilePath = Path.Combine(outputDirectory, "test_video.mp4");
        File.WriteAllText(testFilePath, "video content");
        
        var isValid = await downloadService.VerifyDownloadAsync(testFilePath);
        Console.WriteLine($"File verification: {isValid}"); // true

        // DownloadVideoFileAsync - Download the actual video file
        var outputPath = Path.Combine(outputDirectory, "downloaded_coub.webm");
        var downloadedPath = await downloadService.DownloadVideoFileAsync(sourceUrl, outputPath);
        Console.WriteLine($"Video file downloaded to: {downloadedPath}");
        Console.WriteLine($"File exists: {File.Exists(downloadedPath)}");

        // Error handling examples
        
        // Invalid URL - throws ArgumentException
        try
        {
            await downloadService.DownloadVideoAsync("");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Caught expected error: {ex.Message}");
        }

        // Metadata extraction failure - throws MetadataExtractionException
        mockCoubApiClient.Setup(api => api.GetVideoInfoAsync(It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync((CoubVideoInfo)null!);

        try
        {
            await downloadService.FetchMetadataAsync("https://coub.com/view/nonexistent");
        }
        catch (MetadataExtractionException ex)
        {
            Console.WriteLine($"Caught expected error: {ex.Message}");
        }

        // Cleanup
        if (File.Exists(testFilePath)) File.Delete(testFilePath);
        if (File.Exists(outputPath)) File.Delete(outputPath);
        Directory.Delete(outputDirectory, true);
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

## BatchJob

The `BatchJob` class represents a batch processing job for downloading and converting multiple Coub videos. It manages a collection of download tasks with shared settings and provides progress tracking, status management, and batch lifecycle operations. Batch jobs support parallel processing, error handling, and detailed progress reporting.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

public class BatchJobDemo
{
    public async Task RunBatchProcessingDemo()
    {
        // Create a new batch job
        var batchJob = new BatchJob
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Weekend Coub Download Batch",
            Description = "Download and convert trending Coubs from this weekend",
            OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "CoubDownloads", DateTime.Now.ToString("yyyy-MM-dd")),
            MaxParallelTasks = 4,
            ContinueOnError = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add download tasks to the batch
        batchJob.Tasks = new List<DownloadTask>
        {
            new DownloadTask
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://coub.com/view/coub123",
                OutputFileName = "funny_cat_coub.mp4",
                State = ProcessingState.Pending
            },
            new DownloadTask
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://coub.com/view/coub456",
                OutputFileName = "dancing_dog_coub.mp4",
                State = ProcessingState.Pending
            },
            new DownloadTask
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://coub.com/view/coub789",
                OutputFileName = "epic_fail_coub.mp4",
                State = ProcessingState.Pending
            }
        };

        // Set shared conversion settings for all tasks
        batchJob.SharedSettings = new ConversionSettings
        {
            Width = 1920,
            Height = 1080,
            FrameRate = 30,
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            VideoBitrate = 5000,
            AudioBitrate = 192,
            Format = "mp4"
        };

        // Check batch status
        Console.WriteLine($"Batch created: {batchJob.Name}");
        Console.WriteLine($"Total tasks: {batchJob.TotalTasks}");
        Console.WriteLine($"State: {batchJob.State}");
        Console.WriteLine($"Progress: {batchJob.GetProgressPercent()}%");
        Console.WriteLine($"Estimated time: {batchJob.GetElapsedTime()?.ToString("g") ?? "Not started"}");

        // Start processing the batch
        batchJob.State = ProcessingState.Downloading;
        batchJob.StartedAt = DateTime.UtcNow;
        batchJob.UpdatedAt = DateTime.UtcNow;

        // Simulate processing tasks
        foreach (var task in batchJob.Tasks)
        {
            task.State = ProcessingState.Downloading;
            batchJob.CompletedTasks++;
            batchJob.UpdatedAt = DateTime.UtcNow;

            Console.WriteLine($"Processed task {task.OutputFileName}: {task.State}");
        }

        // Mark batch as completed
        batchJob.State = ProcessingState.Completed;
        batchJob.CompletedAt = DateTime.UtcNow;
        batchJob.UpdatedAt = DateTime.UtcNow;

        Console.WriteLine($"\nBatch completed successfully!");
        Console.WriteLine($"Total duration: {batchJob.GetElapsedTime()?.ToString("g")}");
        Console.WriteLine($"Final progress: {batchJob.GetProgressPercent()}%");
        Console.WriteLine($"Failed tasks: {batchJob.FailedTasks}");
        Console.WriteLine($"Output directory: {batchJob.OutputDirectory}");

        // Check pending tasks (should be 0 after completion)
        Console.WriteLine($"Pending tasks: {batchJob.GetPendingTaskCount()}");
        Console.WriteLine($"Is completed: {batchJob.IsCompleted}");
    }
}
```

## DownloadResult

The `DownloadResult` class represents the outcome of a download and conversion operation. It contains detailed information about the operation's success or failure, including output file paths, processing metrics, video format and quality settings, error details, warnings, and metadata. This class is used throughout the application to track and report on individual download tasks.

### Usage Example

```csharp
using System;
using System.IO;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

public class DownloadResultDemo
{
    public void ProcessDownloadResult()
    {
        // Create a successful download result
        var successResult = new DownloadResult
        {
            Id = Guid.NewGuid().ToString(),
            TaskId = "task_12345",
            Success = true,
            OutputFilePath = @"/home/user/Videos/coub-downloads/funny_cat.mp4",
            OutputFileSizeBytes = 5242880, // 5MB
            ProcessingTimeMs = 1250,
            Format = VideoFormat.Mp4,
            Quality = VideoQuality.Hd1080p,
            VideoMetadata = "{"width":1920,"height":1080,"duration":15.5}",
            AudioSyncInfo = "Audio synced successfully (15.5s)",
            CompletedAt = DateTime.UtcNow
        };

        // Add warnings if any occurred during processing
        successResult.AddWarning("Audio normalization applied");
        successResult.AddWarning("Metadata extraction warning: bitrate mismatch");

        // Display result information
        Console.WriteLine($"Status: {successResult.GetStatusMessage()}");
        Console.WriteLine($"File: {Path.GetFileName(successResult.OutputFilePath)}");
        Console.WriteLine($"Size: {successResult.OutputFileSizeBytes} bytes ({successResult.FormatFileSize(successResult.OutputFileSizeBytes)})");
        Console.WriteLine($"Processing time: {successResult.ProcessingTimeMs}ms");
        Console.WriteLine($"Format: {successResult.Format}");
        Console.WriteLine($"Quality: {successResult.Quality}");
        Console.WriteLine($"Warnings: {successResult.Warnings.Count}");
        
        // Create a failed download result
        var failedResult = new DownloadResult
        {
            Id = Guid.NewGuid().ToString(),
            TaskId = "task_67890",
            Success = false,
            ErrorMessage = "Network timeout while downloading video source",
            ErrorType = "NetworkError",
            ErrorStackTrace = "at CoubDownloader.Infrastructure.Services.DownloadService.DownloadVideoAsync...",
            CompletedAt = DateTime.UtcNow
        };

        // Display failure information
        Console.WriteLine($"\nFailed result: {failedResult.GetStatusMessage()}");
        Console.WriteLine($"Error: {failedResult.ErrorMessage}");
        Console.WriteLine($"Error type: {failedResult.ErrorType}");
    }
}
```

## AudioTrack

The `AudioTrack` class represents an audio track extracted from a Coub video. It contains detailed audio metadata including technical specifications (sample rate, channels, bitrate, codec), looping configuration for synchronization with video content, and validation methods to ensure audio track integrity.

### Usage Example

```csharp
using System;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

public class AudioTrackDemo
{
    public void ProcessAudioTrack()
    {
        // Create an audio track with default settings
        var audioTrack = new AudioTrack
        {
            Id = "audio_12345",
            VideoId = "video_67890",
            Duration = 15.5,
            SampleRate = 44100,
            Channels = 2,
            Bitrate = 192,
            Codec = "aac",
            FilePath = @"/path/to/audio.mp3",
            LoopStrategy = AudioLoopStrategy.Repeat,
            LoopCount = 3,
            FadeInMs = 100,
            FadeOutMs = 200,
            VolumeLevel = 1.0,
            SyncDuration = 15.5,
            CreatedAt = DateTime.UtcNow
        };

        // Validate the audio track
        Console.WriteLine($"Is valid: {audioTrack.IsValid()}");
        Console.WriteLine($"Audio spec: {audioTrack.GetAudioSpec()}");
        Console.WriteLine($"Original duration: {audioTrack.Duration}s");
        Console.WriteLine($"Looped duration: {audioTrack.CalculateLoopedDuration():F2}s");

        // Update audio properties
        audioTrack.LoopStrategy = AudioLoopStrategy.Crossfade;
        audioTrack.LoopCount = 2;
        audioTrack.VolumeLevel = 0.8;

        // Calculate new looped duration
        var newLoopedDuration = audioTrack.CalculateLoopedDuration();
        Console.WriteLine($"Updated looped duration: {newLoopedDuration:F2}s");

        // Access audio metadata
        Console.WriteLine($"Sample rate: {audioTrack.SampleRate}Hz");
        Console.WriteLine($"Channels: {audioTrack.Channels}");
        Console.WriteLine($"Bitrate: {audioTrack.Bitrate}kbps");
        Console.WriteLine($"Codec: {audioTrack.Codec}");
        Console.WriteLine($"Created at: {audioTrack.CreatedAt}");
    }
}
```

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

public class BatchJobDemo
{
    public async Task RunBatchProcessingDemo()
    {
        // Create a new batch job
        var batchJob = new BatchJob
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Weekend Coub Download Batch",
            Description = "Download and convert trending Coubs from this weekend",
            OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "CoubDownloads", DateTime.Now.ToString("yyyy-MM-dd")),
            MaxParallelTasks = 4,
            ContinueOnError = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add download tasks to the batch
        batchJob.Tasks = new List<DownloadTask>
        {
            new DownloadTask
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://coub.com/view/coub123",
                OutputFileName = "funny_cat_coub.mp4",
                State = ProcessingState.Pending
            },
            new DownloadTask
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://coub.com/view/coub456",
                OutputFileName = "dancing_dog_coub.mp4",
                State = ProcessingState.Pending
            },
            new DownloadTask
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://coub.com/view/coub789",
                OutputFileName = "epic_fail_coub.mp4",
                State = ProcessingState.Pending
            }
        };

        // Set shared conversion settings for all tasks
        batchJob.SharedSettings = new ConversionSettings
        {
            Width = 1920,
            Height = 1080,
            FrameRate = 30,
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            VideoBitrate = 5000,
            AudioBitrate = 192,
            Format = "mp4"
        };

        // Check batch status
        Console.WriteLine($"Batch created: {batchJob.Name}");
        Console.WriteLine($"Total tasks: {batchJob.TotalTasks}");
        Console.WriteLine($"State: {batchJob.State}");
        Console.WriteLine($"Progress: {batchJob.GetProgressPercent()}%");
        Console.WriteLine($"Estimated time: {batchJob.GetElapsedTime()?.ToString("g") ?? "Not started"}");

        // Start processing the batch
        batchJob.State = ProcessingState.Downloading;
        batchJob.StartedAt = DateTime.UtcNow;
        batchJob.UpdatedAt = DateTime.UtcNow;

        // Simulate processing tasks
        foreach (var task in batchJob.Tasks)
        {
            task.State = ProcessingState.Downloading;
            batchJob.CompletedTasks++;
            batchJob.UpdatedAt = DateTime.UtcNow;
            
            Console.WriteLine($"Processed task {task.OutputFileName}: {task.State}");
        }

        // Mark batch as completed
        batchJob.State = ProcessingState.Completed;
        batchJob.CompletedAt = DateTime.UtcNow;
        batchJob.UpdatedAt = DateTime.UtcNow;

        Console.WriteLine($"\nBatch completed successfully!");
        Console.WriteLine($"Total duration: {batchJob.GetElapsedTime()?.ToString("g")}");
        Console.WriteLine($"Final progress: {batchJob.GetProgressPercent()}%");
        Console.WriteLine($"Failed tasks: {batchJob.FailedTasks}");
        Console.WriteLine($"Output directory: {batchJob.OutputDirectory}");

        // Check pending tasks (should be 0 after completion)
        Console.WriteLine($"Pending tasks: {batchJob.GetPendingTaskCount()}");
        Console.WriteLine($"Is completed: {batchJob.IsCompleted}");
    }
}
```

## CoubPlaylist

The `CoubPlaylist` class represents a Coub playlist sourced from a channel feed or tag page, containing an ordered collection of video URLs ready for batch processing. It provides properties for playlist metadata and methods to check validity and retrieve effective video URLs while respecting optional limits.

### Usage Example

```csharp
using System;
using System.Linq;
using CoubDownloader.Domain.Models;

public class CoubPlaylistDemo
{
    public void ProcessPlaylist()
    {
        // Create a new playlist
        var playlist = new CoubPlaylist
        {
            Id = "channel_funny_cats",
            Title = "Funny Cats Channel",
            Description = "A collection of funny cat videos from Coub",
            PlaylistUrl = "https://coub.com/channel/funnycats",
            MaxVideos = 10,
            CreatedAt = DateTime.UtcNow,
            FetchedAt = DateTime.UtcNow
        };

        // Add video URLs to the playlist
        playlist.VideoUrls.AddRange(new[]
        {
            "https://coub.com/view/coub1",
            "https://coub.com/view/coub2",
            "https://coub.com/view/coub3",
            "https://coub.com/view/coub4",
            "https://coub.com/view/coub5"
        });

        // Check if playlist is valid (has ID, URL, and videos)
        Console.WriteLine($"Is valid: {playlist.IsValid()}"); // true

        // Check if playlist is empty
        Console.WriteLine($"Is empty: {playlist.IsEmpty()}"); // false

        // Get total video count
        Console.WriteLine($"Total videos: {playlist.TotalVideos}"); // 5

        // Get effective video URLs (respects MaxVideos limit)
        var effectiveUrls = playlist.GetEffectiveVideoUrls();
        Console.WriteLine($"Effective URLs count: {effectiveUrls.Count()}"); // 5

        // Update MaxVideos to limit the playlist
        playlist.MaxVideos = 3;
        var limitedUrls = playlist.GetEffectiveVideoUrls();
        Console.WriteLine($"Limited URLs count: {limitedUrls.Count()}"); // 3

        // Access playlist properties
        Console.WriteLine($"Playlist ID: {playlist.Id}");
        Console.WriteLine($"Title: {playlist.Title}");
        Console.WriteLine($"Description: {playlist.Description}");
        Console.WriteLine($"Playlist URL: {playlist.PlaylistUrl}");
        Console.WriteLine($"Created at: {playlist.CreatedAt}");
        Console.WriteLine($"Fetched at: {playlist.FetchedAt}");
    }
}
```

## VideoSection

The `VideoSection` class represents a segment or chapter within a video that can be extracted, processed, and combined with other sections to create custom video compositions. It provides properties for timing, descriptions, transitions, and validation to support video editing workflows.

### Usage Example

```csharp
using System;
using CoubDownloader.Domain.Models;

public class VideoSectionDemo
{
    public void CreateAndProcessVideoSection()
    {
        // Create a video section for a compilation
        var section = new VideoSection
        {
            Id = Guid.NewGuid().ToString(),
            VideoId = "abc123",
            Index = 1,
            StartTime = 15.5,
            EndTime = 28.75,
            Description = "Funny cat jumping scene",
            IsIncluded = true,
            TransitionEffect = "fade",
            TransitionDurationMs = 300
        };

        // Validate the section
        Console.WriteLine($"Section is valid: {section.IsValid()}"); // true
        Console.WriteLine($"Section duration: {section.GetDuration():F2}s"); // 13.25s

        // Create another section
        var outroSection = new VideoSection
        {
            Id = Guid.NewGuid().ToString(),
            VideoId = "abc123",
            Index = 2,
            StartTime = 28.75,
            EndTime = 35.0,
            Description = "Outro with credits",
            IsIncluded = true,
            TransitionEffect = "slide_right",
            TransitionDurationMs = 500
        };

        // Process sections
        var sections = new[] { section, outroSection };
        foreach (var s in sections.OrderBy(s => s.Index))
        {
            Console.WriteLine($"Section {s.Index}: {s.Description} ({s.StartTime:F1}s - {s.EndTime:F1}s)");
        }
    }
}
```

## FileOperationException

The `FileOperationException` class is a custom exception used to indicate file system operation failures in the application. It extends `CoubDownloaderException` and provides additional context about which file path caused the error and what type of operation failed. This exception is particularly useful for scenarios where file operations might fail due to permission issues, missing directories, or corrupted files, allowing for better error diagnosis and recovery.

### Usage Example

```csharp
using System;
using System.IO;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Domain.Enums;

public class FileOperationExample
{
    public void ProcessVideoFile(string inputPath, string outputPath)
    {
        try
        {
            // Validate input file exists
            if (!File.Exists(inputPath))
            {
                throw new FileOperationException(
                    "Input video file does not exist.",
                    inputPath,
                    FileOperationType.ExistsCheck
                );
            }

            // Ensure output directory exists
            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputDirectory))
            {
                throw new FileOperationException(
                    "Output directory does not exist and cannot be created.",
                    outputDirectory,
                    FileOperationType.CreateDirectory
                );
            }

            // Read video file
            var videoContent = File.ReadAllBytes(inputPath);
            
            // Process video content...
            
            // Write output file
            File.WriteAllBytes(outputPath, videoContent);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new FileOperationException(
                "Access denied while processing video file.",
                inputPath,
                FileOperationType.Write,
                ex
            );
        }
        catch (IOException ex)
        {
            throw new FileOperationException(
                "Failed to read or write video file due to I/O error.",
                inputPath,
                FileOperationType.Write,
                ex
            );
        }
    }

    public void DeleteTemporaryFiles(string[] tempFiles)
    {
        foreach (var filePath in tempFiles)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new FileOperationException(
                    $"Failed to delete temporary file: {filePath}",
                    filePath,
                    FileOperationType.Delete,
                    ex
                );
            }
        }
    }
}
```

## ProcessExecutionException

The `ProcessExecutionException` class is a custom exception thrown when external process execution fails (e.g., FFmpeg, FFprobe, or other command-line tools). It extends `CoubDownloaderException` and captures detailed information about the failed process including the process name, arguments, exit code, and standard error output. This exception is particularly useful for debugging video processing failures and provides all necessary context to diagnose issues with external tool execution.

### Usage Example

```csharp
using System;
using CoubDownloader.Domain.Exceptions;

public class VideoProcessingService
{
    public void ConvertVideoWithFFmpeg(string inputPath, string outputPath)
    {
        try
        {
            // Execute FFmpeg process
            var process = System.Diagnostics.Process.Start("ffmpeg", $"-i \"{inputPath}\" -c:v libx264 \"{outputPath}\"");
            process.WaitForExit();

            // Check exit code
            if (process.ExitCode != 0)
            {
                throw new ProcessExecutionException(
                    "FFmpeg process failed to convert video.",
                    "ffmpeg",
                    $"-i \"{inputPath}\" -c:v libx264 \"{outputPath}\"",
                    process.ExitCode,
                    process.StandardError.ReadToEnd()
                );
            }
        }
        catch (ProcessExecutionException ex)
        {
            // Log detailed error information
            Console.WriteLine($"Process failed: {ex.Message}");
            Console.WriteLine($"Process: {ex.ProcessName}");
            Console.WriteLine($"Arguments: {ex.Arguments}");
            Console.WriteLine($"Exit Code: {ex.ExitCode}");
            Console.WriteLine($"Error Output: {ex.StandardError}");
            
            // Re-throw with additional context
            throw new ProcessExecutionException(
                $"Video conversion failed for file: {inputPath}",
                ex
            );
        }
    }

    public void CheckFFprobeVersion()
    {
        try
        {
            var process = System.Diagnostics.Process.Start("ffprobe", "-version");
            process.WaitForExit();
            
            if (process.ExitCode != 0)
            {
                throw new ProcessExecutionException(
                    "FFprobe failed to execute.",
                    "ffprobe",
                    "-version",
                    process.ExitCode,
                    process.StandardError.ReadToEnd()
                );
            }
        }
        catch (Exception ex) when (ex is ProcessExecutionException)
        {
            // Handle process execution failure
            throw;
        }
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

## CoubVideoExtensions

The `CoubVideoExtensions` static class provides utility methods for analyzing and working with Coub video properties. It includes methods for calculating aspect ratios, determining video orientation and quality, formatting view counts, calculating required audio durations for looping, and estimating output file sizes. These extension methods help standardize video analysis and processing across the application.



### Usage Example

```csharp
using System;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Extensions;

public class CoubVideoExtensionsDemo
{
public void AnalyzeVideo(CoubVideo video)
{
// Get video aspect ratio
var aspectRatio = video.GetAspectRatio();
Console.WriteLine($"Aspect ratio: {aspectRatio:F2}");

// Check video orientation
if (video.IsVerticalFormat())
{
Console.WriteLine("Video is in vertical/portrait format");
}

// Check video quality
if (video.IsHdQuality())
{
Console.WriteLine("Video is HD quality (720p or higher)");
}

if (video.Is4kQuality())
{
Console.WriteLine("Video is 4K quality");
}

// Get formatted view count
var viewsText = video.GetFormattedViewCount();
Console.WriteLine($"Views: {viewsText}");

// Calculate required audio duration for looping
var requiredAudioDuration = CoubVideoExtensions.CalculateRequiredAudioDuration(video);
Console.WriteLine($"Required audio duration: {requiredAudioDuration}s");

// Get duration category
var durationCategory = video.GetDurationCategory();
Console.WriteLine($"Duration category: {durationCategory}");

// Get audio specifications
var audioSpec = CoubVideoExtensions.GetAudioSpec(video);
Console.WriteLine($"Audio: {audioSpec}");

// Calculate looped duration
var loopedDuration = CoubVideoExtensions.CalculateLoopedDuration(video);
Console.WriteLine($"Looped duration: {loopedDuration}s");

// Check audio channel configuration
if (CoubVideoExtensions.IsStereo(video))
{
Console.WriteLine("Audio is stereo");
}

if (CoubVideoExtensions.IsMono(video))
{
Console.WriteLine("Audio is mono");
}

// Get FFmpeg codec parameters
var codecParams = CoubVideoExtensions.GetFFmpegCodecParams(video);
Console.WriteLine($"FFmpeg codec params: {codecParams}");

// Estimate output file size
var estimatedSize = CoubVideoExtensions.EstimateOutputSize(video);
Console.WriteLine($"Estimated output size: {estimatedSize:N0} bytes");

// Check if hardware acceleration should be used
if (CoubVideoExtensions.ShouldUseHardwareAcceleration(video))
{
Console.WriteLine("Hardware acceleration recommended for this video");
}
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