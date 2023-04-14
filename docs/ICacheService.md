# ICacheService

A service interface for managing in-memory and distributed cache operations, providing both local and remote caching capabilities with statistics tracking.

## API

### Properties

#### `Value`
- **Purpose**: Gets or sets the cached value.
- **Type**: `object?`
- **Remarks**: The value is stored with its expiration and access tracking managed by the cache service.

#### `ExpirationTime`
- **Purpose**: Gets or sets the absolute expiration time for the cached value.
- **Type**: `DateTime`
- **Remarks**: When this time is reached, the value is considered expired and may be evicted.

#### `CreatedAt`
- **Purpose**: Gets the timestamp when the cache entry was created.
- **Type**: `DateTime`
- **Remarks**: Read-only; set automatically when the entry is added.

#### `AccessCount`
- **Purpose**: Gets the number of times the cached value has been accessed.
- **Type**: `int`
- **Remarks**: Incremented on each successful retrieval via `Get` or `TryGet`.

### Methods

#### `Set<T>(T value, DateTime? expirationTime = null)`
- **Purpose**: Stores a value in the cache with an optional expiration time.
- **Parameters**:
  - `value` (`T`): The value to cache.
  - `expirationTime` (`DateTime?`, optional): The absolute expiration time. If `null`, the default expiration policy applies.
- **Return Value**: None.
- **Remarks**: Overwrites existing entries with the same key. Thread-safe.

#### `TryGet<T>(out T? value)`
- **Purpose**: Attempts to retrieve a cached value without throwing if the key is missing or expired.
- **Parameters**:
  - `value` (`out T?`): Receives the cached value if found and valid.
- **Return Value**: `bool` indicating whether the value was found and is not expired.
- **Remarks**: Increments `AccessCount` on success. Returns `false` for missing or expired entries.

#### `Get<T>()`
- **Purpose**: Retrieves a cached value, throwing if the key is missing or expired.
- **Type Parameters**: `T` – The expected type of the cached value.
- **Return Value**: `T?` – The cached value, or `null` if expired or missing.
- **Throws**: `InvalidOperationException` if the cached value cannot be cast to `T`.
- **Remarks**: Increments `AccessCount` on success. Not safe for concurrent access without synchronization.

#### `Remove()`
- **Purpose**: Removes the current cache entry from the service.
- **Return Value**: None.
- **Remarks**: Safe to call even if the entry does not exist.

#### `Clear()`
- **Purpose**: Removes all entries from the cache.
- **Return Value**: None.
- **Remarks**: Affects the entire cache, not just the current entry.

#### `GetStatistics()`
- **Purpose**: Retrieves aggregated cache usage statistics.
- **Return Value**: `CacheStatistics` – An object containing hit/miss counts, hit rate, size, and entry count.
- **Remarks**: Statistics are updated atomically and reflect global cache state.

### Statistics Properties

#### `TotalEntries`
- **Purpose**: Gets the total number of entries currently in the cache.
- **Type**: `int`
- **Remarks**: Read-only; reflects the current size of the cache.

#### `Hits`
- **Purpose**: Gets the total number of successful cache retrievals.
- **Type**: `int`
- **Remarks**: Read-only; updated on each successful `Get` or `TryGet`.

#### `Misses`
- **Purpose**: Gets the total number of failed cache retrievals.
- **Type**: `int`
- **Remarks**: Read-only; includes misses due to missing keys or expired entries.

#### `HitRate`
- **Purpose**: Gets the ratio of cache hits to total retrieval attempts.
- **Type**: `double`
- **Remarks**: Read-only; calculated as `Hits / (Hits + Misses)`. Returns `0.0` if no attempts have been made.

#### `Size`
- **Purpose**: Gets the estimated total size of all cached values in bytes.
- **Type**: `long`
- **Remarks**: Read-only; may be approximate depending on serialization and storage.

### Distributed Cache Integration

#### `MemoryCacheService`
- **Purpose**: Gets the underlying in-memory cache instance.
- **Type**: `MemoryCacheService`
- **Remarks**: Read-only; provides direct access to local cache operations.

#### `DistributedCacheAdapter`
- **Purpose**: Gets the adapter for distributed cache operations.
- **Type**: `DistributedCacheAdapter`
- **Remarks**: Read-only; enables interaction with remote cache backends.

#### `AddRemoteCache()`
- **Purpose**: Registers a distributed cache backend for remote storage.
- **Return Value**: None.
- **Remarks**: Must be called before using distributed operations. Thread-safe during initialization.

## Usage

### Example 1: Basic Local Cache Usage
