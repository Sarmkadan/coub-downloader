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

## PerformanceMonitor

`PerformanceMonitor` tracks and analyzes the performance of application operations by measuring execution time, success/failure rates, and resource utilization. It provides detailed metrics for individual operations as well as aggregated statistics across all monitored operations. The monitor supports both manual and automatic timing through `OperationTimer`, and includes system-level metrics for memory usage, CPU utilization, and garbage collection statistics.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Statistics;
using CoubDownloader.Domain.Models;

public class CoubDownloadService
{
    private readonly PerformanceMonitor _performanceMonitor;
    private readonly ILoggingService _logger;

    public CoubDownloadService(PerformanceMonitor performanceMonitor, ILoggingService logger)
    {
        _performanceMonitor = performanceMonitor;
        _logger = logger;
    }

    public async Task<DownloadResult> DownloadCoubAsync(string coubId, CancellationToken cancellationToken)
    {
        using var operationTimer = _performanceMonitor.StartOperation("DownloadCoub");
        
        try
        {
            // Simulate download operation
            await Task.Delay(150, cancellationToken);
            
            operationTimer.MarkSuccess();
            return new DownloadResult { Success = true, DurationMs = operationTimer.TotalTimeMs };
        }
        catch (Exception ex)
        {
            operationTimer.MarkFailed();
            _logger.LogError($"Failed to download coub {coubId}", ex, "DownloadService");
            return new DownloadResult { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    public void PrintPerformanceSummary()
    {
        var metrics = _performanceMonitor.GetMetrics("DownloadCoub");
        if (metrics != null)
        {
            Console.WriteLine($"Operation: {metrics.Name}");
            Console.WriteLine($"Total executions: {metrics.TotalCount}");
            Console.WriteLine($"Success rate: {(double)metrics.SuccessCount / metrics.TotalCount:P0}");
            Console.WriteLine($"Average time: {metrics.AverageTimeMs:F2} ms");
            Console.WriteLine($"Min/Max time: {metrics.MinTimeMs}/{metrics.MaxTimeMs} ms");
        }
        
        Console.WriteLine($"\nOverall performance:");
        Console.WriteLine(_performanceMonitor.GetSummaryReport());
    }
    
    public void LogSystemMetrics()
    {
        var memoryUsage = PerformanceMonitor.GetMemoryUsageMb();
        var cpuUsage = PerformanceMonitor.GetCpuUsagePercent();
        var gcStats = PerformanceMonitor.GetGcStatistics();
        
        _logger.LogInfo($"System metrics - Memory: {memoryUsage} MB, CPU: {cpuUsage:F1}%, " +
                       $"GC Gen0: {gcStats.Gen0Collections}, Gen1: {gcStats.Gen1Collections}, Gen2: {gcStats.Gen2Collections}",
                       "PerformanceMonitor");
    }
}

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

## PerformanceMonitor

`PerformanceMonitor` tracks and analyzes the performance of application operations by measuring execution time, success/failure rates, and resource utilization. It provides detailed metrics for individual operations as well as aggregated statistics across all monitored operations. The monitor supports both manual and automatic timing through `OperationTimer`, and includes system-level metrics for memory usage, CPU utilization, and garbage collection statistics.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Statistics;
using CoubDownloader.Domain.Models;

public class CoubDownloadService
{
    private readonly PerformanceMonitor _performanceMonitor;
    private readonly ILoggingService _logger;

    public CoubDownloadService(PerformanceMonitor performanceMonitor, ILoggingService logger)
    {
        _performanceMonitor = performanceMonitor;
        _logger = logger;
    }

    public async Task<DownloadResult> DownloadCoubAsync(string coubId, CancellationToken cancellationToken)
    {
        using var operationTimer = _performanceMonitor.StartOperation("DownloadCoub");
        
        try
        {
            // Simulate download operation
            await Task.Delay(150, cancellationToken);
            
            operationTimer.MarkSuccess();
            return new DownloadResult { Success = true, DurationMs = operationTimer.TotalTimeMs };
        }
        catch (Exception ex)
        {
            operationTimer.MarkFailed();
            _logger.LogError($"Failed to download coub {coubId}", ex, "DownloadService");
            return new DownloadResult { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    public void PrintPerformanceSummary()
    {
        var metrics = _performanceMonitor.GetMetrics("DownloadCoub");
        if (metrics != null)
        {
            Console.WriteLine($"Operation: {metrics.Name}");
            Console.WriteLine($"Total executions: {metrics.TotalCount}");
            Console.WriteLine($"Success rate: {(double)metrics.SuccessCount / metrics.TotalCount:P0}");
            Console.WriteLine($"Average time: {metrics.AverageTimeMs:F2} ms");
            Console.WriteLine($"Min/Max time: {metrics.MinTimeMs}/{metrics.MaxTimeMs} ms");
        }
        
        Console.WriteLine($"\nOverall performance:");
        Console.WriteLine(_performanceMonitor.GetSummaryReport());
    }
    
    public void LogSystemMetrics()
    {
        var memoryUsage = PerformanceMonitor.GetMemoryUsageMb();
        var cpuUsage = PerformanceMonitor.GetCpuUsagePercent();
        var gcStats = PerformanceMonitor.GetGcStatistics();
        
        _logger.LogInfo($"System metrics - Memory: {memoryUsage} MB, CPU: {cpuUsage:F1}%, " +
                       $"GC Gen0: {gcStats.Gen0Collections}, Gen1: {gcStats.Gen1Collections}, Gen2: {gcStats.Gen2Collections}",
                       "PerformanceMonitor");
    }
}

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

## PerformanceMonitor

`PerformanceMonitor` tracks and analyzes the performance of application operations by measuring execution time, success/failure rates, and resource utilization. It provides detailed metrics for individual operations as well as aggregated statistics across all monitored operations. The monitor supports both manual and automatic timing through `OperationTimer`, and includes system-level metrics for memory usage, CPU utilization, and garbage collection statistics.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Statistics;
using CoubDownloader.Domain.Models;

public class CoubDownloadService
{
    private readonly PerformanceMonitor _performanceMonitor;
    private readonly ILoggingService _logger;

    public CoubDownloadService(PerformanceMonitor performanceMonitor, ILoggingService logger)
    {
        _performanceMonitor = performanceMonitor;
        _logger = logger;
    }

    public async Task<DownloadResult> DownloadCoubAsync(string coubId, CancellationToken cancellationToken)
    {
        using var operationTimer = _performanceMonitor.StartOperation("DownloadCoub");
        
        try
        {
            // Simulate download operation
            await Task.Delay(150, cancellationToken);
            
            operationTimer.MarkSuccess();
            return new DownloadResult { Success = true, DurationMs = operationTimer.TotalTimeMs };
        }
        catch (Exception ex)
        {
            operationTimer.MarkFailed();
            _logger.LogError($"Failed to download coub {coubId}", ex, "DownloadService");
            return new DownloadResult { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    public void PrintPerformanceSummary()
    {
        var metrics = _performanceMonitor.GetMetrics("DownloadCoub");
        if (metrics != null)
        {
            Console.WriteLine($"Operation: {metrics.Name}");
            Console.WriteLine($"Total executions: {metrics.TotalCount}");
            Console.WriteLine($"Success rate: {(double)metrics.SuccessCount / metrics.TotalCount:P0}");
            Console.WriteLine($"Average time: {metrics.AverageTimeMs:F2} ms");
            Console.WriteLine($"Min/Max time: {metrics.MinTimeMs}/{metrics.MaxTimeMs} ms");
        }
        
        Console.WriteLine($"\nOverall performance:");
        Console.WriteLine(_performanceMonitor.GetSummaryReport());
    }
    
    public void LogSystemMetrics()
    {
        var memoryUsage = PerformanceMonitor.GetMemoryUsageMb();
        var cpuUsage = PerformanceMonitor.GetCpuUsagePercent();
        var gcStats = PerformanceMonitor.GetGcStatistics();
        
        _logger.LogInfo($"System metrics - Memory: {memoryUsage} MB, CPU: {cpuUsage:F1}%, " +
                       $"GC Gen0: {gcStats.Gen0Collections}, Gen1: {gcStats.Gen1Collections}, Gen2: {gcStats.Gen2Collections}",
                       "PerformanceMonitor");
    }
}
