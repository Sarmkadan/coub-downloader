# InMemoryDownloadTaskRepositoryExtensions
The `InMemoryDownloadTaskRepositoryExtensions` class provides a set of extension methods for working with in-memory download task repositories. It offers methods to retrieve download tasks based on various criteria and update their states, allowing for efficient management of download tasks in memory.

## API
* `GetByVideoIdsAsync`: Retrieves a collection of download tasks associated with the specified video IDs. This method takes no parameters and returns an `IEnumerable<DownloadTask>`. It throws an exception if an error occurs during the retrieval process.
* `GetByBatchJobIdsAsync`: Retrieves a collection of download tasks associated with the specified batch job IDs. This method takes no parameters and returns an `IEnumerable<DownloadTask>`. It throws an exception if an error occurs during the retrieval process.
* `GetByStatesAsync`: Retrieves a collection of download tasks with the specified states. This method takes no parameters and returns an `IEnumerable<DownloadTask>`. It throws an exception if an error occurs during the retrieval process.
* `UpdateStatesAsync`: Updates the states of the specified download tasks. This method takes no parameters and returns an `int` representing the number of tasks updated. It throws an exception if an error occurs during the update process.

## Usage
```csharp
// Example 1: Retrieving download tasks by video IDs
var videoIds = new[] { "video1", "video2" };
var downloadTasks = await InMemoryDownloadTaskRepositoryExtensions.GetByVideoIdsAsync();
foreach (var task in downloadTasks)
{
    Console.WriteLine($"Video ID: {task.VideoId}, State: {task.State}");
}

// Example 2: Updating download task states
var updatedCount = await InMemoryDownloadTaskRepositoryExtensions.UpdateStatesAsync();
Console.WriteLine($"Updated {updatedCount} download tasks");
```

## Notes
When using the `InMemoryDownloadTaskRepositoryExtensions` class, consider the following edge cases:
* If no download tasks match the specified criteria, the `GetByVideoIdsAsync`, `GetByBatchJobIdsAsync`, and `GetByStatesAsync` methods will return an empty collection.
* The `UpdateStatesAsync` method will only update the states of download tasks that exist in the in-memory repository.
* This class is designed for use in a single-threaded or synchronized multi-threaded environment, as the in-memory repository is not inherently thread-safe. If used in a multi-threaded environment without proper synchronization, data corruption or inconsistencies may occur.
