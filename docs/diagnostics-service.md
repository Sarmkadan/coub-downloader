# DiagnosticsService

`DiagnosticsService` (namespace `CoubDownloader.Infrastructure.Diagnostics`, file `Infrastructure/Diagnostics/DiagnosticsService.cs`) collects a point-in-time health snapshot of the running application: uptime, version and runtime info, memory and GC counters, operation performance metrics, optional cache statistics, and whether FFmpeg and disk space look usable. `ApplicationStartup` resolves it from DI and runs a health check at startup.

The same file also defines the report types returned by the service: `DiagnosticsReport` and `RuntimeStatisticsData`.

## Constructor

```csharp
public DiagnosticsService(
    ILoggingService logger,
    PerformanceMonitor performanceMonitor,
    ICacheService? cacheService = null)
```

| Parameter | Required | Purpose |
|-----------|----------|---------|
| `logger` | yes | Receives `LogWarning` calls from the memory and disk checks (category `"Diagnostics"`). |
| `performanceMonitor` | yes | Source of `OperationMetrics` via `GetAllMetrics()`. |
| `cacheService` | no | If `null`, `CacheStats` stays `null` and the cache section is omitted from the text report. |

The start time used for `UpTime` is captured when the service instance is constructed (`_startTime = DateTime.UtcNow`). It is not the process start time.

## Public API

### `DiagnosticsReport PerformHealthCheck()`

Builds a new `DiagnosticsReport` on every call. Each call also:

- logs a warning when memory is above the threshold,
- logs a warning when disk space is below the threshold,
- runs `ffmpeg -version` as a child process to check FFmpeg availability.

It does not throw for the disk or FFmpeg checks, which catch their own exceptions (see below). Exceptions from the injected logger, performance monitor, or cache service are not caught and propagate to the caller.

Health rules:

| Check | Condition | Effect |
|-------|-----------|--------|
| Memory | `RuntimeStatistics.GetMemoryUsageMb() > 1000` | Adds warning `High memory usage: {MB}MB`. Does **not** change `IsHealthy`. |
| Disk | Free space `< 500` MB | Adds warning `Low disk space: {MB}MB` and sets `IsHealthy = false`. |
| FFmpeg | `ffmpeg -version` does not exit with code 0 within 5 seconds | Sets `FFmpegAvailable = false` and adds warning `FFmpeg is not available`. Does **not** change `IsHealthy`. |

`IsHealthy` starts as `true`; disk space is the only check that makes it `false`.

### `string GetDiagnosticsString()`

Calls `PerformHealthCheck()` and formats the result as a fixed-width, box-drawn text report. Because it calls `PerformHealthCheck()`, every call also repeats the memory and disk warning log entries.

Sections of the output, in order:

1. Header: status (`✓ HEALTHY` / `✗ UNHEALTHY`), timestamp, uptime (`dd:hh:mm:ss`).
2. `SYSTEM INFORMATION`: app version, runtime version, OS, processor count, architecture (x64/x86).
3. `RUNTIME STATISTICS`: memory usage in MB, Gen0/Gen1/Gen2 GC collection counts.
4. `CACHE STATISTICS` (only when a cache service was supplied): total entries, hits, misses, evictions, hit rate (`P2` format).
5. `WARNINGS` (only when there are warnings): each warning truncated to 50 characters (47 plus `...`).
6. `TOOLS`: FFmpeg available (`✓ Yes` / `✗ No`).

`PerformanceMetrics`, `TotalMemoryMb`, and `Size` (cache) are part of the report objects but are not printed by this method.

## Report types

### `DiagnosticsReport`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Timestamp` | `DateTime` | — | UTC time when the report was built. |
| `UpTime` | `TimeSpan` | — | Time since the `DiagnosticsService` instance was constructed. |
| `AppInfo` | `ApplicationInfo` | `new()` | Version, runtime, OS, processor count, bitness, build date (from `VersionHelper.GetApplicationInfo()`). |
| `RuntimeStats` | `RuntimeStatisticsData` | `new()` | Memory and GC figures (see below). |
| `PerformanceMetrics` | `List<OperationMetrics>` | `[]` | Snapshot of `PerformanceMonitor.GetAllMetrics()`. |
| `Warnings` | `List<string>` | `[]` | Human-readable warnings from the checks above. |
| `FFmpegAvailable` | `bool` | `false` | Result of the FFmpeg check. |
| `IsHealthy` | `bool` | `false` | Set to `true` by `PerformHealthCheck()` and cleared only by the disk check. |
| `CacheStats` | `CacheStatistics?` | `null` | `null` when no cache service was supplied. |

Note: the `IsHealthy` and `FFmpegAvailable` property defaults are `false`. `PerformHealthCheck()` sets `IsHealthy` to `true` before the checks run.

### `RuntimeStatisticsData`

| Property | Type | Source |
|----------|------|--------|
| `MemoryMb` | `long` | Process working set, in MB (`RuntimeStatistics.GetMemoryUsageMb()`). |
| `Gen0Collections` | `int` | `GC.CollectionCount(0)`, since process start. |
| `Gen1Collections` | `int` | `GC.CollectionCount(1)`, since process start. |
| `Gen2Collections` | `int` | `GC.CollectionCount(2)`, since process start. |
| `TotalMemoryMb` | `long` | Managed heap size (`GC.GetTotalMemory(false)`), in MB. Not printed by `GetDiagnosticsString()`. |

## Related types

These are defined elsewhere and used as-is:

- `ILoggingService` (`Infrastructure/Middleware`): `LogWarning(string message, string? category)`.
- `PerformanceMonitor` / `OperationMetrics` (`Infrastructure/Statistics`).
- `RuntimeStatistics` / `GcStatistics` (`Infrastructure/Statistics`): memory, CPU, and GC helpers. `DiagnosticsService` does not use the CPU helper.
- `ICacheService` / `CacheStatistics` (`Infrastructure/Caching`).
- `VersionHelper` / `ApplicationInfo` (`Infrastructure/Utilities`).

## Implementation notes

- **FFmpeg check** starts `ffmpeg` from `PATH` with `-version`, redirects stdout without reading it, and waits up to 5 seconds. On timeout the process is not killed. Any exception (for example, `ffmpeg` not found) returns `false`.
- **Disk check** uses the first entry of `DriveInfo.GetDrives()`, not the drive the application writes to. If the call throws, the result is `0`, which reports `Low disk space: 0MB` and marks the report unhealthy.
- **Memory** is the process working set, not the managed heap. Managed heap size is in `TotalMemoryMb`.
- **DI registration**: `ApplicationStartup` calls `GetRequiredService<DiagnosticsService>()`, but a text search of the `.cs` files found no `AddSingleton`/`AddScoped`/`AddTransient` for `DiagnosticsService`. Unless the registration exists in a form that search missed, that call throws at startup. `examples/DiagnosticsAndMonitoring.cs` resolves an `IDiagnosticsService` interface that does not exist in the source tree.
- **Timestamp line** in `GetDiagnosticsString()` uses the format `{report.Timestamp:yyyy-MM-dd HH:mm:ss,-48}`. The alignment `,-48` is after the colon, so it is treated as part of the date format. The output therefore ends with a literal `,-48` and the line is not padded to the box width. The intended form would be `{report.Timestamp,-48:yyyy-MM-dd HH:mm:ss}`.

## Example

```csharp
var diagnostics = serviceProvider.GetRequiredService<DiagnosticsService>();

DiagnosticsReport report = diagnostics.PerformHealthCheck();
if (!report.IsHealthy)
{
    foreach (var warning in report.Warnings)
    {
        Console.Error.WriteLine(warning);
    }
}

Console.WriteLine(diagnostics.GetDiagnosticsString());
```
