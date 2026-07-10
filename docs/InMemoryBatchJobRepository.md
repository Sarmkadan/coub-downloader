# InMemoryBatchJobRepository

An in-memory implementation of `IBatchJobRepository` that stores `BatchJob` entities in a `ConcurrentDictionary` for testing and development scenarios where persistence is not required. Provides thread-safe operations for managing batch jobs in memory.

## API

### `Task<BatchJob?> GetByIdAsync(Guid id)`
Retrieves a batch job by its unique identifier. Returns `null` if the job does not exist. No exceptions are thrown for missing keys.

**Parameters:**
- `id` – The unique identifier of the batch job.

**Return value:**
- A `Task` resolving to the `BatchJob` instance if found, otherwise `null`.

---

### `Task<IEnumerable<BatchJob>> GetAllAsync()`
Retrieves all batch jobs stored in memory. The result is an `IEnumerable<BatchJob>` that may be empty if no jobs exist.

**Return value:**
- A `Task` resolving to an enumerable of all `BatchJob` instances.

---

### `Task<BatchJob> CreateAsync(BatchJob job)`
Creates a new batch job in memory. The provided `job` must have a unique `Id`; otherwise, the behavior is undefined. Returns the created job after insertion.

**Parameters:**
- `job` – The `BatchJob` instance to create.

**Return value:**
- A `Task` resolving to the created `BatchJob`.

**Exceptions:**
- Throws `ArgumentNullException` if `job` is `null`.
- Throws `ArgumentException` if the `Id` of `job` already exists in the repository.

---

### `Task<BatchJob> UpdateAsync(BatchJob job)`
Updates an existing batch job in memory. The `Id` of the provided `job` must match an existing entry; otherwise, the operation has no effect. Returns the updated job.

**Parameters:**
- `job` – The `BatchJob` instance to update.

**Return value:**
- A `Task` resolving to the updated `BatchJob`.

**Exceptions:**
- Throws `ArgumentNullException` if `job` is `null`.

---
### `Task<bool> DeleteAsync(Guid id)`
Deletes a batch job by its unique identifier. Returns `true` if the job existed and was removed, otherwise `false`.

**Parameters:**
- `id` – The unique identifier of the batch job to delete.

**Return value:**
- A `Task<bool>` indicating whether the deletion was successful.

---
### `Task<bool> ExistsAsync(Guid id)`
Checks whether a batch job with the specified identifier exists in memory.

**Parameters:**
- `id` – The unique identifier of the batch job to check.

**Return value:**
- A `Task<bool>` resolving to `true` if the job exists, otherwise `false`.

---
### `Task<IEnumerable<BatchJob>> GetByStateAsync(BatchJobState state)`
Retrieves all batch jobs matching the specified state. The result may be empty if no jobs match the state.

**Parameters:**
- `state` – The `BatchJobState` to filter by.

**Return value:**
- A `Task` resolving to an enumerable of `BatchJob` instances matching the state.

---
### `Task<IEnumerable<BatchJob>> GetRecentAsync(int count)`
Retrieves the most recently created batch jobs, limited by `count`. The result may be fewer than `count` if fewer jobs exist. Order is not guaranteed.

**Parameters:**
- `count` – The maximum number of jobs to return.

**Return value:**
- A `Task` resolving to an enumerable of the most recent `BatchJob` instances.

**Exceptions:**
- Throws `ArgumentOutOfRangeException` if `count` is negative.

---
### `Task<IEnumerable<BatchJob>> SearchByNameAsync(string namePattern)`
Searches batch jobs by name using a case-insensitive substring match. Returns all jobs where the `Name` property contains the `namePattern` as a substring.

**Parameters:**
- `namePattern` – The substring to search for in job names.

**Return value:**
- A `Task` resolving to an enumerable of `BatchJob` instances matching the search criteria.

**Exceptions:**
- Throws `ArgumentNullException` if `namePattern` is `null`.

---
### `Task UpdateProgressAsync(Guid id, int progress, string? statusMessage = null)`
Updates the progress and optional status message of a batch job by its identifier. Progress is clamped to the range `[0, 100]`.

**Parameters:**
- `id` – The unique identifier of the batch job.
- `progress` – The progress value to set (0–100).
- `statusMessage` – An optional status message to associate with the job.

**Return value:**
- A `Task` representing the asynchronous operation.

**Exceptions:**
- Throws `ArgumentOutOfRangeException` if `progress` is outside `[0, 100]`.
- Throws `ArgumentException` if the `id` does not exist.

## Usage

### Example 1: Creating and retrieving a batch job
