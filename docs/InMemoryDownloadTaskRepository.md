# InMemoryDownloadTaskRepository

An in-memory implementation of a repository for managing `DownloadTask` entities, primarily used for testing or lightweight scenarios where persistence is not required. It provides basic CRUD operations and specialized queries for download tasks, including filtering by video ID, state, batch ID, and task status.

## API

### `Task<DownloadTask?> GetByIdAsync(Guid id)`
Retrieves a download task by its unique identifier.
- **Parameters**: `id` – The GUID of the download task to retrieve.
- **Returns**: A `Task` resolving to the `DownloadTask` if found, or `null` if not found.
- **Exceptions**: Throws `ArgumentException` if `id` is `Guid.Empty`.

### `Task<IEnumerable<DownloadTask>> GetAllAsync()`
Retrieves all download tasks stored in memory.
- **Returns**: A `Task` resolving to an `IEnumerable<DownloadTask>` containing all tasks.

### `Task<DownloadTask> CreateAsync(DownloadTask task)`
Adds a new download task to the in-memory store.
- **Parameters**: `task` – The `DownloadTask` to create.
- **Returns**: A `Task` resolving to the created `DownloadTask`.
- **Exceptions**: Throws `ArgumentNullException` if `task` is `null`.

### `Task<DownloadTask> UpdateAsync(DownloadTask task)`
Updates an existing download task in the in-memory store.
- **Parameters**: `task` – The `DownloadTask` with updated properties.
- **Returns**: A `Task` resolving to the updated `DownloadTask`.
- **Exceptions**: Throws `ArgumentNullException` if `task` is `null`; throws `KeyNotFoundException` if the task ID does not exist.

### `Task<bool> DeleteAsync(Guid id)`
Removes a download task from the in-memory store by its ID.
- **Parameters**: `id` – The GUID of the download task to delete.
- **Returns**: A `Task` resolving to `true` if the task was found and deleted, `false` otherwise.
- **Exceptions**: Throws `ArgumentException` if `id` is `Guid.Empty`.

### `Task<bool> ExistsAsync(Guid id)`
Checks whether a download task with the specified ID exists in the store.
- **Parameters**: `id` – The GUID of the download task to check.
- **Returns**: A `Task` resolving to `true` if the task exists, `false` otherwise.
- **Exceptions**: Throws `ArgumentException` if `id` is `Guid.Empty`.

### `Task<IEnumerable<DownloadTask>> GetByVideoIdAsync(string videoId)`
Retrieves all download tasks associated with a specific video ID.
- **Parameters**: `videoId` – The unique identifier of the video.
- **Returns**: A `Task` resolving to an `IEnumerable<DownloadTask>` of matching tasks.
- **Exceptions**: Throws `ArgumentException` if `videoId` is `null` or whitespace.

### `Task<IEnumerable<DownloadTask>> GetByStateAsync(DownloadTaskState state)`
Retrieves all download tasks in a specific state.
- **Parameters**: `state` – The `DownloadTaskState` to filter by.
- **Returns**: A `Task` resolving to an `IEnumerable<DownloadTask>` of tasks in the specified state.

### `Task<IEnumerable<DownloadTask>> GetByBatchIdAsync(Guid batchId)`
Retrieves all download tasks associated with a specific batch ID.
- **Parameters**: `batchId` – The GUID of the batch.
- **Returns**: A `Task` resolving to an `IEnumerable<DownloadTask>` of matching tasks.
- **Exceptions**: Throws `ArgumentException` if `batchId` is `Guid.Empty`.

### `Task<IEnumerable<DownloadTask>> GetPendingTasksAsync()`
Retrieves all download tasks that are currently pending execution.
- **Returns**: A `Task` resolving to an `IEnumerable<DownloadTask>` of pending tasks.

### `Task<IEnumerable<DownloadTask>> GetRetryableTasksAsync()`
Retrieves all download tasks that are eligible for retry based on their state and retry policy.
- **Returns**: A `Task` resolving to an `IEnumerable<DownloadTask>` of retryable tasks.

### `Task UpdateProgressAsync(Guid id, double progress)`
Updates the progress percentage of a download task.
- **Parameters**:
  - `id` – The GUID of the download task.
  - `progress` – The progress value between `0.0` and `1.0`.
- **Exceptions**: Throws `ArgumentException` if `id` is `Guid.Empty`; throws `ArgumentOutOfRangeException` if `progress` is outside `[0.0, 1.0]`; throws `KeyNotFoundException` if the task ID does not exist.

### `Task UpdateStateAsync(Guid id, DownloadTaskState state)`
Updates the state of a download task.
- **Parameters**:
  - `id` – The GUID of the download task.
  - `state` – The new `DownloadTaskState` to set.
- **Exceptions**: Throws `ArgumentException` if `id` is `Guid.Empty`; throws `KeyNotFoundException` if the task ID does not exist.

## Usage
