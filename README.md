## IPipelineStage

`IPipelineStage` is an interface for defining a stage in the conversion pipeline. It provides a way to execute a specific task, such as downloading a video, validating its metadata, or converting it to a different format.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Pipeline;

// Create a pipeline stage for downloading a video
public class DownloadStage : IPipelineStage<string, DownloadTask>
{
    private readonly ICoubDownloadService _downloadService;

    public string Name => "Download";

    public DownloadStage(ICoubDownloadService downloadService)
    {
        _downloadService = downloadService;
    }

    public async Task<DownloadTask> ExecuteAsync(string url, CancellationToken cancellationToken = default)
    {
        var task = new DownloadTask
        {
            Id = Guid.NewGuid().ToString(),
            Url = url,
            State = ProcessingState.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var video = await _downloadService.DownloadVideoAsync(url, cancellationToken);

        return task;
    }
}

// Create a pipeline stage for validating a video
public class ValidationStage : IPipelineStage<DownloadTask, DownloadTask>
{
    public string Name => "Validate";

    public Task<DownloadTask> ExecuteAsync(DownloadTask task, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(task.Url))
            throw new InvalidOperationException("URL is required");

        if (task.OutputPath is not null && Path.GetInvalidPathChars().Any(c => task.OutputPath.Contains(c)))
            throw new InvalidOperationException("Invalid output path");

        return Task.FromResult(task);
    }
}

// Create a pipeline stage for converting a video
public class ConversionStage : IPipelineStage<DownloadTask, ConversionResult>
{
    private readonly IVideoConversionService _conversionService;

    public string Name => "Convert";

    public ConversionStage(IVideoConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    public async Task<ConversionResult> ExecuteAsync(DownloadTask task, CancellationToken cancellationToken = default)
    {
        var settings = new ConversionSettings { Format = task.Format, Quality = task.Quality };
        settings.ApplyQualityPreset();

        var outputPath = await _conversionService.ConvertVideoAsync(
            task.OutputPath ?? "input.mp4",
            "output.mp4",
            settings,
            null,
            cancellationToken);

        var success = !string.IsNullOrEmpty(outputPath);

        return new ConversionResult
        {
            TaskId = task.Id,
            Success = success,
            OutputPath = outputPath
        };
    }
}
```
