using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoubDownloader.Infrastructure.Middleware;
using Xunit;

namespace CoubDownloader.Tests;

/// <summary>
/// Tests for the RateLimitingService class.
/// </summary>
public class RateLimitingServiceTests
{
    /// <summary>
    /// Tests that concurrent callers never get more requests than the window allows.
    /// </summary>
    [Fact]
    public async Task IsAllowed_ConcurrentCallers_ShouldNotExceedMaxRequests()
    {
        const int maxRequests = 100;
        const int totalCalls = 10_000;
        var limiter = new RateLimitingService(maxRequests, windowSeconds: 60);
        var allowed = 0;

        await Parallel.ForEachAsync(
            Enumerable.Range(0, totalCalls),
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 4 },
            (_, _) =>
            {
                if (limiter.IsAllowed("concurrent"))
                {
                    Interlocked.Increment(ref allowed);
                }

                return ValueTask.CompletedTask;
            });

        Assert.Equal(maxRequests, allowed);
        Assert.Equal(0, limiter.GetStatus("concurrent").RequestsRemaining);
    }

    /// <summary>
    /// Tests that concurrent reads of status while requests are being admitted do not throw or go negative.
    /// </summary>
    [Fact]
    public async Task GetStatus_ConcurrentWithIsAllowed_ShouldStayConsistent()
    {
        const int maxRequests = 50;
        var limiter = new RateLimitingService(maxRequests, windowSeconds: 60);
        var negativeSeen = 0;

        var writer = Task.Run(() =>
        {
            for (var i = 0; i < 5_000; i++)
            {
                limiter.IsAllowed("mixed");
            }
        });

        var reader = Task.Run(() =>
        {
            for (var i = 0; i < 5_000; i++)
            {
                if (limiter.GetStatus("mixed").RequestsRemaining < 0)
                {
                    Interlocked.Increment(ref negativeSeen);
                }
            }
        });

        await Task.WhenAll(writer, reader);

        Assert.Equal(0, negativeSeen);
        Assert.Equal(0, limiter.GetStatus("mixed").RequestsRemaining);
    }
}
