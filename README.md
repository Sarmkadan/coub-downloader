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

## VersionHelper

`VersionHelper` provides utilities for retrieving application version information, runtime details, and performing version comparisons. It helps ensure compatibility checks and provides build metadata throughout the application lifecycle.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Utilities;

// Example 1: Get basic version information
var appVersion = VersionHelper.GetApplicationVersion();
var runtimeVersion = VersionHelper.GetRuntimeVersion();
var osInfo = VersionHelper.GetOperatingSystem();
var buildDate = VersionHelper.GetBuildDate();

Console.WriteLine($"Application Version: {appVersion}");
Console.WriteLine($"Runtime Version: {runtimeVersion}");
Console.WriteLine($"OS: {osInfo}");
Console.WriteLine($"Build Date: {buildDate:yyyy-MM-dd HH:mm:ss}");

// Example 2: Get full application information
var appInfo = VersionHelper.GetApplicationInfo();
Console.WriteLine(appInfo.ToString());

// Example 3: Version comparison
var currentVersion = "1.2.3";
var requiredVersion = "1.2.0";

var isGreater = VersionHelper.IsGreaterThan(currentVersion, requiredVersion);
Console.WriteLine($"Is current version greater than required: {isGreater}");

var isUpdateAvailable = VersionHelper.IsUpdateAvailable(currentVersion, "1.3.0");
Console.WriteLine($"Update available: {isUpdateAvailable}");

// Example 4: Compare versions directly
var comparisonResult = VersionHelper.CompareVersions("2.0.0", "1.9.9");
Console.WriteLine($"Version comparison result: {comparisonResult}"); // Positive if 2.0.0 > 1.9.9
```

## ObjectPool

`ObjectPool<T>` provides a generic object pooling mechanism for reusing expensive resources efficiently. It maintains a pool of reusable objects, reducing the overhead of frequent object creation and garbage collection. The pool supports both synchronous and asynchronous resource management patterns through the `ObjectPool<T>` and `ConnectionPool` classes, with optional reset functionality for pooled objects.


### Usage Example

```csharp
using CoubDownloader.Infrastructure.Utilities;

// Example 1: Basic object pool for database connections
var connectionFactory = () => new DatabaseConnection();
var pool = new ObjectPool<DatabaseConnection>(connectionFactory, maxPoolSize: 5);

// Rent a connection from the pool
using var pooledObject = new PooledObject<DatabaseConnection>(pool);
var connection = pooledObject.Object;

// Use the connection
connection.Execute("SELECT * FROM users");

// Connection is automatically returned to the pool when PooledObject.Dispose() is called

// Example 2: Connection pool with async operations
var connectionPool = new ConnectionPool(async () => 
{
    var conn = new DatabaseConnection();
    await conn.OpenAsync();
    return new ConnectionHandle { IsOpen = true };
}, maxConnections: 10);

// Acquire a connection asynchronously
var connectionHandle = await connectionPool.AcquireAsync();
try
{
    // Use the connection
    await connectionHandle.ExecuteAsync("SELECT * FROM videos");
}
finally
{
    // Release the connection back to the pool
    connectionPool.Release(connectionHandle);
}

// Example 3: Pool with reset functionality
var expensiveObjectFactory = () => new ExpensiveResource();
var resetAction = (ExpensiveResource obj) => obj.Reset();
var resetPool = new ObjectPool<ExpensiveResource>(expensiveObjectFactory, resetAction, maxPoolSize: 3);

// Rent and use objects
var obj1 = resetPool.Rent();
try
{
    obj1.DoWork();
}
finally
{
    resetPool.Return(obj1);
}

// Example 4: Managing pool lifecycle
var lifecyclePool = new ObjectPool<Resource>(() => new Resource(), maxPoolSize: 8);

// Get pool statistics
var available = lifecyclePool.AvailableCount;
var inUse = lifecyclePool.InUseCount;

// Clear the pool when shutting down
lifecyclePool.Clear();

// Example 5: Connection handle properties
var handle = new ConnectionHandle();
Console.WriteLine($"Connection ID: {handle.Id}");
Console.WriteLine($"Created at: {handle.CreatedAt}");
Console.WriteLine($"Is open: {handle.IsOpen}");

// Close all connections in the pool
connectionPool.Close();
```
