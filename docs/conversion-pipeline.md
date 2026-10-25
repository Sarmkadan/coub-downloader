# ConversionPipeline

`ConversionPipeline` is a small, sequential stage runner in `Infrastructure/Pipeline/ConversionPipeline.cs`. It chains `IPipelineStage` implementations so that the output of each stage becomes the input of the next. The file also contains the built-in download, validation, conversion and cleanup stages, the `ConversionResult` type, and the `PipelineBuilder` fluent helper.

> **Status:** the pipeline is not wired into the application. Nothing outside this file references `ConversionPipeline`, `PipelineBuilder` or the stages, and it is not registered in `DependencyInjection.cs`. The `ConversionPipeline` entries in `CHANGELOG.md` describe a `VideoConversionService` that delegates to it, but the current `VideoConversionService` does not. Treat this as a reference implementation until it is wired in.

## Namespace and dependencies

| Type | Namespace |
|------|-----------|
| `IPipelineStage<TInput, TOutput>`, `ConversionPipeline`, `PipelineBuilder`, stages, `ConversionResult` | `CoubDownloader.Infrastructure.Pipeline` |
| `ILoggingService` | `CoubDownloader.Infrastructure.Middleware` |
| `ICoubDownloadService`, `IVideoConversionService` | `CoubDownloader.Application.Services` |
| `DownloadTask`, `ConversionSettings`, `ProcessingState`, `VideoFormat`, `VideoQuality` | `CoubDownloader.Domain.Models` / `CoubDownloader.Domain.Enums` |

## Components

### `IPipelineStage<TInput, TOutput>`

```csharp
public interface IPipelineStage<TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
    string Name { get; }
}
```

- `Name` is used in log messages. Built-in stages use `Download`, `Validate`, `Convert` and `Cleanup`.
- `ExecuteAsync` receives the previous stage's output and the pipeline's cancellation token.

### `ConversionPipeline`

```csharp
var pipeline = new ConversionPipeline(logger);
pipeline.AddStage(stage);                // returns the pipeline for chaining
object result = await pipeline.ExecuteAsync(input, cancellationToken);
```

- **`AddStage<TInput, TOutput>(stage)`** wraps the stage in an internal `StageAdapter` that casts `object` to `TInput` at run time. It returns `this`.
- **`ExecuteAsync(object input, CancellationToken)`** runs every stage in insertion order:
  1. Logs `Executing pipeline stage: {Name}` (category `Pipeline`).
  2. Awaits the stage and passes its output to the next stage.
  3. Logs `Stage completed: {Name}`.
  4. On exception, logs `Pipeline stage failed: {Name}` with the exception and rethrows it unchanged.
- It returns the output of the last stage. With no stages, it returns `input` unchanged.

Type compatibility is checked only at run time. `AddStage` does not verify that stage *N*'s output type matches stage *N+1*'s input type, so a mismatched chain fails with an `InvalidCastException` when the cast in `StageAdapter` runs. Cancellation is passed to each stage, but the loop does not check the token between stages.

### `DownloadStage` (`string` → `DownloadTask`)

- Constructor: `DownloadStage(ICoubDownloadService downloadService)`.
- Creates a `DownloadTask` with a new GUID `Id`, the `Url`, `State = ProcessingState.Pending` and `CreatedAt = DateTime.UtcNow`.
- Calls `DownloadVideoAsync(url, ct)`. The returned `CoubVideo` is currently discarded, so the task's `OutputPath` and `VideoId` stay unset.

### `ValidationStage` (`DownloadTask` → `DownloadTask`)

- Throws `InvalidOperationException("URL is required")` when `Url` is null or empty.
- If `OutputPath` is not null and contains any character from `Path.GetInvalidPathChars()`, throws `InvalidOperationException("Invalid output path")`.
- A null `OutputPath` is accepted. The stage returns the same task instance.
- `Path.GetInvalidPathChars()` is platform-dependent: it returns a short list on Linux and a longer one on Windows.

### `ConversionStage` (`DownloadTask` → `ConversionResult`)

- Constructor: `ConversionStage(IVideoConversionService conversionService)`.
- Builds `new ConversionSettings { Format = task.Format, Quality = task.Quality }` and calls `ApplyQualityPreset()`.
- Calls `ConvertVideoAsync(input, output, settings, progress: null, ct)` with:
  - `input` = `task.OutputPath ?? "input.mp4"`
  - `output` = `"output.mp4"`
- `Success` is `true` when the returned path is not null or empty. The result carries `TaskId`, `Success` and `OutputPath`. Exceptions from the conversion service propagate; they are not converted into a failed `ConversionResult`.

### `CleanupStage` (`ConversionResult` → `ConversionResult`)

- Returns its input unchanged. The comment says "Remove temporary files", but no cleanup is performed.

### `ConversionResult`

| Property | Type | Notes |
|----------|------|-------|
| `TaskId` | `string` | Defaults to `""`. Set by `ConversionStage`. |
| `Success` | `bool` | Set by `ConversionStage`. |
| `OutputPath` | `string?` | Set by `ConversionStage`. |
| `Error` | `string?` | Declared, but never set by any stage. |

### `PipelineBuilder`

Fluent helper that creates one `ConversionPipeline`:

| Method | Adds | Notes |
|--------|------|-------|
| `WithDownload(ICoubDownloadService)` | `DownloadStage` | |
| `WithValidation()` | `ValidationStage` | |
| `WithConversion(IVideoConversionService)` | `ConversionStage` | |
| `WithCleanup()` | `CleanupStage` | |
| `Build()` | — | Returns the same pipeline instance the builder has been adding to. |

The builder does not enforce an order. Methods are applied in the order you call them, and calling `Build()` does not copy the pipeline.

## Usage

### Built-in chain

The full chain built by the builder is `string → DownloadTask → DownloadTask → ConversionResult → ConversionResult`:

```csharp
using CoubDownloader.Application.Services;
using CoubDownloader.Infrastructure.Middleware;
using CoubDownloader.Infrastructure.Pipeline;

var pipeline = new PipelineBuilder(logger)
    .WithDownload(downloadService)
    .WithValidation()
    .WithConversion(conversionService)
    .WithCleanup()
    .Build();

var result = (ConversionResult)await pipeline.ExecuteAsync("https://coub.com/view/abc123", cancellationToken);

if (!result.Success)
{
    logger.LogError($"Conversion failed for task {result.TaskId}");
}
```

Because `ExecuteAsync` takes and returns `object`, the caller casts the final result to the type of the last stage.

### Custom stage

Add your own stage by implementing `IPipelineStage<TInput, TOutput>` and passing it to `AddStage`. The stage types must line up with the previous stage's output:

```csharp
public class LogResultStage : IPipelineStage<ConversionResult, ConversionResult>
{
    public string Name => "LogResult";

    public Task<ConversionResult> ExecuteAsync(ConversionResult input, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Output: {input.OutputPath}");
        return Task.FromResult(input);
    }
}

var pipeline = new ConversionPipeline(logger)
    .AddStage(new LogResultStage());
```

## Known limitations

These behaviours are in the current code. They are documented here and not changed by this document.

1. **Downloaded file is never stored.** `DownloadStage` discards the `CoubVideo` and leaves `OutputPath` null. `ConversionStage` then falls back to the relative path `input.mp4`.
2. **Output path is hard-coded.** `ConversionStage` always writes to `output.mp4` in the working directory and ignores `task.OutputPath` as a destination.
3. **`ConversionResult.Error` is never populated.** Failures surface as exceptions from the stage, and the pipeline logs and rethrows them.
4. **`CleanupStage` is a no-op.** Temporary files are not removed.
5. **Stage types are not checked at compile time.** A wrong chain fails at run time with `InvalidCastException`.
6. **No cancellation check between stages.** Only the stages themselves see the token.
7. **Stale documentation elsewhere.** `docs/IPipelineStage.md` describes a non-generic `IPipelineStage` and a parameterless `PipelineBuilder()` constructor; neither matches the current code. `docs/architecture.md` describes a `download → validate → convert → sync-audio → save` flow that the pipeline does not implement. Use this document and the source as the reference.

## Related

- `Application/Services/IVideoConversionService.cs`, `Application/Services/ICoubDownloadService.cs`
- `Domain/Models/ConversionSettings.cs`, `Domain/Models/DownloadTask.cs`
- `Infrastructure/Middleware/LoggingService.cs` (`ILoggingService`)
