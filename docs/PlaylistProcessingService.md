# PlaylistProcessingService

The `PlaylistProcessingService` is a core component within the `coub-downloader` project responsible for orchestrating the retrieval and asynchronous processing of Coub playlists. It acts as the interface between high-level playlist requests and the background job system, allowing users to either fetch playlist metadata immediately or queue the entire playlist for batched video downloading. This service integrates with the .NET dependency injection container to manage job lifecycle and concurrency.

## API

### Constructor

```csharp
public PlaylistProcessingService(...)
```
Initializes a new instance of the `PlaylistProcessingService`. Dependencies are injected via the constructor to facilitate interaction with the Coub API and the internal job queue system.

### FetchPlaylistAsync

```csharp
public async Task<CoubPlaylist> FetchPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
```
Retrieves the metadata and item list for a specific Coub playlist without initiating any download jobs.
*   **Parameters**:
    *   `playlistId`: The unique identifier or URL slug of the target Coub playlist.
    *   `cancellationToken`: A token to cancel the HTTP request operation.
*   **Returns**: A `CoubPlaylist` object containing the playlist title, author, and a collection of associated Coub entries.
*   **Throws**: Throws a network-related exception if the API request fails, or an `ArgumentException` if the `playlistId` format is invalid.

### QueuePlaylistAsync (Overload 1)

```csharp
public async Task<BatchJob> QueuePlaylistAsync(string playlistId, DownloadOptions options, CancellationToken cancellationToken = default)
```
Creates and queues a single batch job to process all videos within the specified playlist according to the provided download options.
*   **Parameters**:
    *   `playlistId`: The unique identifier of the playlist to process.
    *   `options`: Configuration settings defining output format, quality preferences, and destination paths.
    *   `cancellationToken`: A token to cancel the job queuing operation.
*   **Returns**: A `BatchJob` instance representing the newly created job, including its initial status and tracking ID.
*   **Throws**: Throws if the playlist cannot be resolved or if the job queue is unavailable.

### QueuePlaylistAsync (Overload 2)

```csharp
public async Task<BatchJob> QueuePlaylistAsync(CoubPlaylist playlist, DownloadOptions options, CancellationToken cancellationToken = default)
```
Creates and queues a batch job using an already fetched `CoubPlaylist` object, avoiding a redundant API lookup.
*   **Parameters**:
    *   `playlist`: A populated `CoubPlaylist` instance.
    *   `options`: Configuration settings for the download process.
    *   `cancellationToken`: A token to cancel the queuing operation.
*   **Returns**: A `BatchJob` instance representing the scheduled processing task.
*   **Throws**: Throws if the provided `playlist` object is null or contains no valid entries.

### GetActivePlaylistJobsAsync

```csharp
public Task<IEnumerable<BatchJob>> GetActivePlaylistJobsAsync(CancellationToken cancellationToken = default)
```
Retrieves a snapshot of all currently active or pending playlist processing jobs managed by the service.
*   **Parameters**:
    *   `cancellationToken`: A token to cancel the retrieval operation.
*   **Returns**: An enumerable collection of `BatchJob` objects that are not in a terminal state (completed, failed, or cancelled).
*   **Throws**: Generally does not throw unless the internal job store is corrupted or inaccessible.

### AddPlaylistProcessing

```csharp
public static IServiceCollection AddPlaylistProcessing(this IServiceCollection services, Action<PlaylistOptions>? configure = null)
```
An extension method for `IServiceCollection` that registers `PlaylistProcessingService` and its required dependencies into the dependency injection container.
*   **Parameters**:
    *   `services`: The service collection to register against.
    *   `configure`: An optional action to configure playlist-specific options (e.g., concurrency limits, retry policies).
*   **Returns**: The updated `IServiceCollection` for chaining.
*   **Throws**: Throws `ArgumentNullException` if `services` is null.

## Usage

### Example 1: Fetching Metadata and Conditionally Queuing
This example demonstrates retrieving playlist details to verify content before initiating a download job with specific quality settings.

```csharp
public async Task ProcessPlaylistConditionalAsync(PlaylistProcessingService service, string playlistUrl)
{
    // Fetch metadata first
    var playlist = await service.FetchPlaylistAsync(playlistUrl);
    
    if (playlist.Entries.Count > 10)
    {
        Console.WriteLine($"Large playlist detected: {playlist.Title}. Queuing with low priority.");
        
        var options = new DownloadOptions 
        { 
            Quality = VideoQuality.Low, 
            MaxConcurrency = 2 
        };

        // Queue using the fetched object to save an API call
        var job = await service.QueuePlaylistAsync(playlist, options);
        Console.WriteLine($"Job {job.Id} queued successfully.");
    }
    else
    {
        Console.WriteLine("Playlist too small for batch processing.");
    }
}
```

### Example 2: Monitoring Active Jobs
This example shows how to poll for active jobs to display progress or detect stuck tasks.

```csharp
public async Task MonitorActiveJobsAsync(PlaylistProcessingService service)
{
    var activeJobs = await service.GetActivePlaylistJobsAsync();
    
    foreach (var job in activeJobs)
    {
        var progress = job.TotalItems > 0 
            ? $"{job.ProcessedItems}/{job.TotalItems}" 
            : "Pending";
            
        Console.WriteLine($"Job {job.Id}: Status={job.Status}, Progress={progress}");
        
        if (job.ElapsedTime > TimeSpan.FromHours(2) && job.Status == JobStatus.Running)
        {
            Console.WriteLine($"Warning: Job {job.Id} may be stalled.");
        }
    }
}
```

## Notes

*   **Thread Safety**: The `PlaylistProcessingService` is designed to be thread-safe and registered as a singleton or scoped service depending on the DI configuration. Methods like `QueuePlaylistAsync` and `GetActivePlaylistJobsAsync` can be called concurrently from multiple threads without external locking.
*   **Duplicate Queuing**: Calling `QueuePlaylistAsync` multiple times with the same `playlistId` will result in distinct `BatchJob` instances. The service does not inherently deduplicate requests; callers should implement their own logic to prevent redundant jobs if necessary.
*   **Cancellation**: All asynchronous operations accept a `CancellationToken`. Cancelling the token during `QueuePlaylistAsync` will prevent the job from being persisted to the queue, but cancelling during `FetchPlaylistAsync` will simply abort the HTTP request. Cancelling the token does not cancel a job once it has been successfully queued; job cancellation must be performed via the `BatchJob` control interface.
*   **Resource Disposal**: As the service manages background jobs, ensure the application host shuts down gracefully to allow active `BatchJob` instances to complete or checkpoint their state before the process terminates.
