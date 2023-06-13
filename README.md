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

## StringExtensions

`StringExtensions` provides a set of utility extension methods for common string operations including validation, transformation, and analysis. These methods help standardize and sanitize strings for URLs, file paths, display purposes, and text processing scenarios throughout the application.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Utilities;

// Example 1: URL validation and domain extraction
var url = "https://www.example.com/path/to/resource";
if (url.IsValidUrl())
{
    var domain = url.GetUrlDomain();
    Console.WriteLine($"Domain: {domain}"); // Output: Domain: www.example.com
}

// Example 2: Text formatting for display
var title = "  This is a  Sample  Title  ";
var slug = title.Trim().RemoveDuplicateWhitespace().ToSlug();
Console.WriteLine($"Slug: {slug}"); // Output: Slug: this-is-a-sample-title

var displayTitle = title.Trim().RemoveDuplicateWhitespace().Capitalize();
Console.WriteLine($"Title: {displayTitle}"); // Output: Title: This is a Sample Title

var titleCase = "the quick brown fox".ToTitleCase();
Console.WriteLine($"Title case: {titleCase}"); // Output: Title case: The Quick Brown Fox

// Example 3: String manipulation and analysis
var text = "Hello World Hello";
var count = text.CountOccurrences("Hello");
Console.WriteLine($"Occurrences: {count}"); // Output: Occurrences: 2

var containsAny = text.ContainsAny("World", "Universe");
Console.WriteLine($"Contains any: {containsAny}"); // Output: Contains any: True

var startsWithAny = text.StartsWithAny("Hello", "Hi");
Console.WriteLine($"Starts with any: {startsWithAny}"); // Output: Starts with any: True

// Example 4: String extraction and truncation
var content = "[start]This is the extracted content[end]";
var extracted = content.SubstringBetween("[start]", "[end]");
Console.WriteLine($"Extracted: {extracted}"); // Output: Extracted: This is the extracted content

var longText = "This is a very long text that needs to be shortened for display purposes";
var truncated = longText.Truncate(30);
Console.WriteLine($"Truncated: {truncated}"); // Output: Truncated: This is a very long text...

// Example 5: Case-insensitive replacement
var mixedCase = "Hello hello HELLO";
var replaced = mixedCase.ReplaceIgnoreCase("hello", "Hi");
Console.WriteLine($"Replaced: {replaced}"); // Output: Replaced: Hi Hi Hi

// Example 6: Splitting by multiple separators
var tags = "tag1, tag2; tag3 | tag4";
var tagArray = tags.SplitByMultiple(",", ";", "|");
Console.WriteLine($"Tags: {string.Join(", ", tagArray)}"); // Output: Tags: tag1, tag2, tag3, tag4

// Example 7: Numeric validation
var numberString = "12345";
var isNumeric = numberString.IsNumeric();
Console.WriteLine($"Is numeric: {isNumeric}"); // Output: Is numeric: True
```

## ICacheService

`ICacheService` defines a contract for caching services with time-to-live (TTL) support. It provides methods for storing, retrieving, and managing cached values with automatic expiration. The interface supports both in-memory and distributed caching scenarios through its implementations: `MemoryCacheService` for local caching and `DistributedCacheAdapter` for multi-instance environments.

## DateTimeExtensions

`DateTimeExtensions` provides a comprehensive set of extension methods for `DateTime` and `TimeSpan` types, offering utilities for date manipulation, time formatting, and temporal comparisons. These methods simplify common date operations like calculating start/end of time periods, converting between Unix timestamps and `DateTime`, and generating human-readable relative time strings.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Utilities;

// Example 1: Get human-readable relative time
var pastDate = DateTime.UtcNow.AddHours(-2);
var relativeTime = pastDate.GetRelativeTime();
Console.WriteLine($"Relative time: {relativeTime}"); // Output: Relative time: 2h ago

var futureDate = DateTime.UtcNow.AddDays(1);
var futureRelativeTime = futureDate.GetRelativeTime();
Console.WriteLine($"Future time: {futureRelativeTime}"); // Output: Future time: Jul 16, 2026

// Example 2: Date range checking
var now = DateTime.UtcNow;
var startDate = now.AddDays(-7);
var endDate = now.AddDays(7);
var isWithinRange = now.IsWithinRange(startDate, endDate);
Console.WriteLine($"Is within range: {isWithinRange}"); // Output: Is within range: True

// Example 3: Date period calculations
var today = DateTime.Today;
var startOfDay = today.StartOfDay();
var endOfDay = today.EndOfDay();
Console.WriteLine($"Start of day: {startOfDay:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"End of day: {endOfDay:yyyy-MM-dd HH:mm:ss}");

var startOfWeek = today.StartOfWeek();
var startOfMonth = today.StartOfMonth();
var endOfMonth = today.EndOfMonth();
Console.WriteLine($"Start of week: {startOfWeek:yyyy-MM-dd}");
Console.WriteLine($"Start of month: {startOfMonth:yyyy-MM-dd}");
Console.WriteLine($"End of month: {endOfMonth:yyyy-MM-dd}");

// Example 4: Unix timestamp conversion
var timestamp = now.ToUnixTimestamp();
Console.WriteLine($"Unix timestamp: {timestamp}");
var convertedDate = timestamp.FromUnixTimestamp();
Console.WriteLine($"Converted back: {convertedDate:yyyy-MM-dd HH:mm:ss}");

// Example 5: Today/Yesterday checks
var yesterday = DateTime.Today.AddDays(-1);
Console.WriteLine($"Is today: {today.IsToday()}"); // Output: Is today: True
Console.WriteLine($"Is yesterday: {yesterday.IsYesterday()}"); // Output: Is yesterday: True

// Example 6: TimeSpan formatting
var duration = TimeSpan.FromMinutes(90);
Console.WriteLine($"Duration formatted: {duration.FormatDuration()}"); // Output: Duration formatted: 01:30:00
```
