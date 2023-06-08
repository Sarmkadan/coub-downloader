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
```

## RateLimitingService

`RateLimitingService` prevents API abuse by enforcing a fixed number of requests per time window. It tracks request counts per identifier and blocks excess requests until the window resets. This is useful for controlling access to external APIs or internal services.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Middleware;

public class UserService
{
    private readonly RateLimitingService _rateLimiter;

    public UserService()
    {
        // Allow 100 requests per 60 seconds
        _rateLimiter = new RateLimitingService(maxRequestsPerWindow: 100, windowSeconds: 60);
    }

    public async Task<ApiResponse> GetUserAsync(string userId)
    {
        if (!_rateLimiter.IsAllowed(userId))
        {
            var status = _rateLimiter.GetStatus(userId);
            throw new InvalidOperationException($"Rate limit exceeded. {status.RequestsRemaining} requests remaining. Reset in {status.SecondsUntilReset} seconds.");
        }

        // Execute API call
        var response = await CallExternalApiAsync(userId);
        
        // Reset rate limit for this user after successful call
        _rateLimiter.Reset(userId);
        return response;
    }

    private Task<ApiResponse> CallExternalApiAsync(string userId)
    {
        // Implementation of external API call
        return Task.FromResult(new ApiResponse());
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
```
