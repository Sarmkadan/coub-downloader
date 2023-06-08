## ErrorHandlingMiddleware

`ErrorHandlingMiddleware` provides centralized error handling and recovery strategies for the application. It intercepts exceptions, maps them to structured error responses, and supports custom exception handlers for specific exception types. The middleware integrates with `ILoggingService` to log errors with proper categorization and severity levels.

The class also includes a `RetryPolicy` helper for implementing exponential backoff retry logic in both synchronous and asynchronous operations.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Middleware;
using CoubDownloader.Domain.Exceptions;

// Example 1: Basic error handling with default handlers
public class VideoDownloadService
{
    private readonly ErrorHandlingMiddleware _errorHandler;
    private readonly ILoggingService _logger;

    public VideoDownloadService(ErrorHandlingMiddleware errorHandler, ILoggingService logger)
    {
        _errorHandler = errorHandler;
        _logger = logger;
    }

    public async Task DownloadCoubAsync(string coubUrl, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(coubUrl))
                throw new ArgumentException("Coub URL cannot be empty", nameof(coubUrl));

            // Download logic here
            _logger.LogInfo($"Starting download for coub: {coubUrl}", "DownloadService");
        }
        catch (Exception ex)
        {
            var errorResponse = _errorHandler.HandleError(ex);
            _logger.LogError(errorResponse.Message, ex, errorResponse.Category);
            
            // Re-throw or handle based on error type
            if (errorResponse.StatusCode >= 500)
                throw new CoubDownloaderException("Download failed", ex);
            
            throw;
        }
    }
}

// Example 2: Custom exception handler for specific exception types
public class CoubDownloadService
{
    private readonly ErrorHandlingMiddleware _errorHandler;
    private readonly ILoggingService _logger;

    public CoubDownloadService(ErrorHandlingMiddleware errorHandler, ILoggingService logger)
    {
        _errorHandler = errorHandler;
        _logger = logger;
        
        // Register custom handler for network-related exceptions
        _errorHandler.RegisterHandler<HttpRequestException>(ex => new ErrorResponse
        {
            StatusCode = 503,
            Message = "Failed to download coub due to network issues",
            ErrorType = "NetworkException",
            Category = "Network",
            Timestamp = DateTime.UtcNow,
            Details = ex.Message,
            Metadata = new Dictionary<string, object>
            {
                ["Uri"] = ex.RequestUri?.ToString() ?? "unknown",
                ["StatusCode"] = ex.StatusCode
            }
        });
    }

    public async Task DownloadVideoAsync(string url, CancellationToken cancellationToken)
    {
        // Use retry policy for transient operations
        var retryPolicy = new RetryPolicy
        {
            MaxRetries = 5,
            InitialDelayMs = 200,
            BackoffMultiplier = 1.5
        };
        
        try
        {
            var videoData = await retryPolicy.ExecuteAsync(async () => 
                await DownloadWithRetryAsync(url, cancellationToken));
            return videoData;
        }
        catch (Exception ex)
        {
            var errorResponse = _errorHandler.HandleError(ex);
            throw new CoubDownloaderException(errorResponse.Message, ex);
        }
    }

    private async Task<byte[]> DownloadWithRetryAsync(string url, CancellationToken cancellationToken)
    {
        // Actual download implementation
        using var httpClient = new HttpClient();
        return await httpClient.GetByteArrayAsync(url, cancellationToken);
    }
}

// Example 3: Using error response properties for conditional logic
public class ErrorResponseHandler
{
    private readonly ErrorHandlingMiddleware _errorHandler;

    public void HandleDownloadError(Exception ex)
    {
        var errorResponse = _errorHandler.HandleError(ex);
        
        Console.WriteLine($"Error Type: {errorResponse.ErrorType}");
        Console.WriteLine($"Status Code: {errorResponse.StatusCode}");
        Console.WriteLine($"Category: {errorResponse.Category}");
        Console.WriteLine($"Message: {errorResponse.Message}");
        
        // Apply different strategies based on error type
        if (errorResponse.StatusCode == 404)
        {
            Console.WriteLine("Resource not found - implementing fallback strategy");
        }
        else if (errorResponse.StatusCode >= 500)
        {
            Console.WriteLine("Server error - waiting before retry");
        }
    }
}
```

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
