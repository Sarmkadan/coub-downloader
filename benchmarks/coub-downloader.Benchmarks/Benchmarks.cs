using BenchmarkDotNet.Attributes;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Extensions;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Benchmarks;

/// <summary>
/// A benchmark class for measuring the performance of various domain operations.
/// </summary>
[MemoryDiagnoser]
public class DomainBenchmarks
{
    private CoubVideo _video = default!;
    private DownloadResult _downloadResult = default!;
    private ConversionSettings _conversionSettings = default!;
    private BatchJob _batchJob = default!;

    /// <summary>
    /// Initializes the benchmark by setting up the test data.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _video = new CoubVideo { ViewCount = 1_234_567, Width = 1920, Height = 1080, Duration = 20 };
        _downloadResult = new DownloadResult { OutputFileSizeBytes = 10 * 1024 * 1024 }; // 10MB
        _conversionSettings = new ConversionSettings { VideoBitrate = 5000, AudioBitrate = 128 };
        
        _batchJob = new BatchJob();
        for (int i = 0; i < 100; i++)
        {
            _batchJob.Tasks.Add(new DownloadTask { State = i < 50 ? ProcessingState.Completed : ProcessingState.Pending });
        }
    }

    /// <summary>
    /// Measures the time it takes to format the view count of a Coub video.
    /// </summary>
    /// <returns>The formatted view count as a string.</returns>
    [Benchmark]
    public string GetFormattedViewCount()
    {
        return _video.GetFormattedViewCount();
    }

    /// <summary>
    /// Measures the time it takes to format the file size of a download result.
    /// </summary>
    /// <returns>The formatted file size as a string.</returns>
    [Benchmark]
    public string GetFormattedFileSize()
    {
        return _downloadResult.GetFormattedFileSize();
    }

    /// <summary>
    /// Measures the time it takes to estimate the output size of a conversion settings object.
    /// </summary>
    /// <param name="durationInSeconds">The duration in seconds.</param>
    /// <returns>The estimated output size in bytes.</returns>
    [Benchmark]
    public long EstimateOutputSize()
    {
        return _conversionSettings.EstimateOutputSize(60); // 60 seconds
    }

    /// <summary>
    /// Measures the time it takes to calculate the progress percentage of a batch job.
    /// </summary>
    /// <returns>The progress percentage as an integer.</returns>
    [Benchmark]
    public int GetProgressPercent()
    {
        return _batchJob.GetProgressPercent();
    }
}
