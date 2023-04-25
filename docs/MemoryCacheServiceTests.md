# MemoryCacheServiceTests

This class contains unit tests for the `MemoryCacheService` class, verifying its core caching operations including set, get, remove, clear, statistics tracking, TTL expiration, overwriting, complex type serialization, and integration with a remote cache (propagation, fallback, and exception handling). The tests ensure correct behavior for both local and remote cache interactions.

## API

### Properties

- **`Name`** (`string`)  
  Gets or sets the name associated with the test instance. This property is used by some tests to configure the cache key or value.

- **`Value`** (`int`)  
  Gets or sets the integer value associated with the test instance. This property is used by some tests to configure the cache value.

### Test Methods

All test methods are parameterless, return `void`, and throw an `AssertionException` (or equivalent) if the expected behavior is not observed.

- **`Set_ThenGet_ReturnsStoredValue`**  
  Verifies that a value stored via `Set` can be retrieved via `Get` with the same key.

- **`TryGet_ExistingKey_ReturnsTrueAndValue`**  
  Verifies that `TryGet` returns `true` and the correct value for an existing key.

- **`TryGet_MissingKey_ReturnsFalseAndDefault`**  
  Verifies that `TryGet` returns `false` and the default value for a key that has not been set.

- **`Remove_ExistingKey_KeyNoLongerRetrievable`**  
  Verifies that after calling `Remove` on an existing key, subsequent `Get` or `TryGet` calls indicate the key is absent.

- **`Clear_AfterMultipleSets_CacheIsEmpty`**  
  Verifies that after calling `Clear`, all previously stored entries are removed.

- **`GetStatistics_AfterHitsAndMisses_TracksAccurately`**  
  Verifies that the cache statistics (hit count, miss count, hit rate) reflect the actual sequence of `Get`/`TryGet` operations.

- **`GetStatistics_EmptyCache_HitRateIsZero`**  
  Verifies that on an empty cache, the hit rate is zero and miss count equals the number of lookups.

- **`Set_ExpiredTtl_EntryNotRetrievable`**  
  Verifies that an entry set with a TTL that has already expired is not retrievable.

- **`Set_OverwritesExistingKey`**  
  Verifies that calling `Set` on an existing key replaces the previous value.

- **`TryGet_ComplexType_DeserializesCorrectly`**  
  Verifies that a complex object stored via `Set` is correctly deserialized when retrieved via `TryGet`.

- **`Set_PropagatesValueToRemoteCache`**  
  Verifies that a value set locally is also propagated to the remote cache (if configured).

- **`TryGet_HitOnLocal_DoesNotQueryRemote`**  
  Verifies that when a key is found in the local cache, the remote cache is not queried.

- **`TryGet_LocalMissRemoteHit_CachesLocallyAndReturnsValue`**  
  Verifies that when a key is missing locally but present remotely, the value is fetched from remote, cached locally, and returned.

- **`Remove_PropagatesDeletionToRemoteCache`**  
  Verifies that removing a key locally also removes it from the remote cache.

- **`Clear_PropagatesClearToRemoteCache`**  
  Verifies that clearing the local cache also clears the remote cache.

- **`Set_RemoteThrows_DoesNotBubbleException`**  
  Verifies that if the remote cache throws an exception during `Set`, the exception is caught and not propagated to the caller.

## Usage

The following examples demonstrate how to use the `MemoryCacheServiceTests` class in a test project.

### Example 1: Programmatic invocation for debugging

```csharp
var tests = new MemoryCacheServiceTests
{
    Name = "testKey",
    Value = 42
};

try
{
    tests.Set_ThenGet_ReturnsStoredValue();
    Console.WriteLine("Test passed.");
}
catch (AssertionException ex)
{
    Console.WriteLine($"Test failed: {ex.Message}");
}
```

### Example 2: Data-driven test using xUnit

```csharp
public class CacheTestSuite
{
    [Theory]
    [InlineData("key1", 100)]
    [InlineData("key2", 200)]
    public void RunSetAndGetTest(string name, int value)
    {
        var tests = new MemoryCacheServiceTests
        {
            Name = name,
            Value = value
        };
        tests.Set_ThenGet_ReturnsStoredValue();
    }
}
```

## Notes

- **Edge cases**  
  - TTL expiration: Tests ensure that entries with expired TTL are treated as missing, even if they exist in the cache store.  
  - Overwriting: Setting a new value for an existing key replaces the previous value without error.  
  - Missing keys: `TryGet` returns `false` and the default value for keys that were never set or have been removed.  
  - Complex types: Serialization/deserialization of complex objects must preserve object equality and structure.  
  - Remote cache failures: The `Set_RemoteThrows_DoesNotBubbleException` test verifies that local cache operations remain unaffected when the remote cache is unavailable or throws.

- **Thread-safety**  
  - The test class itself is not thread-safe; its `Name` and `Value` properties are intended to be set before test execution and should not be modified concurrently.  
  - The underlying `MemoryCacheService` is assumed to be thread-safe, but these tests are designed for single-threaded execution. Concurrent access patterns are not covered.  
  - Remote cache propagation tests rely on mock or stub implementations; actual remote cache behavior may differ under concurrency.
