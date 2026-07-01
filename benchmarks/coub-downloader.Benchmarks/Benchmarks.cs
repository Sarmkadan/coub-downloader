using BenchmarkDotNet.Attributes;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Extensions;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Benchmarks;

[MemoryDiagnoser]
public class DomainBenchmarks
{
    private CoubVideo _video = default!;
    private DownloadResult _downloadResult = default!;
    private ConversionSettings _conversionSettings = default!;
    private BatchJob _batchJob = default!;

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

    [Benchmark]
    public string GetFormattedViewCount()
    {
        return _video.GetFormattedViewCount();
    }

    [Benchmark]
    public string GetFormattedFileSize()
    {
        return _downloadResult.GetFormattedFileSize();
    }

    [Benchmark]
    public long EstimateOutputSize()
    {
        return _conversionSettings.EstimateOutputSize(60); // 60 seconds
    }

    [Benchmark]
    public int GetProgressPercent()
    {
        return _batchJob.GetProgressPercent();
    }
}
