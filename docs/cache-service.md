# CacheService

In-memory cache with per-entry TTL, hit/miss/eviction statistics, and an optional adapter that layers a local cache over one or more remote caches. Defined in `Infrastructure/Caching/CacheService.cs`, namespace `CoubDownloader.Infrastructure.Caching`.

The file contains four public types:

| Type | Kind | Purpose |
|------|------|---------|
| `ICacheService` | interface | Contract for all cache implementations |
| `MemoryCacheService` | class | Thread-safe in-process cache with TTL and statistics |
| `CacheStatistics` | class | Snapshot of counters returned by `GetStatistics()` |
| `DistributedCacheAdapter` | class | Local-first cache that writes through to remote `ICacheService` instances |

In the application, `MemoryCacheService` is registered as a singleton with a 3600-second default TTL (`Infrastructure/DependencyInjectionExtended.cs`) and injected as `ICacheService` into `CoubApiClient` and `DiagnosticsService`.

---

## `ICacheService`

```csharp
public interface ICacheService
{
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    bool TryGet<T>(string key, out T? value);
    T? Get<T>(string key);
    void Remove(string key);
    void Clear();
    CacheStatistics GetStatistics();
}
```

### `Set<T>(string key, T value, TimeSpan? ttl = null)`
- Stores `value` under `key`. An existing entry with the same key is replaced.
- `ttl` is the lifetime of this entry. When `null`, the implementation's default TTL is used.
- Expiration time is computed once at write time as `DateTime.UtcNow + ttl`. Reads do not extend it.

### `bool TryGet<T>(string key, out T? value)`
- Returns `true` and the stored value when the key exists and has not expired.
- Returns `false` and `default` when the key is missing or expired.
- Counts a hit or a miss (see [Statistics](#statistics)).

### `T? Get<T>(string key)`
- Equivalent to `TryGet<T>`, returning `default` instead of signalling a miss.
- Cannot distinguish "missing" from "stored `default` value"; use `TryGet` when that matters.

### `void Remove(string key)`
- Deletes the entry if present. Removing a missing key is a no-op.

### `void Clear()`
- Deletes all entries. Does not reset the hit, miss, or eviction counters.

### `CacheStatistics GetStatistics()`
- Returns a new `CacheStatistics` snapshot. See [Statistics](#statistics).

---

## `MemoryCacheService`

```csharp
public class MemoryCacheService : ICacheService
public MemoryCacheService(int defaultTtlSeconds = 3600)
```

### Constructor
- `defaultTtlSeconds` — default lifetime, in seconds, applied by `Set` when no `ttl` is given. Default `3600` (one hour).

### Concurrency
- All public members take the same private lock object, so individual calls are atomic and safe to call from multiple threads.
- `Get<T>` delegates to `TryGet<T>` and inherits its locking and counting.

### Storage
- Entries are held in a `Dictionary<string, CacheEntry>` with a `Value`, `ExpirationTime`, `CreatedAt`, and an internal `AccessCount`. `CreatedAt` and `AccessCount` are not exposed through the public API.
- Keys are case-sensitive, as with the default `Dictionary<string, …>` comparer.
- `null` is a valid value for reference types.

### Type handling
- `TryGet<T>` casts the stored `object` to `T?`. Requesting a type different from the one stored throws `InvalidCastException`, and the hit counter has already been incremented by that point. Callers must request the same type they stored.

---

## Expiration and eviction

### When an entry expires
An entry is expired when `DateTime.UtcNow > ExpirationTime`. The comparison is strict, so an entry is still readable at the exact expiration instant. A TTL of zero or less produces an entry that is already expired on the next check.

Expiration uses wall-clock UTC (`DateTime.UtcNow`), not a monotonic clock. A system clock change can therefore expire entries early or late.

### Two removal paths

Expired entries are removed in two ways:

1. **Lazy removal on read.** `TryGet` (and therefore `Get`) checks the entry it found. If it has expired, the entry is removed, the call counts as a miss and an eviction, and `false` is returned.
2. **Sweep on write.** Every `Set` calls an internal sweep that scans the whole dictionary and removes all expired entries, incrementing the eviction counter for each one. This cost is O(n) per `Set`.

Nothing else removes expired entries. `Remove`, `Clear`, `TryGet` on a different key, and `GetStatistics` do not sweep.

### Consequences
- `GetStatistics().TotalEntries` and `Size` can include entries that have expired but have not yet been swept or read. They are not a count of live entries.
- A cache that is written to rarely can hold expired entries in memory until the next `Set` or a read of that key.
- Reading a key that has expired is reported as a miss and also as an eviction.

### Explicit removal and clearing
- `Remove(key)` on an existing entry increments the eviction counter, even if that entry had already expired and was not yet swept. Removing a missing key changes no counter.
- `Clear()` removes everything without incrementing the eviction counter. Hit and miss counters are also left unchanged.

### Overwriting
- `Set` on an existing key replaces the entry, including its expiration time and its internal access count. The replacement does not increment the eviction counter.

---

## Statistics

`GetStatistics()` returns a `CacheStatistics` snapshot:

| Property | Type | Meaning |
|----------|------|---------|
| `TotalEntries` | `int` | Number of entries currently in the dictionary (may include expired, unswept entries) |
| `Hits` | `int` | Successful `TryGet` / `Get` calls since construction |
| `Misses` | `int` | `TryGet` / `Get` calls that found no live entry, including expired reads |
| `Evictions` | `int` | Entries removed by expiry sweeps, lazy expiry on read, or `Remove` of an existing key |
| `HitRate` | `double` | `Hits / (Hits + Misses)`, or `0` when there have been no lookups |
| `Size` | `long` | Approximate memory footprint: `string` values count `Length * 2` bytes; `null` counts `0`; any other value counts a flat `1024` |

Notes:
- Counters are cumulative for the lifetime of the instance. They are never reset, including by `Clear()`.
- `Size` is an estimate, not a measurement. It does not account for keys, overhead, or the contents of non-string objects.
- `Size` and `TotalEntries` are computed under the lock on each call, so `GetStatistics()` costs O(n).

---

## `DistributedCacheAdapter`

```csharp
public class DistributedCacheAdapter : ICacheService
public DistributedCacheAdapter(ICacheService localCache)
public void AddRemoteCache(ICacheService remoteCache)
```

A local `ICacheService` is consulted first. Remote caches are added with `AddRemoteCache` and are consulted only on a local miss. The adapter does not itself have a TTL; it passes the caller's `ttl` through to each cache.

### Operations

| Method | Behaviour |
|--------|-----------|
| `Set` | Writes to the local cache, then to every remote cache with the same `ttl`. Exceptions from remotes are swallowed. |
| `TryGet` | Checks local first. On a local miss, tries each remote in order; on the first remote hit, writes the value into the local cache and returns it. Exceptions from remotes are swallowed and the next remote is tried. |
| `Get` | Delegates to `TryGet`. |
| `Remove` | Removes from local, then from every remote. Remote exceptions are swallowed. |
| `Clear` | Clears local, then every remote. Remote exceptions are swallowed. |
| `GetStatistics` | Returns the **local** cache's statistics only. Remote statistics are not included. |

### Behaviour to be aware of
- **TTL is not preserved on read-through.** When a value is copied from a remote into the local cache, the local `Set` is called without a TTL, so the local default TTL applies. The remaining lifetime from the remote entry is lost.
- **Remote failures are silent.** Exceptions from remote `Set`, `TryGet`, `Remove`, and `Clear` are caught and discarded, so a failing remote is indistinguishable from a miss or a no-op.
- **Local exceptions propagate.** Only remote calls are wrapped in `try`/`catch`; exceptions from the local cache are not caught.
- **Failed local writes on read-through are not caught either**, so an exception from `_localCache.Set` during a remote hit propagates out of `TryGet`.
- **`AddRemoteCache` is not synchronized.** The remote list is a plain `List<ICacheService>`. Register all remotes before the adapter is shared between threads.
- **Thread safety depends on the components.** The adapter adds no locking of its own; it is as thread-safe as the local and remote `ICacheService` instances it uses (`MemoryCacheService` is safe).

---

## Example

```csharp
var local = new MemoryCacheService(defaultTtlSeconds: 300);
var cache = new DistributedCacheAdapter(local);
cache.AddRemoteCache(remoteCache); // any ICacheService implementation

cache.Set("coub:abc123", "https://example.com/video.mp4", TimeSpan.FromMinutes(10));

if (cache.TryGet("coub:abc123", out string? url))
{
    // use url
}

var stats = local.GetStatistics();
Console.WriteLine($"hit rate: {stats.HitRate:P0}, entries: {stats.TotalEntries}");
```

---

## Related

- [ICacheService](ICacheService.md) — note: that page describes an older API shape (for example `Set<T>(T value, DateTime?)` and an `AccessCount` property) and does not match the current interface above.
- Tests: `tests/coub-downloader.Tests/CacheServiceTests.cs`
