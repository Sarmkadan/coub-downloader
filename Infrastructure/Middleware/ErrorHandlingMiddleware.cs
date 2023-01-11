// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Exceptions;

namespace CoubDownloader.Infrastructure.Middleware;

/// <summary>Centralized error handling and recovery strategies</summary>
public class ErrorHandlingMiddleware
{
    private readonly ILoggingService _logger;
    private readonly Dictionary<Type, Func<Exception, ErrorResponse>> _handlers = [];

    public ErrorHandlingMiddleware(ILoggingService logger)
    {
        _logger = logger;
        RegisterDefaultHandlers();
    }

    /// <summary>Handle an exception and return a structured error response</summary>
    public ErrorResponse HandleError(Exception ex)
    {
        var exceptionType = ex.GetType();

        if (_handlers.TryGetValue(exceptionType, out var handler))
        {
            var response = handler(ex);
            _logger.LogError(response.Message, ex, response.Category);
            return response;
        }

        var defaultResponse = new ErrorResponse
        {
            StatusCode = 500,
            Message = "An unexpected error occurred",
            ErrorType = exceptionType.Name,
            Category = "System",
            Timestamp = DateTime.UtcNow,
            Details = ex.Message
        };

        _logger.LogError(defaultResponse.Message, ex, "Unhandled");
        return defaultResponse;
    }

    /// <summary>Register a custom error handler for a specific exception type</summary>
    public void RegisterHandler<TException>(Func<TException, ErrorResponse> handler)
        where TException : Exception
    {
        _handlers[typeof(TException)] = ex => handler((TException)ex);
    }

    private void RegisterDefaultHandlers()
    {
        RegisterHandler<CoubDownloaderException>(ex => new ErrorResponse
        {
            StatusCode = 400,
            Message = ex.Message,
            ErrorType = "CoubDownloaderException",
            Category = "Application",
            Timestamp = DateTime.UtcNow,
            Details = ex.InnerException?.Message
        });

        RegisterHandler<ArgumentException>(ex => new ErrorResponse
        {
            StatusCode = 400,
            Message = "Invalid argument",
            ErrorType = "ArgumentException",
            Category = "Validation",
            Timestamp = DateTime.UtcNow,
            Details = ex.Message
        });

        RegisterHandler<FileNotFoundException>(ex => new ErrorResponse
        {
            StatusCode = 404,
            Message = "File not found",
            ErrorType = "FileNotFoundException",
            Category = "IO",
            Timestamp = DateTime.UtcNow,
            Details = ex.Message
        });

        RegisterHandler<TimeoutException>(ex => new ErrorResponse
        {
            StatusCode = 408,
            Message = "Operation timed out",
            ErrorType = "TimeoutException",
            Category = "Timeout",
            Timestamp = DateTime.UtcNow,
            Details = ex.Message
        });

        RegisterHandler<InvalidOperationException>(ex => new ErrorResponse
        {
            StatusCode = 409,
            Message = "Operation is not valid in current state",
            ErrorType = "InvalidOperationException",
            Category = "State",
            Timestamp = DateTime.UtcNow,
            Details = ex.Message
        });
    }
}

/// <summary>Structured error response model</summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = "";
    public string ErrorType { get; set; } = "";
    public string Category { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>Retry policy with exponential backoff</summary>
public class RetryPolicy
{
    public int MaxRetries { get; set; } = 3;
    public int InitialDelayMs { get; set; } = 100;
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>Execute action with automatic retry on failure</summary>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        int attempt = 0;
        int delayMs = InitialDelayMs;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                attempt++;
                await Task.Delay(delayMs);
                delayMs = (int)(delayMs * BackoffMultiplier);
            }
        }
    }

    /// <summary>Execute action synchronously with automatic retry</summary>
    public T Execute<T>(Func<T> operation)
    {
        int attempt = 0;
        int delayMs = InitialDelayMs;

        while (true)
        {
            try
            {
                return operation();
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                attempt++;
                Thread.Sleep(delayMs);
                delayMs = (int)(delayMs * BackoffMultiplier);
            }
        }
    }
}
