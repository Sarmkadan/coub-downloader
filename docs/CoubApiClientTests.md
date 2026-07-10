# CoubApiClientTests

A test class for validating the behavior of the `CoubApiClient` service. It contains unit tests that verify caching logic, successful API interactions, error handling, and edge-case responses for methods that retrieve video information, verify video existence, and search for videos.

## API

### public CoubApiClientTests

The default constructor for the test class. It is parameterless and initializes the test instance, typically used by the test runner to discover and execute the test methods defined within this class.

### public async Task GetVideoInfoAsync_CacheHit_ReturnsCachedInfo

Tests that when video information is already present in the cache, the method returns the cached data without making an external API call. Verifies the fast-path cache retrieval logic.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions propagated from the underlying test assertions or the caching layer if misconfigured.

### public async Task GetVideoInfoAsync_SuccessfulApiCall_ReturnsVideoInfoAndCaches

Tests that a successful API response yields the expected video information object and that the result is correctly stored in the cache for subsequent requests.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures or from the API client if the mocked HTTP handler is not set up correctly.

### public async Task GetVideoInfoAsync_ApiReturnsNotFound_ReturnsNullAndLogsWarning

Tests the scenario where the remote API responds with a 404 status. Expects the method to return `null` and to emit a warning-level log entry without throwing an exception.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures if the null return or logging behavior is not satisfied.

### public async Task GetVideoInfoAsync_HttpRequestException_ReturnsNullAndLogsError

Tests the behavior when an `HttpRequestException` occurs during the API call. Expects the method to gracefully return `null` and log an error, rather than allowing the exception to propagate.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures if the error handling or logging deviates from the expected behavior.

### public async Task GetVideoInfoAsync_InvalidUrl_ReturnsNull

Tests that providing a malformed or invalid URL results in a `null` return value, ensuring the client validates input before attempting a network request.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures if the method does not return `null` for invalid input.

### public async Task VerifyVideoExistsAsync_CacheHit_ReturnsCachedValue

Tests that when a video existence check has been previously cached, the method returns the cached boolean result immediately without invoking the API.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures or cache misconfiguration.

### public async Task VerifyVideoExistsAsync_VideoExists_ReturnsTrueAndCaches

Tests that when the API confirms a video exists, the method returns `true` and stores this positive result in the cache.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures if the return value is not `true` or the caching step is skipped.

### public async Task VerifyVideoExistsAsync_VideoDoesNotExist_ReturnsFalseAndCaches

Tests that when the API indicates a video does not exist, the method returns `false` and caches this negative result to avoid redundant future lookups.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures if the return value is not `false` or the caching step is skipped.

### public async Task SearchVideosAsync_CacheHit_ReturnsCachedList

Tests that a search query whose results are already cached returns the cached list of videos without performing a network request.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures or cache misconfiguration.

### public async Task SearchVideosAsync_SuccessfulApiCall_ReturnsVideosAndCaches

Tests that a successful search API call returns the expected collection of video objects and persists them in the cache for the given query.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures if the returned collection or caching behavior is incorrect.

### public async Task SearchVideosAsync_HttpRequestException_ReturnsEmptyListAndLogsError

Tests that an `HttpRequestException` during a search operation results in an empty list (not null) and an error-level log entry, preventing the exception from surfacing to callers.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures if the return value is not an empty list or logging is absent.

### public async Task SearchVideosAsync_ApiReturnsMalformedJson_ReturnsEmptyListAndLogsError

Tests that when the API returns a response with malformed JSON that cannot be deserialized, the method returns an empty list and logs an error, rather than throwing a deserialization exception.

- **Returns**: A completed `Task` representing the asynchronous test operation.
- **Throws**: Exceptions from assertion failures if the method does not handle the malformed response gracefully.

## Usage

```csharp
// Example 1: Running all CoubApiClientTests using xUnit
using Xunit;

public class TestSuiteExecution
{
    [Fact]
    public async Task RunAllCoubApiClientTests()
    {
        var tests = new CoubApiClientTests();

        // Cache hit scenarios
        await tests.GetVideoInfoAsync_CacheHit_ReturnsCachedInfo();
        await tests.VerifyVideoExistsAsync_CacheHit_ReturnsCachedValue();
        await tests.SearchVideosAsync_CacheHit_ReturnsCachedList();

        // Successful API call scenarios
        await tests.GetVideoInfoAsync_SuccessfulApiCall_ReturnsVideoInfoAndCaches();
        await tests.VerifyVideoExistsAsync_VideoExists_ReturnsTrueAndCaches();
        await tests.SearchVideosAsync_SuccessfulApiCall_ReturnsVideosAndCaches();

        // Error handling scenarios
        await tests.GetVideoInfoAsync_ApiReturnsNotFound_ReturnsNullAndLogsWarning();
        await tests.GetVideoInfoAsync_HttpRequestException_ReturnsNullAndLogsError();
        await tests.GetVideoInfoAsync_InvalidUrl_ReturnsNull();
        await tests.VerifyVideoExistsAsync_VideoDoesNotExist_ReturnsFalseAndCaches();
        await tests.SearchVideosAsync_HttpRequestException_ReturnsEmptyListAndLogsError();
        await tests.SearchVideosAsync_ApiReturnsMalformedJson_ReturnsEmptyListAndLogsError();
    }
}
```

```csharp
// Example 2: Selective execution focusing on cache integrity
using Xunit;

public class CacheIntegrityVerification
{
    [Fact]
    public async Task VerifyCacheBehaviorAcrossOperations()
    {
        var tests = new CoubApiClientTests();

        // Verify that cache hits work for all three operation types
        await tests.GetVideoInfoAsync_CacheHit_ReturnsCachedInfo();
        await tests.VerifyVideoExistsAsync_CacheHit_ReturnsCachedValue();
        await tests.SearchVideosAsync_CacheHit_ReturnsCachedList();

        // Verify that successful operations populate the cache
        await tests.GetVideoInfoAsync_SuccessfulApiCall_ReturnsVideoInfoAndCaches();
        await tests.VerifyVideoExistsAsync_VideoExists_ReturnsTrueAndCaches();
        await tests.SearchVideosAsync_SuccessfulApiCall_ReturnsVideosAndCaches();

        // Verify that negative results are also cached to prevent redundant API calls
        await tests.VerifyVideoExistsAsync_VideoDoesNotExist_ReturnsFalseAndCaches();
    }
}
```

## Notes

- **Caching consistency**: Tests for cache-hit scenarios assume that the cache has been pre-populated by a prior successful call or by test setup. The actual implementation must ensure that the same cache key is used for both writes and reads.
- **Thread safety**: These tests are designed to run asynchronously but do not inherently validate concurrent access to the underlying cache. If the production `CoubApiClient` is intended for multi-threaded use, additional concurrency tests should be authored separately.
- **Logging dependencies**: Several tests assert that warnings or errors are logged under specific failure conditions. These tests require a configured logging provider or a mock logger to capture and verify log output.
- **Empty list vs. null**: Search methods return an empty collection on failure rather than `null`, while single-item retrieval methods return `null` on failure. Callers must handle these differing sentinel values appropriately.
- **Malformed JSON handling**: The test `SearchVideosAsync_ApiReturnsMalformedJson_ReturnsEmptyListAndLogsError` implies that JSON deserialization failures are caught and suppressed. This prevents transient API glitches from crashing the application but may mask schema changes that require developer attention.
- **Invalid URL validation**: The test `GetVideoInfoAsync_InvalidUrl_ReturnsNull` indicates that URL validation occurs before any network request, avoiding unnecessary HTTP traffic for malformed input.
