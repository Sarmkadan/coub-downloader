// existing content ...

## RetryHelper

`RetryHelper` provides a set of utility methods for executing operations with retry and fallback strategies. It helps to make operations more resilient to transient failures.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Utilities;

// Example 1: Execute with exponential backoff retry
var data = await RetryHelper.ExecuteWithRetryAsync(
    () => FetchDataAsync(),
    maxRetries: 3,
    initialDelayMs: 500,
    shouldRetry: (ex, attempt) => ex is TimeoutException
);

Console.WriteLine($"Fetched data: {data}");

// Example 2: Execute with linear retry
var data2 = await RetryHelper.ExecuteWithLinearRetryAsync(
    () => FetchDataAsync(),
    maxRetries: 5,
    delayMs: 1000
);

Console.WriteLine($"Fetched data: {data2}");

// Example 3: Execute with fallback
var data3 = await RetryHelper.ExecuteWithFallbackAsync(
    () => FetchDataAsync(),
    async () => await FetchFallbackDataAsync()
);

Console.WriteLine($"Fetched data: {data3}");

// Example 4: Execute with timeout
var data4 = await RetryHelper.ExecuteWithTimeoutAsync(
    () => FetchDataAsync(),
    TimeSpan.FromSeconds(10)
);

Console.WriteLine($"Fetched data: {data4}");
```