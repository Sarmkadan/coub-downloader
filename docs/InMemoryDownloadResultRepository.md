# InMemoryDownloadResultRepository

An in-memory implementation of `IDownloadResultRepository` that stores `DownloadResult` entities in a concurrent dictionary. Suitable for testing, prototyping, or lightweight scenarios where persistence is not required.

## API

### `Task<DownloadResult?> GetByIdAsync(Guid id)`
Retrieves a download result by its unique identifier.
- **Parameters**: `id` – The GUID of the download result to fetch.
- **Returns**: A task that resolves to the matching `DownloadResult` if found; otherwise, `null`.
- **Exceptions**: Throws `ArgumentNullException` if `id` is `null`.

### `Task<IEnumerable<DownloadResult>> GetAllAsync()`
Returns all stored download results.
- **Returns**: A task that resolves to an enumerable of all `DownloadResult` instances in the repository.
- **Exceptions**: None.

### `Task<DownloadResult> CreateAsync(DownloadResult entity)`
Adds a new download result to the repository.
- **Parameters**: `entity` – The `DownloadResult` to create.
- **Returns**: A task that resolves to the created `DownloadResult`.
- **Exceptions**: Throws `ArgumentNullException` if `entity` is `null`.

### `Task<DownloadResult> UpdateAsync(DownloadResult entity)`
Updates an existing download result.
- **Parameters**: `entity` – The updated `DownloadResult`.
- **Returns**: A task that resolves to the updated `DownloadResult`.
- **Exceptions**: Throws `ArgumentNullException` if `entity` is `null`; throws `KeyNotFoundException` if the entity’s ID does not exist.

### `Task<bool> DeleteAsync(Guid id)`
Removes a download result by its ID.
- **Parameters**: `id` – The GUID of the download result to delete.
- **Returns**: A task that resolves to `true` if the entity was found and deleted; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `id` is `null`.

### `Task<bool> ExistsAsync(Guid id)`
Checks whether a download result with the given ID exists.
- **Parameters**: `id` – The GUID to check.
- **Returns**: A task that resolves to `true` if the entity exists; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `id` is `null`.

### `Task<DownloadResult?> GetByTaskIdAsync(string taskId)`
Finds a download result by its associated task identifier.
- **Parameters**: `taskId` – The task identifier to search for.
- **Returns**: A task that resolves to the matching `DownloadResult` if found; otherwise, `null`.
- **Exceptions**: Throws `ArgumentNullException` if `taskId` is `null`.

### `Task<IEnumerable<DownloadResult>> GetSuccessfulResultsAsync()`
Returns all download results marked as successful.
- **Returns**: A task that resolves to an enumerable of successful `DownloadResult` instances.
- **Exceptions**: None.

### `Task<IEnumerable<DownloadResult>> GetFailedResultsAsync()`
Returns all download results marked as failed.
- **Returns**: A task that resolves to an enumerable of failed `DownloadResult` instances.
- **Exceptions**: None.

### `Task<IEnumerable<DownloadResult>> GetByProcessingTimeRangeAsync(DateTimeOffset start, DateTimeOffset end)`
Returns download results whose processing time falls within the specified range (inclusive).
- **Parameters**:
  - `start` – The start of the processing time range.
  - `end` – The end of the processing time range.
- **Returns**: A task that resolves to an enumerable of `DownloadResult` instances whose `ProcessingTime` lies within the range.
- **Exceptions**: Throws `ArgumentNullException` if `start` or `end` is `null`; throws `ArgumentException` if `start` is later than `end`.

## Usage

### Example 1: Basic CRUD
