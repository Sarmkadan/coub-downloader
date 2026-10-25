# RetryHelper

Static helpers in `CoubDownloader.Infrastructure.Utilities` for re-running an async operation after failures, bounding how long a single operation may run, and falling back to alternatives. Source: `Infrastructure/Utilities/RetryHelper.cs`.

The file contains two classes:

- `RetryHelper` - retry, timeout and circuit-breaker entry points.
- `FallbackHelper` - fallback and first-success entry points (documented at the end).

All methods take a `Func<Task<T>>` rather than a `Task<T>`, so each attempt starts a fresh invocation of the operation.

Current production call sites: `Application/Services/CoubDownloadService.cs` (`FetchMetadataAsync`, `ExtractVideoSourceAsync`) call `ExecuteWithRetryAsync` with `maxRetries: 3` and a `shouldRetry` predicate that filters on network exceptions.

---

## Retry policy summary

| Method | Backoff between attempts | Retries on | Default `maxRetries` | Default delay |
|---|---|---|---|---|
| `ExecuteWithRetryAsync` | Exponential: `delay *= 1.5` after each retry | Any exception, unless `shouldRetry` returns `false` | 3 | 100 ms initial |
| `ExecuteWithLinearRetryAsync` | Fixed | Any exception | 3 | 100 ms |
| `ExecuteWithJitterRetryAsync` | Random: `base + random[0, base)` | Any exception | 3 | 100 ms base |
| `ExecuteWithRetryAndTimeoutAsync` | Fixed | Any exception (including the timeout's `TimeoutException`) | 3 | 100 ms |

Common rules for all retry methods:

- `maxRetries = N` means **at most N + 1 invocations** of the operation (the first attempt plus N retries).
- A negative `maxRetries` disables retries: the operation runs once and any exception propagates.
- The retry loop only catches exceptions while `attempt < maxRetries`. On the final failure the original exception is rethrown unchanged, and no delay is applied after it.
- The helpers do **not** accept a `CancellationToken`. Nothing stops a retry loop except the operation succeeding, the retry budget running out, or a `shouldRetry` predicate returning `false`.
- `OperationCanceledException` is **not** special-cased. It is retried like any other exception unless `shouldRetry` excludes it.

---

## Public API

### `RetryHelper`

#### `public static Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, int initialDelayMs = 100, Func<Exception, int, bool>? shouldRetry = null)`

Runs `operation` and retries it with exponential backoff.

- **Parameters**:
  - `operation` - delegate invoked once per attempt.
  - `maxRetries` - number of retries after the first attempt. Default `3`.
  - `initialDelayMs` - delay before the first retry, in milliseconds. Default `100`.
  - `shouldRetry` - optional predicate `(exception, attempt) => bool`. `attempt` is the number of retries already performed when the failure occurred (`0` after the first attempt fails). Return `false` to rethrow the exception immediately. It is not called for the final failure, because the retry budget is already exhausted at that point.
- **Delay schedule**: the delay is `initialDelayMs`, then multiplied by `1.5` after each retry and truncated to `int`. With defaults: 100, 150, 225, 337, 505, ... ms. There is no upper cap.
- **Return value**: the result of the first successful attempt.
- **Exceptions**: the exception from the last attempt (or the one rejected by `shouldRetry`), unchanged. An exception thrown by `Task.Delay` (for example a negative `initialDelayMs` other than `-1`) is also propagated.

```csharp
var info = await RetryHelper.ExecuteWithRetryAsync(
    () => _coubApiClient.GetVideoInfoAsync(coubUrl, cancellationToken),
    maxRetries: 3,
    shouldRetry: (ex, _) => IsRetryableNetworkException(ex));
```

---

#### `public static Task<T> ExecuteWithLinearRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, int delayMs = 100)`

Runs `operation` and retries it after a fixed delay.

- **Parameters**:
  - `operation` - delegate invoked once per attempt.
  - `maxRetries` - number of retries after the first attempt. Default `3`.
  - `delayMs` - delay before every retry, in milliseconds. Default `100`.
- **Delay schedule**: `delayMs` for every retry.
- **Predicate**: none. Every exception is retried while the budget lasts.
- **Return value**: the result of the first successful attempt.
- **Exceptions**: the exception from the last attempt, unchanged.

---

#### `public static Task<T> ExecuteWithJitterRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, int baseDelayMs = 100)`

Runs `operation` and retries it after a randomized delay, intended to spread out retries from concurrent callers.

- **Parameters**:
  - `operation` - delegate invoked once per attempt.
  - `maxRetries` - number of retries after the first attempt. Default `3`.
  - `baseDelayMs` - lower bound of the delay and the width of the random range. Default `100`.
- **Delay schedule**: `baseDelayMs + random.Next(0, baseDelayMs)`, so each delay is in the range `[baseDelayMs, 2 * baseDelayMs - 1]`. With `baseDelayMs = 0` the delay is always `0`. The delay does not grow between retries.
- **Randomness**: a new `System.Random` is created on each call. It is not seeded explicitly and is not cryptographic.
- **Return value**: the result of the first successful attempt.
- **Exceptions**: the exception from the last attempt, unchanged.

---

#### `public static Task<T> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> operation, int failureThreshold = 5, int resetTimeoutSeconds = 60)`

**Placeholder. It does not implement a circuit breaker.** The method awaits `operation()` once and returns its result or exception. `failureThreshold` and `resetTimeoutSeconds` are ignored, and no failure state is kept between calls.

Do not rely on it for fail-fast behavior. A real implementation would need shared state (failure count, open/half-open/closed status and a reset timer) across calls.

---

#### `public static Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout)`

Intended to fail an operation that takes longer than `timeout`. **In the current implementation the timeout is not enforced.**

- The method creates a `CancellationTokenSource` that would cancel after `timeout`, but the token is never passed to `operation`. The operation is therefore not cancelled, and the method waits for it to finish regardless of elapsed time.
- The only effect is in the `catch` block: if `operation` itself throws `OperationCanceledException`, it is rethrown as `TimeoutException("Operation timed out after {timeout.TotalSeconds} seconds")`. The original exception is not attached as `InnerException`, so a cancellation that did not come from the timeout is reported as a timeout as well.
- **Return value**: the result of `operation` if it completes.
- **Exceptions**: `TimeoutException` (only when `operation` throws `OperationCanceledException`); any other exception from `operation` unchanged.

To get real timeouts, the operation must accept and honor a `CancellationToken`. Callers that need this today should pass a token to the operation directly.

---

#### `public static Task<T> ExecuteWithRetryAndTimeoutAsync<T>(Func<Task<T>> operation, int maxRetries = 3, int operationTimeoutMs = 5000, int delayMs = 100)`

Retries `operation` with a fixed delay, calling `ExecuteWithTimeoutAsync` for each attempt.

- **Parameters**:
  - `operation` - delegate invoked once per attempt.
  - `maxRetries` - number of retries after the first attempt. Default `3`.
  - `operationTimeoutMs` - intended per-attempt timeout in milliseconds. Default `5000`. **Currently has no effect**, for the reason given under `ExecuteWithTimeoutAsync`.
  - `delayMs` - fixed delay between attempts, in milliseconds. Default `100`.
- **Retry predicate**: none. Any exception, including the `TimeoutException` produced by a cancelled operation, is retried while the budget lasts.
- **Return value**: the result of the first successful attempt.
- **Exceptions**: the exception from the last attempt, unchanged.

---

### `FallbackHelper`

These methods do not retry. They run an alternative when the primary operation fails.

#### `public static Task<T> ExecuteWithFallbackAsync<T>(Func<Task<T>> primaryOperation, Func<Task<T>> fallbackOperation, Func<Exception, bool>? shouldFallback = null)`

Runs `primaryOperation`. If it throws and `shouldFallback` (default: always `true`) returns `true`, runs `fallbackOperation` and returns its result.

- **Parameters**:
  - `primaryOperation` - tried first.
  - `fallbackOperation` - run only after a qualifying failure.
  - `shouldFallback` - optional predicate on the primary's exception. When it returns `false`, the exception propagates.
- **Return value**: the primary's result, or the fallback's result.
- **Exceptions**: exceptions from the fallback propagate. Exceptions from the primary propagate when `shouldFallback` returns `false`.

---

#### `public static Task<T> ExecuteWithFallbackAsync<T>(Func<Task<T>> operation, T defaultValue)`

Runs `operation`. If it throws any exception, returns `defaultValue`.

- **Parameters**:
  - `operation` - the operation to run.
  - `defaultValue` - value returned on any failure.
- **Return value**: the operation's result, or `defaultValue`.
- **Exceptions**: none from `operation`. All exceptions are swallowed silently, including `OperationCanceledException`, and nothing is logged. Use this overload only when a failure is genuinely acceptable to ignore.

---

#### `public static Task<T> ExecuteFirstSuccessAsync<T>(params Func<Task<T>>[] operations)`

Runs `operations` in order, one at a time, and returns the first successful result. Each later operation runs only if all earlier ones have failed.

- **Parameters**:
  - `operations` - candidates, tried in the given order.
- **Return value**: the result of the first operation that completes without throwing.
- **Exceptions**:
  - If every operation throws, the **last** exception is rethrown. Earlier exceptions are discarded.
  - If no operations are passed, `InvalidOperationException("All operations failed")` is thrown.
  - A `null` entry in `operations` throws `NullReferenceException` when invoked, which is treated as a failure like any other.

---

## Behavior notes and limitations

- **Retry budget vs. wall-clock time.** Worst-case time spent in `ExecuteWithRetryAsync` is the sum of all delays plus the time of each attempt. With defaults and `maxRetries: 3`, the delays total 100 + 150 + 225 = 475 ms before the final attempt. Delays are not capped, so large `maxRetries` or `initialDelayMs` values can make calls very long.
- **Null operation.** Passing a `null` delegate to a retry method does not throw immediately. The `NullReferenceException` is raised inside the retry loop and is retried like any other exception.
- **Timeouts are not enforced.** See `ExecuteWithTimeoutAsync`. `ExecuteWithRetryAndTimeoutAsync` inherits this.
- **Circuit breaker is a stub.** See `ExecuteWithCircuitBreakerAsync`.
- **Exception identity.** Retry methods rethrow the original exception object, so stack traces and `Assert.Same`-style identity are preserved. `ExecuteFirstSuccessAsync` uses `throw lastException`, which resets the stack trace of that exception.
- **Thread context.** The helpers use `await` without `ConfigureAwait(false)`, so continuations resume on the caller's synchronization context.

---

## Tests

`tests/coub-downloader.Tests/RetryHelperTests.cs` covers `ExecuteWithRetryAsync`:

- a successful first attempt runs the operation once;
- transient failures are retried until success;
- the final exception is rethrown after retries are exhausted;
- `shouldRetry` can stop retries for cancellation;
- a null operation fails with `NullReferenceException`;
- a negative `maxRetries` disables retries.

The other helpers in this file, including `FallbackHelper`, have no tests in that file.
