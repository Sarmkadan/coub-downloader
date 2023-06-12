
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