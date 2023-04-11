# InMemoryCoubVideoRepository

An in-memory implementation of `ICoubVideoRepository` that stores `CoubVideo` entities in a `ConcurrentDictionary` for testing and development purposes. This repository provides thread-safe operations for managing coub videos without requiring a persistent data store.

## API

### `Task<CoubVideo?> GetByIdAsync(Guid id)`
Retrieves a `CoubVideo` by its unique identifier.
- **Parameters**: `id` – The unique identifier of the video.
- **Returns**: A `Task` resolving to the `CoubVideo` if found, otherwise `null`.
- **Exceptions**: Throws `ArgumentNullException` if `id` is `null`.

### `Task<IEnumerable<CoubVideo>> GetAllAsync()`
Retrieves all stored `CoubVideo` entities.
- **Returns**: A `Task` resolving to an `IEnumerable<CoubVideo>` containing all videos.
- **Exceptions**: Never throws.

### `Task<CoubVideo> CreateAsync(CoubVideo video)`
Adds a new `CoubVideo` to the repository.
- **Parameters**: `video` – The `CoubVideo` to add.
- **Returns**: A `Task` resolving to the created `CoubVideo`.
- **Exceptions**: Throws `ArgumentNullException` if `video` is `null`.

### `Task<CoubVideo> UpdateAsync(CoubVideo video)`
Updates an existing `CoubVideo` in the repository.
- **Parameters**: `video` – The `CoubVideo` with updated properties.
- **Returns**: A `Task` resolving to the updated `CoubVideo`.
- **Exceptions**: Throws `ArgumentNullException` if `video` is `null` or if the video does not exist.

### `Task<bool> DeleteAsync(Guid id)`
Removes a `CoubVideo` by its identifier.
- **Parameters**: `id` – The unique identifier of the video to remove.
- **Returns**: A `Task` resolving to `true` if the video was found and removed, otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `id` is `null`.

### `Task<bool> ExistsAsync(Guid id)`
Checks whether a `CoubVideo` with the given identifier exists.
- **Parameters**: `id` – The unique identifier to check.
- **Returns**: A `Task` resolving to `true` if the video exists, otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `id` is `null`.

### `Task<CoubVideo?> GetByUrlAsync(string url)`
Retrieves a `CoubVideo` by its URL.
- **Parameters**: `url` – The URL of the video.
- **Returns**: A `Task` resolving to the `CoubVideo` if found, otherwise `null`.
- **Exceptions**: Throws `ArgumentNullException` if `url` is `null`.

### `Task<IEnumerable<CoubVideo>> GetByCreatorAsync(string creatorName)`
Retrieves all `CoubVideo` entities created by a specific creator.
- **Parameters**: `creatorName` – The name of the creator.
- **Returns**: A `Task` resolving to an `IEnumerable<CoubVideo>` containing matching videos.
- **Exceptions**: Throws `ArgumentNullException` if `creatorName` is `null`.

### `Task<IEnumerable<CoubVideo>> SearchByTitleAsync(string titlePart)`
Searches for `CoubVideo` entities whose titles contain the specified substring.
- **Parameters**: `titlePart` – The substring to search for in titles.
- **Returns**: A `Task` resolving to an `IEnumerable<CoubVideo>` containing matching videos.
- **Exceptions**: Throws `ArgumentNullException` if `titlePart` is `null`.

### `Task<IEnumerable<CoubVideo>> GetByViewCountRangeAsync(int minViews, int maxViews)`
Retrieves `CoubVideo` entities whose view counts fall within the specified range (inclusive).
- **Parameters**:
  - `minViews` – The minimum view count (inclusive).
  - `maxViews` – The maximum view count (inclusive).
- **Returns**: A `Task` resolving to an `IEnumerable<CoubVideo>` containing matching videos.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `minViews` is greater than `maxViews`.

## Usage

### Example 1: Basic CRUD Operations
