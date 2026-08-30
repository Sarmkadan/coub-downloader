using System;
using System.Threading;
using System.Threading.Tasks;
using CoubDownloader.Infrastructure.Utilities;
using Xunit;

namespace CoubDownloader.Tests;

/// <summary>
/// Tests for the RetryHelper class.
/// </summary>
public class RetryHelperTests
{
    /// <summary>
    /// Tests that a successful operation is executed only once.
    /// </summary>
    [Fact]
    public async Task ExecuteWithRetryAsync_ShouldReturnOnFirstSuccessfulAttempt()
    {
        var attempts = 0;

        var result = await RetryHelper.ExecuteWithRetryAsync(() =>
        {
            attempts++;
            return Task.FromResult("success");
        }, initialDelayMs: 0);

        Assert.Equal("success", result);
        Assert.Equal(1, attempts);
    }

    /// <summary>
    /// Tests that transient failures are retried until the operation succeeds.
    /// </summary>
    [Fact]
    public async Task ExecuteWithRetryAsync_ShouldSucceedAfterTransientFailures()
    {
        const int transientFailures = 3;
        var attempts = 0;

        var result = await RetryHelper.ExecuteWithRetryAsync(() =>
        {
            attempts++;
            return attempts <= transientFailures
                ? Task.FromException<int>(new InvalidOperationException("Transient failure"))
                : Task.FromResult(42);
        }, maxRetries: transientFailures, initialDelayMs: 0);

        Assert.Equal(42, result);
        Assert.Equal(transientFailures + 1, attempts);
    }

    /// <summary>
    /// Tests that the exception from the final attempt is rethrown after retries are exhausted.
    /// </summary>
    [Fact]
    public async Task ExecuteWithRetryAsync_ShouldRethrowLastExceptionWhenRetriesAreExhausted()
    {
        var attempts = 0;
        var expected = new InvalidOperationException("Final failure");

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            RetryHelper.ExecuteWithRetryAsync<int>(() =>
            {
                attempts++;
                var exception = attempts == 3
                    ? expected
                    : new InvalidOperationException($"Failure {attempts}");
                return Task.FromException<int>(exception);
            }, maxRetries: 2, initialDelayMs: 0));

        Assert.Same(expected, actual);
        Assert.Equal(3, attempts);
    }

    /// <summary>
    /// Tests that cancellation from the operation is propagated without retrying.
    /// </summary>
    [Fact]
    public async Task ExecuteWithRetryAsync_ShouldPropagateCancellation()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var attempts = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            RetryHelper.ExecuteWithRetryAsync<int>(() =>
            {
                attempts++;
                return Task.FromCanceled<int>(source.Token);
            }, initialDelayMs: 0, shouldRetry: (exception, _) => exception is not OperationCanceledException));

        Assert.Equal(1, attempts);
    }

    /// <summary>
    /// Tests that a null operation fails immediately.
    /// </summary>
    [Fact]
    public async Task ExecuteWithRetryAsync_ShouldRejectNullOperation()
    {
        Func<Task<int>> operation = null!;

        await Assert.ThrowsAsync<NullReferenceException>(() =>
            RetryHelper.ExecuteWithRetryAsync(operation, initialDelayMs: 0));
    }

    /// <summary>
    /// Tests that a negative retry count disables retries.
    /// </summary>
    [Fact]
    public async Task ExecuteWithRetryAsync_ShouldNotRetryWhenRetryCountIsNegative()
    {
        var attempts = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            RetryHelper.ExecuteWithRetryAsync<int>(() =>
            {
                attempts++;
                return Task.FromException<int>(new InvalidOperationException("Failure"));
            }, maxRetries: -1, initialDelayMs: 0));

        Assert.Equal(1, attempts);
    }
}
