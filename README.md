## InMemoryCoubVideoRepository

`InMemoryCoubVideoRepository` provides an in-memory implementation of `ICoubVideoRepository` for managing Coub video entities. It stores videos in a thread-safe dictionary and supports all standard CRUD operations along with specialized queries for finding videos by URL, creator name, title search, and view count ranges. This implementation is ideal for testing, development, or scenarios where persistence is not required.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Repositories;
using CoubDownloader.Domain.Models;

// Example 1: Basic usage with dependency injection
public class VideoCatalogService
{
    private readonly InMemoryCoubVideoRepository _repository;

    public VideoCatalogService(InMemoryCoubVideoRepository repository)
    {
        _repository = repository;
    }

    public async Task ManageVideoCatalogAsync()
    {
        // Create a new coub video
        var video = new CoubVideo
        {
            Title = "Funny Cat Compilation",
            Url = "https://coub.com/view/12345",
            CreatorName = "cat_lover",
            ViewCount = 15000,
            Tags = new List<string> { "cat", "funny", "compilation" },
            DurationSeconds = 45,
            CreatedAt = DateTime.UtcNow
        };

        var createdVideo = await _repository.CreateAsync(video);
        Console.WriteLine($"Created video with ID: {createdVideo.Id}");

        // Get video by ID
        var fetchedVideo = await _repository.GetByIdAsync(createdVideo.Id);
        if (fetchedVideo != null)
        {
            Console.WriteLine($"Fetched video: {fetchedVideo.Title}");
        }

        // Get all videos
        var allVideos = await _repository.GetAllAsync();
        Console.WriteLine($"Total videos: {allVideos.Count()}");

        // Search by title
        var searchResults = await _repository.SearchByTitleAsync("funny");
        Console.WriteLine($"Videos matching 'funny': {searchResults.Count()}");

        // Get by creator
        var creatorVideos = await _repository.GetByCreatorAsync("cat");
        Console.WriteLine($"Videos by creator 'cat_lover': {creatorVideos.Count()}");

        // Get by view count range
        var popularVideos = await _repository.GetByViewCountRangeAsync(10000, 50000);
        Console.WriteLine($"Videos with 10k-50k views: {popularVideos.Count()}");

        // Update video
        fetchedVideo.Title = "Updated: Funny Cat Compilation";
        var updatedVideo = await _repository.UpdateAsync(fetchedVideo);

        // Check if video exists
        var exists = await _repository.ExistsAsync(updatedVideo.Id);
        Console.WriteLine($"Video exists: {exists}");

        // Get video by URL
        var urlVideo = await _repository.GetByUrlAsync("https://coub.com/view/12345");
        Console.WriteLine($"Video found by URL: {urlVideo?.Title}");

        // Delete video
        var deleted = await _repository.DeleteAsync(updatedVideo.Id);
        Console.WriteLine($"Video deleted: {deleted}");
    }
}

// Example 2: Manual instantiation for testing
var repository = new InMemoryCoubVideoRepository();

// Create test videos
var video1 = new CoubVideo
{
    Title = "Test Video 1",
    Url = "https://coub.com/view/test1",
    CreatorName = "test_creator",
    ViewCount = 1000,
    Tags = new List<string> { "test" }
};

var video2 = new CoubVideo
{
    Title = "Test Video 2",
    Url = "https://coub.com/view/test2",
    CreatorName = "another_creator",
    ViewCount = 2500,
    Tags = new List<string> { "demo", "test" }
};

// Add videos
var created1 = await repository.CreateAsync(video1);
var created2 = await repository.CreateAsync(video2);

// Get all and filter
var all = await repository.GetAllAsync();
var byCreator = await repository.GetByCreatorAsync("test");
var byViews = await repository.GetByViewCountRangeAsync(500, 2000);

// Update and delete
created1.Title = "Updated Test Video 1";
var updated = await repository.UpdateAsync(created1);
var deleted = await repository.DeleteAsync(updated.Id);
```

## InMemoryDownloadTaskRepository

`InMemoryDownloadTaskRepository` provides an in‑memory implementation of `IDownloadTaskRepository` for development and testing. It stores `DownloadTask` entities in a thread‑safe dictionary and offers full CRUD operations together with queries for video ID, processing state, batch job ID, and for retrieving pending or retryable tasks.

### Usage Example

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using CoubDownloader.Infrastructure.Repositories;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

public class DownloadTaskDemo
{
    private readonly InMemoryDownloadTaskRepository _repo = new();

    public async Task RunAsync()
    {
        // Create a new task
        var task = new DownloadTask
        {
            VideoId = "vid123",
            BatchJobId = "batch01",
            State = ProcessingState.Pending,
            ProgressPercent = 0
        };

        var created = await _repo.CreateAsync(task);
        Console.WriteLine($"Created task Id: {created.Id}");

        // Update state and progress
        await _repo.UpdateStateAsync(created.Id, ProcessingState.Downloading);
        await _repo.UpdateProgressAsync(created.Id, 45);

        // Query by state
        var pending = await _repo.GetPendingTasksAsync();
        Console.WriteLine($"Pending tasks count: {pending.Count()}");

        // Check existence
        bool exists = await _repo.ExistsAsync(created.Id);
        Console.WriteLine($"Task exists: {exists}");

        // Delete the task
        bool deleted = await _repo.DeleteAsync(created.Id);
        Console.WriteLine($"Task deleted: {deleted}");
    }
}

// In a real program you would call:
// await new DownloadTaskDemo().RunAsync();
```

## InMemoryDownloadResultRepository

`InMemoryDownloadResultRepository` provides an in-memory implementation of `IDownloadResultRepository` for managing download result entities. It stores results in a thread-safe dictionary and supports all standard CRUD operations along with specialized queries for retrieving results by task ID, filtering by success/failure status, and searching within processing time ranges. This implementation is ideal for testing, development, or scenarios where persistence is not required.


### Usage Example

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using CoubDownloader.Infrastructure.Repositories;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

public class DownloadResultService
{
    private readonly InMemoryDownloadResultRepository _repository;

    public DownloadResultService(InMemoryDownloadResultRepository repository)
    {
        _repository = repository;
    }

    public async Task ManageDownloadResultsAsync()
    {
        // Create a successful download result
        var successResult = new DownloadResult
        {
            TaskId = "task-001",
            Success = true,
            OutputFilePath = "/data/videos/output.mp4",
            OutputFileSizeBytes = 15728640, // 15 MB
            ProcessingTimeMs = 2500,
            Format = VideoFormat.Mp4,
            Quality = VideoQuality.High,
            VideoMetadata = "{\"duration\": 45, \"fps\": 30}",
            CompletedAt = DateTime.UtcNow
        };

        var createdResult = await _repository.CreateAsync(successResult);
        Console.WriteLine($"Created result with ID: {createdResult.Id}");

        // Create a failed download result
        var failedResult = new DownloadResult
        {
            TaskId = "task-002",
            Success = false,
            ErrorMessage = "Network timeout",
            ErrorType = "NetworkError",
            ErrorStackTrace = "at System.Net.Http.HttpClient.SendAsync...",
            ProcessingTimeMs = 1200,
            CompletedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(failedResult);

        // Get result by ID
        var fetchedResult = await _repository.GetByIdAsync(createdResult.Id);
        if (fetchedResult != null)
        {
            Console.WriteLine($"Fetched result: Status={fetchedResult.GetStatusMessage()}");
        }

        // Get all results
        var allResults = await _repository.GetAllAsync();
        Console.WriteLine($"Total results: {allResults.Count()}");

        // Get result by task ID
        var byTaskId = await _repository.GetByTaskIdAsync("task-001");
        Console.WriteLine($"Result for task-001: {byTaskId?.GetStatusMessage()}");

        // Get successful results
        var successfulResults = await _repository.GetSuccessfulResultsAsync();
        Console.WriteLine($"Successful results: {successfulResults.Count()}");

        // Get failed results
        var failedResults = await _repository.GetFailedResultsAsync();
        Console.WriteLine($"Failed results: {failedResults.Count()}");

        // Get results by processing time range (1000ms to 3000ms)
        var timeRangeResults = await _repository.GetByProcessingTimeRangeAsync(1000, 3000);
        Console.WriteLine($"Results in time range: {timeRangeResults.Count()}");

        // Check if result exists
        var exists = await _repository.ExistsAsync(createdResult.Id);
        Console.WriteLine($"Result exists: {exists}");

        // Update result
        fetchedResult.OutputFileSizeBytes = 16256896; // Updated size
        var updatedResult = await _repository.UpdateAsync(fetchedResult);
        Console.WriteLine($"Updated result size: {updatedResult.OutputFileSizeBytes} bytes");

        // Delete result
        var deleted = await _repository.DeleteAsync(updatedResult.Id);
        Console.WriteLine($"Result deleted: {deleted}");
    }
}

// Example 2: Manual instantiation for testing
var repository = new InMemoryDownloadResultRepository();

// Create test results
var result1 = new DownloadResult
{
    TaskId = "test-task-1",
    Success = true,
    OutputFilePath = "/tmp/video1.mp4",
    OutputFileSizeBytes = 10485760,
    ProcessingTimeMs = 1500,
    Format = VideoFormat.Mp4,
    Quality = VideoQuality.Medium
};

var result2 = new DownloadResult
{
    TaskId = "test-task-2",
    Success = false,
    ErrorMessage = "Invalid URL format",
    ErrorType = "ValidationError",
    ProcessingTimeMs = 800
};

// Add results
var created1 = await repository.CreateAsync(result1);
var created2 = await repository.CreateAsync(result2);

// Get all and filter
var all = await repository.GetAllAsync();
var successful = await repository.GetSuccessfulResultsAsync();
var failed = await repository.GetFailedResultsAsync();
var byTask = await repository.GetByTaskIdAsync("test-task-1");
var byTimeRange = await repository.GetByProcessingTimeRangeAsync(500, 2000);

// Update and delete
created1.OutputFileSizeBytes = 11010048;
var updated = await repository.UpdateAsync(created1);
var deleted = await repository.DeleteAsync(updated.Id);
```
