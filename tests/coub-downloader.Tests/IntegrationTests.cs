using Xunit;
using Moq;
using Moq.Protected;
using FluentAssertions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Models; // Note: CoubVideoInfo is duplicated for testing below
using CoubDownloader.Infrastructure.Integration;
using CoubDownloader.Infrastructure.Repositories;
using CoubDownloader.Infrastructure.Caching;
using CoubDownloader.Infrastructure.Middleware;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using CoubDownloader.Domain.Enums;
using System.Collections.Generic; // Added for List

namespace CoubDownloader.Tests;

// Dummy record for CoubVideoInfo, as it's an internal DTO not exposed publicly
// This is needed because CoubVideoInfo is defined in CoubApiClient.cs, not a shared Domain.Models file
// A better long-term solution is to move CoubVideoInfo to Domain.Models.
public record CoubVideoInfo
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public double Duration { get; set; }
    public bool HasAudio { get; set; }
    public string? ChannelUrl { get; set; }
    public long ViewCount { get; set; }
}

public class IntegrationTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILoggingService> _mockLoggingService;
    // Mock for ICoubDownloadService to control DownloadVideoFileAsync in BatchProcessing test
    private readonly Mock<ICoubDownloadService> _mockCoubDownloadService;
    private readonly HttpClient _httpClient; // Keep httpclient here for manual construction of services

    public IntegrationTests()
    {
        var services = new ServiceCollection();

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _mockCacheService = new Mock<ICacheService>();
        _mockLoggingService = new Mock<ILoggingService>();
        _mockCoubDownloadService = new Mock<ICoubDownloadService>();

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://coub.com/api/v2/")
        };

        // Register mocked dependencies for CoubApiClient
        services.AddSingleton(_httpClient); // Use the mock-backed HttpClient
        services.AddSingleton(_mockCacheService.Object);
        services.AddSingleton(_mockLoggingService.Object);
        services.AddTransient<ICoubApiClient, CoubApiClient>();

        // Register in-memory repositories
        services.AddSingleton<IBatchJobRepository, InMemoryBatchJobRepository>();
        services.AddSingleton<ICoubVideoRepository, InMemoryCoubVideoRepository>();
        services.AddSingleton<IDownloadResultRepository, InMemoryDownloadResultRepository>();
        services.AddSingleton<IDownloadTaskRepository, InMemoryDownloadTaskRepository>();

        // Register application services
        // Register the MOCKED ICoubDownloadService for BatchProcessingService to use
        services.AddSingleton(_mockCoubDownloadService.Object);
        services.AddTransient<IVideoConversionService, VideoConversionService>(); // This will use real ffmpeg/ffprobe
        services.AddTransient<IBatchProcessingService, BatchProcessingService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    // Helper to setup mock HttpMessageHandler for specific responses
    private void SetupHttpResponse(HttpStatusCode statusCode, string? content = null)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content != null ? new StringContent(content) : null
            });
    }

    [Fact]
    public async Task EndToEnd_SingleVideoDownload_Succeeds()
    {
        // Arrange
        var coubUrl = "https://coub.com/view/integrationtest";
        var outputDir = Path.Combine(Path.GetTempPath(), "integration_test_outputs");
        // var finalOutputPath = Path.Combine(outputDir, "integrationtest.mp4"); // Not used as conversion is skipped

        // Ensure output directory exists and is clean
        if (Directory.Exists(outputDir))
        {
            Directory.Delete(outputDir, true);
        }
        Directory.CreateDirectory(outputDir);

        // Mock Coub API client response
        var videoId = "integrationtest";
        var coubApiResponse = new CoubVideoInfo
        {
            Id = videoId,
            Title = "Integration Test Coub",
            Duration = 10.0,
            HasAudio = true,
            ChannelUrl = "test_channel",
            ViewCount = 100
        };
        var jsonResponse = JsonSerializer.Serialize(coubApiResponse);

        SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

        // Mock Http for the actual video file download
        var videoContent = "This is dummy video content.";
        var videoContentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(videoContent));

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("media-source.coub.com")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(videoContentStream)
                {
                    Headers = { ContentLength = videoContent.Length }
                }
            });

        // Manually construct CoubDownloadService to use the shared mock HttpMessageHandler
        var realCoubApiClient = _serviceProvider.GetRequiredService<ICoubApiClient>();
        var videoRepository = _serviceProvider.GetRequiredService<ICoubVideoRepository>();
        var downloadService = new CoubDownloadService(
            _httpClient, // Use the HttpClient that uses our mock HttpMessageHandler
            videoRepository,
            realCoubApiClient);

        // Act
        // 1. Download video metadata and extract source
        var coubVideo = await downloadService.DownloadVideoAsync(coubUrl);
        coubVideo.Should().NotBeNull();
        coubVideo.SourceUrl.Should().Contain("media-source.coub.com");

        // 2. Download the video file to a temporary location
        var tempVideoPath = Path.Combine(outputDir, $"{videoId}.webm");
        await downloadService.DownloadVideoFileAsync(coubVideo.SourceUrl, tempVideoPath);
        File.Exists(tempVideoPath).Should().BeTrue();
        (await File.ReadAllTextAsync(tempVideoPath)).Should().Be(videoContent);

        // Conversion test part is commented out due to dependency on actual FFmpeg binaries.
        // For a true integration test of conversion, FFmpeg/FFprobe must be installed and in PATH.
        // If an environment with ffmpeg/ffprobe is guaranteed, uncomment the following:
        /*
        var videoConversionService = _serviceProvider.GetRequiredService<IVideoConversionService>();
        var conversionSettings = new ConversionSettings
        {
            Format = VideoFormat.Shorts,
            Width = 1080,
            Height = 1920,
            VideoCodec = "h264", // or appropriate codec for shorts
            AudioCodec = "aac"
        };
        var convertedPath = await videoConversionService.ConvertToShortsAsync(tempVideoPath, finalOutputPath);
        File.Exists(convertedPath).Should().BeTrue();
        File.GetLastWriteTime(convertedPath).Should().BeAfter(File.GetLastWriteTime(tempVideoPath));
        */

        // Clean up
        File.Delete(tempVideoPath);
        Directory.Delete(outputDir, true);
    }

    [Fact]
    public async Task EndToEnd_BatchProcessing_SuccessfulCompletion()
    {
        // Arrange
        var batchJobId = "batch1";
        var outputDir = Path.Combine(Path.GetTempPath(), "batch_test_outputs");
        if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
        Directory.CreateDirectory(outputDir);

        var batchName = "Test Batch";
        var urls = new List<string>
        {
            "https://coub.com/view/batchtest1",
            "https://coub.com/view/batchtest2"
        };

        // Mock CoubDownloadService's behavior when called by BatchProcessingService
        // Need to setup GetVideoInfoAsync on _mockCoubDownloadService
        _mockCoubDownloadService.Setup(ds => ds.DownloadVideoAsync(
            It.Is<string>(u => u.Contains("batchtest1")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoubVideo { Id = "batchtest1", Url = urls[0], SourceUrl = "http://media.com/batchtest1.webm" });
        _mockCoubDownloadService.Setup(ds => ds.DownloadVideoAsync(
            It.Is<string>(u => u.Contains("batchtest2")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoubVideo { Id = "batchtest2", Url = urls[1], SourceUrl = "http://media.com/batchtest2.webm" });

        // Mock DownloadVideoFileAsync, called by the mocked CoubDownloadService.DownloadVideoAsync
        _mockCoubDownloadService.Setup(ds => ds.DownloadVideoFileAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns((string sourceUrl, string outputPath, IProgress<int>? progress, CancellationToken ct) =>
            {
                // Simulate file write for each task
                File.WriteAllText(outputPath, $"dummy file content for {Path.GetFileNameWithoutExtension(outputPath)}");
                return Task.FromResult(outputPath);
            });


        var batchService = _serviceProvider.GetRequiredService<IBatchProcessingService>();
        // The downloadService here refers to the mocked one injected into BatchProcessingService
        // No need to get it explicitly here, as BatchProcessingService gets it from DI.

        // Act
        // 1. Create batch job
        var batchJob = await batchService.CreateBatchJobAsync(batchName, outputDir);
        batchJob.Should().NotBeNull();
        batchJob.State.Should().Be(ProcessingState.Pending);

        // 2. Add tasks
        var downloadTasks = urls.Select(url => new DownloadTask { Url = url }).ToList();
        await batchService.AddTasksAsync(batchJob.Id, downloadTasks);

        var updatedBatch = await batchService.GetBatchStatusAsync(batchJob.Id);
        updatedBatch.TotalTasks.Should().Be(urls.Count);

        // 3. Start batch processing
        await batchService.StartBatchAsync(batchJob.Id);

        // Assert
        var finalBatch = await batchService.GetBatchStatusAsync(batchJob.Id);
        finalBatch.State.Should().Be(ProcessingState.Completed);
        finalBatch.Tasks.Should().AllSatisfy(t => t.State.Should().Be(ProcessingState.Completed));
        finalBatch.Tasks.Should().AllSatisfy(t => t.OutputPath.Should().NotBeNullOrEmpty());

        // Verify files were "created" by the mocked DownloadVideoFileAsync
        File.Exists(Path.Combine(outputDir, "batchtest1.mp4")).Should().BeTrue();
        File.Exists(Path.Combine(outputDir, "batchtest2.mp4")).Should().BeTrue();

        // Clean up
        Directory.Delete(outputDir, true);
    }
}
