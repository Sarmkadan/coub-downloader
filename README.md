## IPipelineStage

`IPipelineStage` is an interface for defining a stage in the conversion pipeline. It provides a way to execute a specific task, such as downloading a video, validating its metadata, or converting it to a different format.


## ILoggingService

`ILoggingService` provides structured logging capabilities with support for different severity levels (Info, Warning, Error, Debug) and optional categorization. It is used throughout the application to record operational events, errors, and debugging information to both file and in-memory storage. The service supports dependency injection and can be easily mocked for testing purposes.


### Usage Example

```csharp
using CoubDownloader.Infrastructure.Middleware;

// Example 1: Using FileLoggingService in production
public class VideoDownloadService
{
    private readonly ILoggingService _logger;
    private readonly FileLoggingService _fileLogger;

    public VideoDownloadService(ILoggingService logger, FileLoggingService fileLogger)
    {
        _logger = logger;
        _fileLogger = fileLogger;
    }

    public async Task DownloadCoubAsync(string coubUrl, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Starting download for coub: {coubUrl}", "DownloadService");
            
            // Download logic here
            
            _logger.LogInfo("Download completed successfully", "DownloadService");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to download coub", ex, "DownloadService");
            throw;
        }
    }
}

// Example 2: Using MemoryLoggingService for testing
public class CoubProcessorTests
{
    private readonly MemoryLoggingService _memoryLogger = new();

    [Fact]
    public void TestDownloadProcess_ShouldLogMessages()
    {
        var processor = new CoubProcessor(_memoryLogger);
        processor.ProcessCoub("https://coub.com/view/123");
        
        // Verify logs were recorded
        var logs = _memoryLogger.GetLogs();
        Assert.NotEmpty(logs);
        Assert.Contains(logs, l => l.Level == "INFO" && l.Message.Contains("Starting"));
    }
}

// Example 3: Logging different severity levels
public class LoggingExamples
{
    private readonly ILoggingService _logger;

    public void DemonstrateLogging()
    {
        _logger.LogDebug("Debug information about internal state", "Performance");
        _logger.LogInfo("Application started", "System");
        _logger.LogWarning("Disk space running low", "Storage");
        _logger.LogError("Failed to connect to database", new InvalidOperationException("Connection timeout"), "Database");
    }
}
```

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
