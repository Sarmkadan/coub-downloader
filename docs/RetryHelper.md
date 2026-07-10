# RetryHelper

`RetryHelper` is a static utility class that provides a collection of resilient asynchronous execution strategies. It encapsulates common retry, timeout, circuit breaker, and fallback patterns, enabling callers to wrap potentially transient operations in robust, configurable pipelines without implementing the resilience logic themselves.

## API

All methods are static and generic, accepting a `Func<Task<T>>` or `Func<CancellationToken, Task<T>>` representing the operation to execute. Unless otherwise noted, each method returns the result of the first successful execution of the operation and throws the last captured exception if all attempts are exhausted or a non-transient failure occurs.

### ExecuteWithRetryAsync\<T\>

```csharp
public static async Task<T> ExecuteWithRetryAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    int maxRetries,
    TimeSpan delay,
    CancellationToken cancellationToken = default)
```

Executes the operation and retries up to `maxRetries` times with a constant `delay` between attempts. If the operation succeeds before retries are exhausted, the result is returned immediately. If all attempts fail, the last exception is rethrown. The `cancellationToken` is passed to each invocation of the operation and can be used to abort the entire retry loop.

### ExecuteWithLinearRetryAsync\<T\>

```csharp
public static async Task<T> ExecuteWithLinearRetryAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    int maxRetries,
    TimeSpan initialDelay,
    CancellationToken cancellationToken = default)
```

Executes the operation and retries up to `maxRetries` times, increasing the delay linearly on each attempt. The delay for attempt *n* (zero-based) is `initialDelay * (n + 1)`. This provides a gradually increasing back-off without randomization. Cancellation and exception semantics are identical to `ExecuteWithRetryAsync`.

### ExecuteWithJitterRetryAsync\<T\>

```csharp
public static async Task<T> ExecuteWithJitterRetryAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    int maxRetries,
    TimeSpan medianDelay,
    CancellationToken cancellationToken = default)
```

Executes the operation and retries up to `maxRetries` times, applying randomized jitter to the delay between attempts. The actual delay for each retry is computed as a random value distributed around `medianDelay`, helping to avoid thundering-herd problems when multiple clients retry simultaneously. Cancellation and exception semantics are identical to `ExecuteWithRetryAsync`.

### ExecuteWithCircuitBreakerAsync\<T\>

```csharp
public static async Task<T> ExecuteWithCircuitBreakerAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    int failureThreshold,
    TimeSpan breakDuration,
    CancellationToken cancellationToken = default)
```

Implements a simple circuit breaker. If the operation fails `failureThreshold` consecutive times, the circuit opens for `breakDuration`. While the circuit is open, any call immediately throws a `CircuitBreakerOpenException` (or a custom exception type indicating the circuit is open) without invoking the operation. After `breakDuration` elapses, the circuit transitions to half-open and permits one trial call; success closes the circuit, failure reopens it. The `cancellationToken` is respected during both operation execution and the break period.

### ExecuteWithTimeoutAsync\<T\>

```csharp
public static async Task<T> ExecuteWithTimeoutAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
```

Executes the operation with a per-attempt timeout. If the operation does not complete within `timeout`, it is cancelled and a `TimeoutException` (or an `OperationCanceledException` derived from the timeout) is thrown. The external `cancellationToken` is combined with the timeout token; if either triggers, the operation is cancelled. No retries are performed—this method enforces a single execution deadline.

### ExecuteWithRetryAndTimeoutAsync\<T\>

```csharp
public static async Task<T> ExecuteWithRetryAndTimeoutAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    int maxRetries,
    TimeSpan delay,
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
```

Combines retry and timeout. Each individual attempt is subject to `timeout`. If an attempt times out or fails, the method waits `delay` and retries, up to `maxRetries` times. The last captured exception (which may be a timeout or the operation's own exception) is thrown if all attempts are exhausted. The external `cancellationToken` can cancel the entire retry loop.

### ExecuteWithFallbackAsync\<T\> (two overloads)

```csharp
public static async Task<T> ExecuteWithFallbackAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    Func<CancellationToken, Task<T>> fallback,
    CancellationToken cancellationToken = default)

public static async Task<T> ExecuteWithFallbackAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    T fallbackValue,
    CancellationToken cancellationToken = default)
```

Executes the primary operation. If it succeeds, its result is returned. If it throws, the fallback is executed (first overload) or the static `fallbackValue` is returned (second overload). The fallback function receives the same `cancellationToken`. If the fallback itself throws, that exception propagates to the caller. The second overload never throws due to primary failure, but may still throw if the operation throws and the caller considers that unacceptable (the fallback value suppresses the exception).

### ExecuteFirstSuccessAsync\<T\>

```csharp
public static async Task<T> ExecuteFirstSuccessAsync<T>(
    IEnumerable<Func<CancellationToken, Task<T>>> operations,
    CancellationToken cancellationToken = default)
```

Accepts a collection of alternative operations and executes them sequentially until one succeeds. The result of the first successful operation is returned. If all operations throw, an `AggregateException` (or a custom composite exception) containing all individual exceptions is thrown. The `cancellationToken` is passed to each operation and can abort the sequence early.

## Usage

### Example 1: HTTP download with linear back-off and timeout

```csharp
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public async Task<string> DownloadWithResilienceAsync(string url)
{
    using var httpClient = new HttpClient();
    
    return await RetryHelper.ExecuteWithRetryAndTimeoutAsync<string>(
        async ct =>
        {
            var response = await httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        },
        maxRetries: 3,
        delay: TimeSpan.FromSeconds(2),
        timeout: TimeSpan.FromSeconds(10),
        cancellationToken: CancellationToken.None);
}
```

### Example 2: Multi-source fallback with circuit breaker

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public async Task<byte[]> FetchDataWithFallbackAsync(CancellationToken ct)
{
    // Primary source with circuit breaker
    var primary = RetryHelper.ExecuteWithCircuitBreakerAsync<byte[]>(
        async innerCt => await FetchFromPrimaryApiAsync(innerCt),
        failureThreshold: 5,
        breakDuration: TimeSpan.FromMinutes(1),
        ct);

    // Fallback sources tried in order
    var fallbacks = new List<Func<CancellationToken, Task<byte[]>>>
    {
        async innerCt => await FetchFromSecondaryApiAsync(innerCt),
        async innerCt => await FetchFromCacheAsync(innerCt)
    };

    return await RetryHelper.ExecuteWithFallbackAsync(
        async token => await primary,
        async token => await RetryHelper.ExecuteFirstSuccessAsync(fallbacks, token),
        ct);
}
```

## Notes

- **Thread safety**: All methods are static and stateless with respect to shared mutable data. They are safe to call concurrently from multiple threads, provided the supplied `Func` delegates are themselves thread-safe or operate on isolated state.
- **Cancellation**: Every method accepts a `CancellationToken` and passes it to the operation delegate. Callers should ensure that the operation respects the token; otherwise, timeouts and external cancellation may not take effect promptly.
- **Exception aggregation**: `ExecuteFirstSuccessAsync` may throw an aggregate exception containing all failures. Callers should be prepared to unwrap or flatten `AggregateException` when using this method.
- **Circuit breaker state**: `ExecuteWithCircuitBreakerAsync` maintains internal state across calls for the same operation delegate instance. If the same delegate is reused across multiple call sites, they share the same circuit state. For independent circuits, supply distinct delegate instances or wrap the method in a stateful abstraction.
- **Jitter distribution**: The exact distribution used by `ExecuteWithJitterRetryAsync` is an implementation detail. Callers should treat `medianDelay` as the central tendency and expect actual delays to vary around it.
- **Fallback exception suppression**: The value-based overload of `ExecuteWithFallbackAsync` does not throw when the primary operation fails. This means callers lose diagnostic information about the primary failure unless they log it inside the operation delegate before throwing.
- **Timeout precision**: `ExecuteWithTimeoutAsync` and `ExecuteWithRetryAndTimeoutAsync` use cooperative cancellation via `CancellationTokenSource`. The timeout is a best-effort deadline; an uncooperative operation may exceed it.
