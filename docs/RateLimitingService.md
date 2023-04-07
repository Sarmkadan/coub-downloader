# RateLimitingService

`RateLimitingService` manages request throttling and rate-limit tracking for external API consumption. It combines a token-bucket mechanism with discrete request counters and reset timestamps, allowing callers to check availability, consume capacity, and inspect current limits through a unified interface. The service is designed for scenarios where both short-term burst control and longer-term quota windows must be respected.

## API

### Properties

#### `RequestsRemaining`
`public int RequestsRemaining`

Gets the number of discrete requests still permitted before the next reset window. This value decreases as requests are consumed and resets to its maximum when the reset time elapses.

#### `ResetTime`
`public DateTime ResetTime`

Gets the UTC timestamp at which the request counter will reset to its full quota. Callers can use this to schedule retries or display backoff information.

#### `IsAllowed`
`public bool IsAllowed`

Indicates whether at least one request is currently permitted under both the token-bucket and the discrete request counter. Returns `false` when either mechanism is exhausted.

#### `TokensAvailable`
`public double TokensAvailable`

Gets the current number of tokens in the bucket. This value reflects continuous refill since `LastRefillTime` and decreases by the cost of each consumed operation.

#### `LastRefillTime`
`public DateTime LastRefillTime`

Gets the UTC timestamp of the most recent token-bucket refill calculation. Used internally to compute token replenishment when `TokensAvailable` is read or when consumption is attempted.

### Constructor

#### `RateLimitingService`
`public RateLimitingService`

Initializes a new instance with default limits. The discrete request counter starts at its maximum, the token bucket is filled to capacity, and both reset and refill timestamps are set to the current UTC time.

### Methods

#### `GetStatus`
`public RateLimitStatus GetStatus()`

Returns a snapshot of the current rate-limit state, including remaining requests, the reset time, available tokens, and whether consumption is currently allowed. The returned object is a value-type or immutable record reflecting the state at the moment of the call.

**Returns:** A `RateLimitStatus` instance populated with current limit information.

**Throws:** No exceptions are thrown by this method.

#### `Reset`
`public void Reset()`

Resets the discrete request counter to its maximum and sets `ResetTime` to the next window boundary. The token bucket is not affected by this call.

**Throws:** No exceptions are thrown by this method.

#### `ClearAll`
`public void ClearAll()`

Fully resets both the discrete request counter and the token bucket. The request counter returns to its maximum, `ResetTime` advances to the next window, the token bucket is refilled to capacity, and `LastRefillTime` is set to the current UTC time.

**Throws:** No exceptions are thrown by this method.

#### `ConsumeAsync`
`public async Task ConsumeAsync()`

Asynchronously attempts to consume one request unit. If capacity is available under both the discrete counter and the token bucket, consumption proceeds immediately and the task completes synchronously. If the token bucket is empty but will refill within a bounded delay, the call asynchronously waits until a token becomes available, then consumes it. If the discrete counter is exhausted and the reset time has not passed, the call waits until the reset time, then resets the counter and consumes.

**Throws:** `OperationCanceledException` if the cancellation token passed to the underlying delay is triggered. `TimeoutException` if the implementation enforces a maximum wait and that duration is exceeded.

#### `TryConsume`
`public bool TryConsume()`

Attempts to consume one request unit without waiting. Returns `true` if both the discrete counter and the token bucket had sufficient capacity and the consumption was recorded. Returns `false` immediately if either mechanism is exhausted.

**Returns:** `true` if consumption succeeded; `false` otherwise.

**Throws:** No exceptions are thrown by this method.

#### `GetRemainingTokens`
`public double GetRemainingTokens()`

Returns the current number of tokens available in the bucket, calculated with refill applied up to the current UTC time. This is equivalent to reading `TokensAvailable` but is provided as a method for callers that prefer explicit invocation.

**Returns:** The number of available tokens as a `double`.

**Throws:** No exceptions are thrown by this method.

## Usage

### Example 1: Fire-and-forget with fallback

```csharp
var rateLimiter = new RateLimitingService();

if (rateLimiter.TryConsume())
{
    await PerformApiCallAsync();
}
else
{
    var status = rateLimiter.GetStatus();
    Console.WriteLine($"Back off until {status.ResetTime:O}; {status.RequestsRemaining} requests left, {status.TokensAvailable:F2} tokens.");
    await Task.Delay(status.ResetTime - DateTime.UtcNow);
    await rateLimiter.ConsumeAsync();
    await PerformApiCallAsync();
}
```

### Example 2: Batch processing with async waiting

```csharp
var rateLimiter = new RateLimitingService();
var items = GetBatchItems();

foreach (var item in items)
{
    await rateLimiter.ConsumeAsync();
    await ProcessItemAsync(item);
}

// Inspect final state
var finalStatus = rateLimiter.GetStatus();
Console.WriteLine($"Batch complete. Remaining: {finalStatus.RequestsRemaining} requests, {finalStatus.TokensAvailable:F2} tokens.");
```

## Notes

- **Thread safety:** All public members that mutate state (`ConsumeAsync`, `TryConsume`, `Reset`, `ClearAll`) must be assumed to use internal synchronization. Callers may safely invoke them from multiple threads concurrently without external locking.
- **Token refill timing:** `TokensAvailable` and `GetRemainingTokens` compute refill based on elapsed wall-clock time since `LastRefillTime`. Frequent reads do not alter the bucket state.
- **Discrete counter vs. token bucket:** The discrete counter enforces a hard cap over a fixed window, while the token bucket smooths bursts. `IsAllowed` and `TryConsume` require both to have capacity. `ConsumeAsync` may wait on either constraint.
- **Reset behavior:** `Reset` only affects the discrete counter window. `ClearAll` is the only method that fully replenishes both mechanisms immediately.
- **Cancellation and timeouts:** Callers of `ConsumeAsync` should consider passing cancellation tokens if the underlying implementation supports them, as indefinite waiting may otherwise occur when limits are exhausted and no maximum wait is internally enforced.
