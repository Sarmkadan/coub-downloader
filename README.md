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

## ExportService

`ExportService` provides report generation and export capabilities for batch jobs and download results. It supports multiple export formats (JSON, CSV, XML, HTML) and includes a fluent API for building custom reports through the `ReportBuilder` class. The service integrates with `ILoggingService` to log export operations and errors.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Reporting;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

public class ReportGenerationService
{
    private readonly ExportService _exportService;
    private readonly ILoggingService _logger;

    public ReportGenerationService(ExportService exportService, ILoggingService logger)
    {
        _exportService = exportService;
        _logger = logger;
    }

    public async Task GenerateBatchReportAsync(BatchJob batchJob)
    {
        // Export batch report as JSON
        var jsonSuccess = await _exportService.ExportBatchReportAsync(
            batchJob,
            "/reports/batch_job.json",
            ExportFormat.Json);
        
        if (jsonSuccess)
        {
            _logger.LogInfo("Batch report exported successfully", "ReportGenerationService");
        }

        // Export batch report as HTML for viewing
        var htmlReport = _exportService.GenerateHtmlReport(batchJob);
        await File.WriteAllTextAsync("/reports/batch_job.html", htmlReport);

        // Export download results as CSV
        var results = batchJob.Tasks
            .Where(t => t.State == ProcessingState.Completed)
            .Select(t => new DownloadResult
            {
                TaskId = t.Id,
                Success = true,
                OutputFilePath = $"/downloads/{t.Id}.mp4",
                OutputFileSizeBytes = 1024 * 1024 * 50, // 50 MB
                ProcessingTimeMs = 15000,
                Format = t.Format,
                Quality = t.Quality
            })
            .ToList();

        var csvSuccess = await _exportService.ExportDownloadResultsAsync(
            results,
            "/reports/download_results.csv",
            ExportFormat.Csv);

        // Use ReportBuilder for custom reports
        var customReport = new ReportBuilder()
            .AddSection("Batch Summary", $"Total: {batchJob.Tasks.Count} tasks")
            .AddTable("Configuration", new Dictionary<string, string>
            {
                {"Max Concurrent Downloads", "5"},
                {"Timeout Seconds", "30"},
                {"Output Directory", "/home/user/downloads"}
            })
            .Build();

        Console.WriteLine(customReport);
    }
}
```

## ApplicationConfiguration

`ApplicationConfiguration` provides centralized configuration management for the Coub Downloader application. It consolidates all application settings including download parameters, conversion settings, caching configuration, logging preferences, and API credentials into a single strongly-typed configuration object. This approach ensures type safety, improves maintainability, and simplifies dependency injection throughout the application.

The configuration is typically loaded from JSON files or environment variables and validated during application startup, with sensible defaults provided for optional settings.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure services with ApplicationConfiguration
var services = new ServiceCollection();

services.AddSingleton<IConfigurationManager>(provider => new ConfigurationManager
{
    // Download settings
    Download = new DownloadSettings
    {
        Enabled = true,
        MaxConcurrentDownloads = 5,
        TimeoutSeconds = 30,
        MaxRetries = 3,
        VerifyFileIntegrity = true,
        MaxFileSizeBytes = 500 * 1024 * 1024 // 500 MB
    },
    
    // Conversion settings
    Conversion = new ConversionSettings
    {
        Enabled = true,
        MaxConcurrentConversions = 3,
        TimeoutSeconds = 60,
        VideoCodec = "libx264",
        AudioCodec = "aac",
        DefaultQuality = 720
    },
    
    // Cache settings
    Cache = new CacheSettings
    {
        Enabled = true,
        DefaultTtlSeconds = 86400 // 24 hours
    },
    
    // Logging settings
    Logging = new LoggingSettings
    {
        LogLevel = "Information",
        LogToFile = true,
        LogDirectory = "/var/log/coub-downloader"
    },
    
    // API settings
    Api = new ApiSettings
    {
        CoubApiBaseUrl = "https://coub.com/api/v2",
        TimeoutSeconds = 15
    },
    
    // Application settings
    OutputDirectory = "/home/user/coub-downloads",
    FfmpegPath = "/usr/bin/ffmpeg",
    EnableHardwareAcceleration = true
});

// Usage in a service
public class DownloadService
{
    private readonly ApplicationConfiguration _config;
    private readonly ILoggingService _logger;
    
    public DownloadService(ApplicationConfiguration config, ILoggingService logger)
    {
        _config = config;
        _logger = logger;
    }
    
    public void ConfigureDownloadSettings()
    {
        _logger.LogInfo($"Max concurrent downloads: {_config.Download.MaxConcurrentDownloads}");
        _logger.LogInfo($"Output directory: {_config.OutputDirectory}");
        _logger.LogInfo($"FFmpeg path: {_config.FfmpegPath}");
        
        if (_config.EnableHardwareAcceleration)
        {
            _logger.LogInfo("Hardware acceleration is enabled");
        }
    }
}
```

## InMemoryBatchJobRepository

`InMemoryBatchJobRepository` provides an in-memory implementation of `IBatchJobRepository` for managing batch jobs. It stores batch jobs in a thread-safe dictionary and supports all standard CRUD operations along with specialized queries for filtering by state, searching by name, and retrieving recent jobs. This implementation is ideal for testing, development, or scenarios where persistence is not required.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Repositories;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

// Example 1: Basic usage with dependency injection
public class BatchJobService
{
    private readonly InMemoryBatchJobRepository _repository;

    public BatchJobService(InMemoryBatchJobRepository repository)
    {
        _repository = repository;
    }

    public async Task ManageBatchJobsAsync()
    {
        // Create a new batch job
        var batchJob = new BatchJob
        {
            Name = "Download Coubs",
            Description = "Download popular coubs for offline viewing",
            State = ProcessingState.Queued,
            Tasks = new List<BatchTask>(),
            MaxConcurrentTasks = 5
        };

        var createdJob = await _repository.CreateAsync(batchJob);
        Console.WriteLine($"Created batch job with ID: {createdJob.Id}");

        // Get all batch jobs
        var allJobs = await _repository.GetAllAsync();
        Console.WriteLine($"Total batch jobs: {allJobs.Count()}");

        // Search by name
        var matchingJobs = await _repository.SearchByNameAsync("Download");
        Console.WriteLine($"Jobs matching 'Download': {matchingJobs.Count()}");

        // Get jobs by state
        var queuedJobs = await _repository.GetByStateAsync(ProcessingState.Queued);
        Console.WriteLine($"Queued jobs: {queuedJobs.Count()}");

        // Update progress
        await _repository.UpdateProgressAsync(createdJob.Id, completed: 10, failed: 0);

        // Check if batch exists
        var exists = await _repository.ExistsAsync(createdJob.Id);
        Console.WriteLine($"Batch exists: {exists}");

        // Get recent jobs
        var recentJobs = await _repository.GetRecentAsync(5);
        Console.WriteLine($"Recent jobs: {recentJobs.Count()}");
    }
}

// Example 2: Manual instantiation for testing
var repository = new InMemoryBatchJobRepository();

// Create a test batch job
var testJob = new BatchJob
{
    Name = "Test Download",
    Description = "Test batch job",
    State = ProcessingState.InProgress,
    MaxConcurrentTasks = 3
};

var created = await repository.CreateAsync(testJob);

// Update the job
created.Description = "Updated test batch job";
var updated = await repository.UpdateAsync(created);

// Get by ID
var fetched = await repository.GetByIdAsync(updated.Id);

// Delete
var deleted = await repository.DeleteAsync(updated.Id);
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

## ApplicationConfiguration

`ApplicationConfiguration` provides centralized configuration management for the Coub Downloader application. It consolidates all application settings including download parameters, conversion settings, caching configuration, logging preferences, and API credentials into a single strongly-typed configuration object. This approach ensures type safety, improves maintainability, and simplifies dependency injection throughout the application.

The configuration is typically loaded from JSON files or environment variables and validated during application startup, with sensible defaults provided for optional settings.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure services with ApplicationConfiguration
var services = new ServiceCollection();

services.AddSingleton<IConfigurationManager>(provider => new ConfigurationManager
{
    // Download settings
    Download = new DownloadSettings
    {
        Enabled = true,
        MaxConcurrentDownloads = 5,
        TimeoutSeconds = 30,
        MaxRetries = 3,
        VerifyFileIntegrity = true,
        MaxFileSizeBytes = 500 * 1024 * 1024 // 500 MB
    },
    
    // Conversion settings
    Conversion = new ConversionSettings
    {
        Enabled = true,
        MaxConcurrentConversions = 3,
        TimeoutSeconds = 60,
        VideoCodec = "libx264",
        AudioCodec = "aac",
        DefaultQuality = 720
    },
    
    // Cache settings
    Cache = new CacheSettings
    {
        Enabled = true,
        DefaultTtlSeconds = 86400 // 24 hours
    },
    
    // Logging settings
    Logging = new LoggingSettings
    {
        LogLevel = "Information",
        LogToFile = true,
        LogDirectory = "/var/log/coub-downloader"
    },
    
    // API settings
    Api = new ApiSettings
    {
        CoubApiBaseUrl = "https://coub.com/api/v2",
        TimeoutSeconds = 15
    },
    
    // Application settings
    OutputDirectory = "/home/user/coub-downloads",
    FfmpegPath = "/usr/bin/ffmpeg",
    EnableHardwareAcceleration = true
});

// Usage in a service
public class DownloadService
{
    private readonly ApplicationConfiguration _config;
    private readonly ILoggingService _logger;
    
    public DownloadService(ApplicationConfiguration config, ILoggingService logger)
    {
        _config = config;
        _logger = logger;
    }
    
    public void ConfigureDownloadSettings()
    {
        _logger.LogInfo($"Max concurrent downloads: {_config.Download.MaxConcurrentDownloads}");
        _logger.LogInfo($"Output directory: {_config.OutputDirectory}");
        _logger.LogInfo($"FFmpeg path: {_config.FfmpegPath}");
        
        if (_config.EnableHardwareAcceleration)
        {
            _logger.LogInfo("Hardware acceleration is enabled");
        }
    }
}
```

## InMemoryBatchJobRepository

`InMemoryBatchJobRepository` provides an in-memory implementation of `IBatchJobRepository` for managing batch jobs. It stores batch jobs in a thread-safe dictionary and supports all standard CRUD operations along with specialized queries for filtering by state, searching by name, and retrieving recent jobs. This implementation is ideal for testing, development, or scenarios where persistence is not required.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Repositories;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

// Example 1: Basic usage with dependency injection
public class BatchJobService
{
    private readonly InMemoryBatchJobRepository _repository;

    public BatchJobService(InMemoryBatchJobRepository repository)
    {
        _repository = repository;
    }

    public async Task ManageBatchJobsAsync()
    {
        // Create a new batch job
        var batchJob = new BatchJob
        {
            Name = "Download Coubs",
            Description = "Download popular coubs for offline viewing",
            State = ProcessingState.Queued,
            Tasks = new List<BatchTask>(),
            MaxConcurrentTasks = 5
        };

        var createdJob = await _repository.CreateAsync(batchJob);
        Console.WriteLine($"Created batch job with ID: {createdJob.Id}");

        // Get all batch jobs
        var allJobs = await _repository.GetAllAsync();
        Console.WriteLine($"Total batch jobs: {allJobs.Count()}");

        // Search by name
        var matchingJobs = await _repository.SearchByNameAsync("Download");
        Console.WriteLine($"Jobs matching 'Download': {matchingJobs.Count()}");

        // Get jobs by state
        var queuedJobs = await _repository.GetByStateAsync(ProcessingState.Queued);
        Console.WriteLine($"Queued jobs: {queuedJobs.Count()}");

        // Update progress
        await _repository.UpdateProgressAsync(createdJob.Id, completed: 10, failed: 0);

        // Check if batch exists
        var exists = await _repository.ExistsAsync(createdJob.Id);
        Console.WriteLine($"Batch exists: {exists}");

        // Get recent jobs
        var recentJobs = await _repository.GetRecentAsync(5);
        Console.WriteLine($"Recent jobs: {recentJobs.Count()}");
    }
}

// Example 2: Manual instantiation for testing
var repository = new InMemoryBatchJobRepository();

// Create a test batch job
var testJob = new BatchJob
{
    Name = "Test Download",
    Description = "Test batch job",
    State = ProcessingState.InProgress,
    MaxConcurrentTasks = 3
};

var created = await repository.CreateAsync(testJob);

// Update the job
created.Description = "Updated test batch job";
var updated = await repository.UpdateAsync(created);

// Get by ID
var fetched = await repository.GetByIdAsync(updated.Id);

// Delete
var deleted = await repository.DeleteAsync(updated.Id);
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

## ApplicationConfiguration

`ApplicationConfiguration` provides centralized configuration management for the Coub Downloader application. It consolidates all application settings including download parameters, conversion settings, caching configuration, logging preferences, and API credentials into a single strongly-typed configuration object. This approach ensures type safety, improves maintainability, and simplifies dependency injection throughout the application.

The configuration is typically loaded from JSON files or environment variables and validated during application startup, with sensible defaults provided for optional settings.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure services with ApplicationConfiguration
var services = new ServiceCollection();

services.AddSingleton<IConfigurationManager>(provider => new ConfigurationManager
{
    // Download settings
    Download = new DownloadSettings
    {
        Enabled = true,
        MaxConcurrentDownloads = 5,
        TimeoutSeconds = 30,
        MaxRetries = 3,
        VerifyFileIntegrity = true,
        MaxFileSizeBytes = 500 * 1024 * 1024 // 500 MB
    },
    
    // Conversion settings
    Conversion = new ConversionSettings
    {
        Enabled = true,
        MaxConcurrentConversions = 3,
        TimeoutSeconds = 60,
        VideoCodec = "libx264",
        AudioCodec = "aac",
        DefaultQuality = 720
    },
    
    // Cache settings
    Cache = new CacheSettings
    {
        Enabled = true,
        DefaultTtlSeconds = 86400 // 24 hours
    },
    
    // Logging settings
    Logging = new LoggingSettings
    {
        LogLevel = "Information",
        LogToFile = true,
        LogDirectory = "/var/log/coub-downloader"
    },
    
    // API settings
    Api = new ApiSettings
    {
        CoubApiBaseUrl = "https://coub.com/api/v2",
        TimeoutSeconds = 15
    },
    
    // Application settings
    OutputDirectory = "/home/user/coub-downloads",
    FfmpegPath = "/usr/bin/ffmpeg",
    EnableHardwareAcceleration = true
});

// Usage in a service
public class DownloadService
{
    private readonly ApplicationConfiguration _config;
    private readonly ILoggingService _logger;
    
    public DownloadService(ApplicationConfiguration config, ILoggingService logger)
    {
        _config = config;
        _logger = logger;
    }
    
    public void ConfigureDownloadSettings()
    {
        _logger.LogInfo($"Max concurrent downloads: {_config.Download.MaxConcurrentDownloads}");
        _logger.LogInfo($"Output directory: {_config.OutputDirectory}");
        _logger.LogInfo($"FFmpeg path: {_config.FfmpegPath}");
        
        if (_config.EnableHardwareAcceleration)
        {
            _logger.LogInfo("Hardware acceleration is enabled");
        }
    }
}
```

## InMemoryBatchJobRepository

`InMemoryBatchJobRepository` provides an in-memory implementation of `IBatchJobRepository` for managing batch jobs. It stores batch jobs in a thread-safe dictionary and supports all standard CRUD operations along with specialized queries for filtering by state, searching by name, and retrieving recent jobs. This implementation is ideal for testing, development, or scenarios where persistence is not required.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Repositories;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

// Example 1: Basic usage with dependency injection
public class BatchJobService
{
    private readonly InMemoryBatchJobRepository _repository;

    public BatchJobService(InMemoryBatchJobRepository repository)
    {
        _repository = repository;
    }

    public async Task ManageBatchJobsAsync()
    {
        // Create a new batch job
        var batchJob = new BatchJob
        {
            Name = "Download Coubs",
            Description = "Download popular coubs for offline viewing",
            State = ProcessingState.Queued,
            Tasks = new List<BatchTask>(),
            MaxConcurrentTasks = 5
        };

        var createdJob = await _repository.CreateAsync(batchJob);
        Console.WriteLine($"Created batch job with ID: {createdJob.Id}");

        // Get all batch jobs
        var allJobs = await _repository.GetAllAsync();
        Console.WriteLine($"Total batch jobs: {allJobs.Count()}");

        // Search by name
        var matchingJobs = await _repository.SearchByNameAsync("Download");
        Console.WriteLine($"Jobs matching 'Download': {matchingJobs.Count()}");

        // Get jobs by state
        var queuedJobs = await _repository.GetByStateAsync(ProcessingState.Queued);
        Console.WriteLine($"Queued jobs: {queuedJobs.Count()}");

        // Update progress
        await _repository.UpdateProgressAsync(createdJob.Id, completed: 10, failed: 0);

        // Check if batch exists
        var exists = await _repository.ExistsAsync(createdJob.Id);
        Console.WriteLine($"Batch exists: {exists}");

        // Get recent jobs
        var recentJobs = await _repository.GetRecentAsync(5);
        Console.WriteLine($"Recent jobs: {recentJobs.Count()}");
    }
}

// Example 2: Manual instantiation for testing
var repository = new InMemoryBatchJobRepository();

// Create a test batch job
var testJob = new BatchJob
{
    Name = "Test Download",
    Description = "Test batch job",
    State = ProcessingState.InProgress,
    MaxConcurrentTasks = 3
};

var created = await repository.CreateAsync(testJob);

// Update the job
created.Description = "Updated test batch job";
var updated = await repository.UpdateAsync(created);

// Get by ID
var fetched = await repository.GetByIdAsync(updated.Id);

// Delete
var deleted = await repository.DeleteAsync(updated.Id);
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
