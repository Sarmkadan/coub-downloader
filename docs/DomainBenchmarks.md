# DomainBenchmarks

`DomainBenchmarks` provides aggregated performance and metadata calculations for a domain entity processed by the coub-downloader pipeline. It centralises view count formatting, file size formatting, output size estimation, and progress percentage computation, allowing callers to obtain human-readable metrics and resource forecasts without duplicating conversion logic.

## API

### `public void Setup`
Initialises or refreshes the internal state required for subsequent metric calculations. Must be called before any other member to ensure accurate results.

- **Parameters:** none
- **Return value:** void
- **Throws:** may throw an `InvalidOperationException` if underlying data sources are unavailable or if called in an invalid state (specific conditions depend on internal dependency readiness).

### `public string GetFormattedViewCount`
Returns the total view count associated with the domain, formatted as a human-readable string (e.g., with thousands separators or abbreviated suffixes).

- **Parameters:** none
- **Return value:** `string` — the formatted view count; never null.
- **Throws:** may throw `InvalidOperationException` if `Setup` has not been called successfully.

### `public string GetFormattedFileSize`
Returns the total file size of the domain’s associated media, formatted as a human-readable string (e.g., “12.3 MB”, “1.1 GB”).

- **Parameters:** none
- **Return value:** `string` — the formatted file size; never null.
- **Throws:** may throw `InvalidOperationException` if `Setup` has not been called successfully.

### `public long EstimateOutputSize`
Estimates the total output size in bytes that the domain will consume after processing (download, conversion, or packaging). The value reflects a best-effort forecast based on current state.

- **Parameters:** none
- **Return value:** `long` — estimated output size in bytes.
- **Throws:** may throw `InvalidOperationException` if `Setup` has not been called successfully.

### `public int GetProgressPercent`
Computes the current processing progress of the domain as an integer percentage (0–100). The calculation relies on internal counters set during processing.

- **Parameters:** none
- **Return value:** `int` — progress percentage, clamped to the range 0–100 inclusive.
- **Throws:** may throw `InvalidOperationException` if `Setup` has not been called successfully.

## Usage

### Example 1: Basic metrics display after setup

```csharp
DomainBenchmarks benchmarks = new DomainBenchmarks();

// Initialise internal state
benchmarks.Setup();

string views = benchmarks.GetFormattedViewCount();
string size = benchmarks.GetFormattedFileSize();
long estimatedBytes = benchmarks.EstimateOutputSize();

Console.WriteLine($"Views: {views}");
Console.WriteLine($"Current size: {size}");
Console.WriteLine($"Estimated output: {estimatedBytes} bytes");
```

### Example 2: Progress reporting during batch processing

```csharp
DomainBenchmarks benchmarks = new DomainBenchmarks();
benchmarks.Setup();

while (!processingComplete)
{
    // Perform incremental work...

    int progress = benchmarks.GetProgressPercent();
    Console.WriteLine($"Processing progress: {progress}%");

    if (progress >= 100)
    {
        processingComplete = true;
    }
}

string finalSize = benchmarks.GetFormattedFileSize();
Console.WriteLine($"Final output size: {finalSize}");
```

## Notes

- **Setup requirement:** All metric-returning members assume `Setup` has been called at least once. Calling them without prior successful setup will likely result in an `InvalidOperationException`. Repeated calls to `Setup` may reset internal counters, causing `GetProgressPercent` to return 0 again.
- **Progress clamping:** `GetProgressPercent` returns values strictly between 0 and 100. If internal counters exceed expected bounds (e.g., due to estimation errors or partial resets), the result is clamped rather than throwing.
- **Estimate volatility:** `EstimateOutputSize` may change over time as more data becomes available. It is a point-in-time forecast and should not be treated as a guaranteed final size.
- **Thread safety:** The type is not explicitly designed for concurrent use. If multiple threads call `Setup` or metric methods simultaneously without external synchronisation, results may be inconsistent or exceptions may occur. Callers should serialise access or use their own locking when sharing an instance across threads.
- **Formatting locale:** `GetFormattedViewCount` and `GetFormattedFileSize` produce culture-invariant or fixed-format strings suitable for display; they do not adapt to the current thread’s culture unless internally configured otherwise.
