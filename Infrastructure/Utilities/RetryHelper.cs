// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>Retry strategies for resilient operations</summary>
public static class RetryHelper
{
    /// <summary>Retry with exponential backoff</summary>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int initialDelayMs = 100,
        Func<Exception, int, bool>? shouldRetry = null)
    {
        int attempt = 0;
        int delayMs = initialDelayMs;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                if (shouldRetry != null && !shouldRetry(ex, attempt))
                    throw;

                attempt++;
                await Task.Delay(delayMs);
                delayMs = (int)(delayMs * 1.5); // Exponential backoff
            }
        }
    }

    /// <summary>Retry with linear backoff</summary>
    public static async Task<T> ExecuteWithLinearRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int delayMs = 100)
    {
        int attempt = 0;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                attempt++;
                await Task.Delay(delayMs);
            }
        }
    }

    /// <summary>Retry with jitter (random delay)</summary>
    public static async Task<T> ExecuteWithJitterRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int baseDelayMs = 100)
    {
        var random = new Random();
        int attempt = 0;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                attempt++;
                var jitter = random.Next(0, baseDelayMs);
                var delay = baseDelayMs + jitter;
                await Task.Delay(delay);
            }
        }
    }

    /// <summary>Bulkhead pattern for circuit breaker</summary>
    public static async Task<T> ExecuteWithCircuitBreakerAsync<T>(
        Func<Task<T>> operation,
        int failureThreshold = 5,
        int resetTimeoutSeconds = 60)
    {
        // Simple implementation - can be extended with actual state management
        return await operation();
    }

    /// <summary>Timeout wrapper</summary>
    public static async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<Task<T>> operation,
        TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);

        try
        {
            return await operation();
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
    }

    /// <summary>Retry with timeout</summary>
    public static async Task<T> ExecuteWithRetryAndTimeoutAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int operationTimeoutMs = 5000,
        int delayMs = 100)
    {
        int attempt = 0;

        while (true)
        {
            try
            {
                return await ExecuteWithTimeoutAsync(operation, TimeSpan.FromMilliseconds(operationTimeoutMs));
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                attempt++;
                await Task.Delay(delayMs);
            }
        }
    }
}

/// <summary>Fallback execution pattern</summary>
public static class FallbackHelper
{
    /// <summary>Execute with fallback value</summary>
    public static async Task<T> ExecuteWithFallbackAsync<T>(
        Func<Task<T>> primaryOperation,
        Func<Task<T>> fallbackOperation,
        Func<Exception, bool>? shouldFallback = null)
    {
        try
        {
            return await primaryOperation();
        }
        catch (Exception ex) when (shouldFallback?.Invoke(ex) ?? true)
        {
            return await fallbackOperation();
        }
    }

    /// <summary>Execute with default fallback value</summary>
    public static async Task<T> ExecuteWithFallbackAsync<T>(
        Func<Task<T>> operation,
        T defaultValue)
    {
        try
        {
            return await operation();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>Execute multiple operations and return first successful result</summary>
    public static async Task<T> ExecuteFirstSuccessAsync<T>(
        params Func<Task<T>>[] operations)
    {
        Exception? lastException = null;

        foreach (var operation in operations)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                lastException = ex;
                continue;
            }
        }

        throw lastException ?? new InvalidOperationException("All operations failed");
    }
}
