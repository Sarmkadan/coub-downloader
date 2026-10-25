# Rate Limiting Service

Source: `Infrastructure/Middleware/RateLimitingService.cs`

This file defines three types in the `CoubDownloader.Infrastructure.Middleware` namespace:

| Type | Algorithm | Purpose |
|------|-----------|---------|
| `RateLimitingService` | Fixed window counter, keyed by identifier | Caps how many calls each identifier may make per time window. |
| `RateLimitStatus` | Plain data object | Snapshot returned by `RateLimitingService.GetStatus`. |
| `ThrottlingService` | Token bucket, keyed by identifier | Smooths calls to a steady rate with a limited burst. |

The service protects the Coub API from being overloaded and keeps the downloader within the request budget that Coub expects. Callers decide what to do when a request is refused: `CoubApiClient` logs a warning and returns `null` or an empty list.

## Purpose

- **`RateLimitingService`** answers "may this identifier make another request in the current window?" Each call to `IsAllowed` that returns `true` consumes one slot. Once the slots are used, `IsAllowed` returns `false` until the window ends.
- **`ThrottlingService`** answers "are there enough tokens for this operation?" Tokens refill continuously at `tokensPerSecond` up to `maxTokens`, so short bursts are allowed but the long-run rate is capped.

Identifiers are arbitrary strings. Each identifier has its own independent bucket, so one endpoint being throttled does not block others.

## Configuration

Both classes are configured only through their constructor parameters. There is no configuration file binding (see [Configuration sources that are not wired](#configuration-sources-that-are-not-wired)).

### `RateLimitingService`

```csharp
public RateLimitingService(int maxRequestsPerWindow = 100, int windowSeconds = 60)
```

| Parameter | Default | Meaning |
|-----------|---------|---------|
| `maxRequestsPerWindow` | `100` | Number of `IsAllowed` successes allowed per identifier in one window. |
| `windowSeconds` | `60` | Length of the window in seconds. The window starts at the first request for an identifier, or at the first request after the previous window ended. |

The constructor does not validate its arguments. A value of `0` or less for `maxRequestsPerWindow` still lets the first call for each identifier through, because the bucket is created with `maxRequestsPerWindow - 1` remaining and that first call returns `true`.

### `ThrottlingService`

```csharp
public ThrottlingService(double tokensPerSecond, double maxTokens)
```

| Parameter | Meaning |
|-----------|---------|
| `tokensPerSecond` | Refill rate. Tokens added per second of elapsed time. |
| `maxTokens` | Bucket capacity and initial token count. This is the largest burst that can be consumed at once. |

The constructor does not validate its arguments. Two cases will block callers indefinitely:

- `tokensPerSecond` of `0` means the bucket never refills.
- `tokens` in `TryConsume` or `ConsumeAsync` greater than `maxTokens` can never be satisfied.

### Where the limits are set today

| Location | Instance | Limits |
|----------|----------|--------|
| `Infrastructure/Integration/CoubApiClient.cs` (constructor) | `new RateLimitingService(maxRequestsPerWindow: RateLimitMaxRequestsPerWindow)` | 30 requests per 60-second window. Window uses the default. |
| `Infrastructure/DependencyInjectionExtended.cs` (`AddPhase2Services`) | `services.AddSingleton(new RateLimitingService(100, 60))` | 100 requests per 60 seconds. |
| `Infrastructure/DependencyInjectionExtended.cs` (`AddPhase2Services`) | `services.AddSingleton(new ThrottlingService(10, 20))` | 10 tokens per second, burst of 20. |

`CoubApiClient` constants:

| Constant | Value | Usage |
|----------|-------|-------|
| `RateLimitMaxRequestsPerWindow` | `30` | Limit passed to the constructor. |
| `RateLimitKeyCoubApi` | `"coub_api"` | Identifier for video info requests. |
| `RateLimitKeyCoubSearch` | `"coub_search"` | Identifier for search requests. |

Each `CoubApiClient` creates its own `RateLimitingService`, so the two identifiers are counted separately within one client, and separate client instances do not share a budget. The DI singletons are registered but are not injected anywhere in the codebase; `CoubApiClient` does not use them.

To change the Coub API budget, edit `RateLimitMaxRequestsPerWindow` in `CoubApiClient.cs`. To change the shared singleton, edit `AddPhase2Services`.

### Configuration sources that are not wired

The following values exist in the configuration model but are **not read** by `RateLimitingService` or `ThrottlingService`. Changing them has no effect on rate limiting:

| Setting | Where defined | Notes |
|---------|---------------|-------|
| `ApiSettings.RateLimitPerWindow` (default `100`) | `Infrastructure/Configuration/ConfigurationManager.cs` | Not referenced by any code. |
| `ApiSettings.RateLimitWindowSeconds` (default `60`) | `Infrastructure/Configuration/ConfigurationManager.cs` | Not referenced by any code. |
| `CoubDownloaderOptions.RateLimitPerMinute` (default `60`), bound from `Api:RateLimitPerMinute` in `appsettings.json` | `Domain/Models/Options/CoubDownloaderOptions.cs` | Not referenced by any code. |
| `InfrastructureConstants.RateLimitRequestsPerWindow`, `RateLimitWindowSeconds`, `ThrottleTokensPerSecond`, `ThrottleMaxTokens` | `Infrastructure/Constants/InfrastructureConstants.cs` | Not referenced by any code. |

If configuration-driven limits are needed, the limits must first be read from these settings and passed to the constructors. That is a behavior change and is not done by this document.

## API reference

### `RateLimitingService`

#### `bool IsAllowed(string identifier)`

Checks the bucket for `identifier` and, if a slot is available, consumes it and returns `true`. Otherwise returns `false` without changing the count.

- First call for an identifier (or first call after `Reset`/`ClearAll`): creates the bucket with `maxRequestsPerWindow - 1` remaining, starts a window of `windowSeconds`, and returns `true`.
- Call after the window end: starts a new window with `maxRequestsPerWindow - 1` remaining and returns `true`.
- Call inside the window with slots left: decrements the count and returns `true`.
- Call inside the window with no slots left: returns `false`.

#### `RateLimitStatus GetStatus(string identifier)`

Returns the current state for `identifier` without consuming a slot.

- Unknown identifier: `RequestsRemaining` is `maxRequestsPerWindow`, `ResetTime` is now plus the window length, and `IsAllowed` is `true`. No bucket is created.
- Known identifier: `RequestsRemaining` is the stored count clamped to be at least `0`, `ResetTime` is the stored window end, and `IsAllowed` is `true` if there are slots left or the window has ended.

Note that the stored count is only refreshed by `IsAllowed`. After a window ends, `GetStatus` can report `RequestsRemaining = 0` with `IsAllowed = true` until the next `IsAllowed` call for that identifier starts a new window.

#### `void Reset(string identifier)`

Removes the bucket for `identifier`. The next `IsAllowed` call starts a fresh window. Unknown identifiers are ignored.

#### `void ClearAll()`

Removes all buckets. Equivalent to calling `Reset` for every identifier.

### `RateLimitStatus`

| Property | Type | Description |
|----------|------|-------------|
| `RequestsRemaining` | `int` | Slots left in the current window. |
| `ResetTime` | `DateTime` | UTC time when the window ends. |
| `IsAllowed` | `bool` | Whether a call would currently be allowed. |
| `SecondsUntilReset` | `int` (read-only) | Whole seconds until `ResetTime`, never negative. Computed from the current UTC time on each read. |

### `ThrottlingService`

#### `Task ConsumeAsync(string identifier, double tokens = 1.0)`

Calls `TryConsume` in a loop, waiting 10 ms between attempts, until it succeeds. It does not accept a `CancellationToken`, so it waits as long as necessary. It never returns if `tokens` exceeds `maxTokens` or the refill rate is `0`.

#### `bool TryConsume(string identifier, double tokens = 1.0)`

Refills the bucket for the time elapsed since its last refill, capped at `maxTokens`. If at least `tokens` are available, subtracts them and returns `true`. Otherwise returns `false`. A new bucket starts full at `maxTokens`.

#### `double GetRemainingTokens(string identifier)`

Returns the number of tokens available right now, including refill for elapsed time, without consuming any. Unknown identifiers return `maxTokens`. This method does not update the stored bucket.

`ThrottlingService` has no reset method. Buckets are kept for the lifetime of the instance.

## Usage

### Fixed window check (as in `CoubApiClient`)

```csharp
private readonly RateLimitingService _rateLimiter = new(maxRequestsPerWindow: 30);

public async Task<CoubVideoInfo?> GetVideoInfoAsync(string url, CancellationToken cancellationToken)
{
    if (!_rateLimiter.IsAllowed("coub_api"))
    {
        _logger.LogWarning("Rate limit exceeded for Coub API", "CoubApiClient");
        return null;
    }

    // ... perform the request
}
```

### Reporting how long to wait

```csharp
var status = limiter.GetStatus("coub_api");
if (!status.IsAllowed)
{
    _logger.LogWarning($"Retry in {status.SecondsUntilReset}s", "CoubApiClient");
}
```

### Smoothing calls with a token bucket

```csharp
var throttle = new ThrottlingService(tokensPerSecond: 10, maxTokens: 20);

await throttle.ConsumeAsync("api", tokens: 1);
await SendRequestAsync();
```

## Behavior notes

- **Thread safety.** Both classes guard their dictionaries with an internal lock. Calls can be made from multiple threads without external synchronization. `Parallel` tests in `tests/coub-downloader.Tests/RateLimitingServiceTests.cs` verify that concurrent `IsAllowed` calls never exceed the limit.
- **Memory.** Buckets are never evicted automatically. Each distinct identifier stays in memory until `Reset` or `ClearAll` removes it (`RateLimitingService` only). Use a small, fixed set of identifiers.
- **Clock.** Both classes use `DateTime.UtcNow`. Clock changes can shift window ends.
- **No cancellation.** `ThrottlingService.ConsumeAsync` cannot be cancelled. Wrap calls with a timeout at the call site if needed.
- **Refusal is not an error.** `IsAllowed` returning `false` is an ordinary result. The class throws no exceptions.
- **Single-process scope.** State lives in memory. Separate processes or machines do not share limits.

## Related files

- `Infrastructure/Integration/CoubApiClient.cs`: primary consumer of `RateLimitingService`.
- `Infrastructure/DependencyInjectionExtended.cs`: registers the unused DI singletons.
- `tests/coub-downloader.Tests/RateLimitingServiceTests.cs`: unit tests.
- `docs/RateLimitingService.md`: an older guide. It describes members that the current code does not have. Prefer this document.
