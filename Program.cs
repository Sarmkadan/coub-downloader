// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Constants;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure;
using CoubDownloader.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Configure dependency injection
services
    .AddCoubDownloaderServices()
    .AddHttpClient();

var serviceProvider = services.BuildServiceProvider();

// Get service instances
var videoRepository = serviceProvider.GetRequiredService<ICoubVideoRepository>();
var taskRepository = serviceProvider.GetRequiredService<IDownloadTaskRepository>();
var batchRepository = serviceProvider.GetRequiredService<IBatchJobRepository>();
var downloadService = serviceProvider.GetRequiredService<ICoubDownloadService>();
var conversionService = serviceProvider.GetRequiredService<IVideoConversionService>();
var audioService = serviceProvider.GetRequiredService<IAudioProcessingService>();
var batchService = serviceProvider.GetRequiredService<IBatchProcessingService>();

// Display application header
Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                   COUB DOWNLOADER v1.0.0                     ║");
Console.WriteLine("║        Download and Convert Coub Videos to MP4/Shorts       ║");
Console.WriteLine("║              Author: Vladyslav Zaiets                       ║");
Console.WriteLine("║              https://sarmkadan.com                          ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Demonstrate application functionality
await DemonstrateApplication(downloadService, conversionService, audioService, batchService,
    videoRepository, taskRepository, batchRepository);

Console.WriteLine("\n✓ Application demonstration completed successfully!");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

/// <summary>Demonstrate the application's core functionality</summary>
async Task DemonstrateApplication(
    ICoubDownloadService downloadService,
    IVideoConversionService conversionService,
    IAudioProcessingService audioService,
    IBatchProcessingService batchService,
    ICoubVideoRepository videoRepository,
    IDownloadTaskRepository taskRepository,
    IBatchJobRepository batchRepository)
{
    try
    {
        Console.WriteLine("\n📋 Demo: Create Sample Data");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        // Create sample conversion settings
        var settings = new ConversionSettings
        {
            Id = Guid.NewGuid().ToString(),
            Format = VideoFormat.MP4,
            Quality = VideoQuality.High,
            VideoBitrate = 5000,
            AudioBitrate = 128,
            FrameRate = 30,
            Width = 1920,
            Height = 1080,
            PreserveAspectRatio = true
        };
        settings.ApplyQualityPreset();
        Console.WriteLine($"✓ Created conversion settings: {settings.Quality} quality, {settings.Width}x{settings.Height}");

        // Create sample Coub video
        var video = new CoubVideo
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Amazing Cat Compilation",
            Url = "https://coub.com/view/2a3b4c5d",
            Duration = 15.5,
            Width = 1920,
            Height = 1080,
            CreatorName = "CatsAreAwesome",
            ViewCount = 50000,
            HasAudio = true,
            Description = "Hilarious cats doing funny things",
            UploadedDate = DateTime.UtcNow.AddDays(-7)
        };

        var savedVideo = await videoRepository.CreateAsync(video);
        Console.WriteLine($"✓ Saved video to repository: {savedVideo.Title} (ID: {savedVideo.Id})");

        // Create audio track
        var audioTrack = new AudioTrack
        {
            Id = Guid.NewGuid().ToString(),
            VideoId = savedVideo.Id,
            Duration = 15.5,
            SampleRate = 44100,
            Channels = 2,
            Bitrate = 128,
            Codec = "aac",
            LoopStrategy = AudioLoopStrategy.Repeat,
            LoopCount = 1,
            VolumeLevel = 1.0
        };

        savedVideo.AudioTrack = audioTrack;
        await videoRepository.UpdateAsync(savedVideo);
        Console.WriteLine($"✓ Added audio track: {audioTrack.GetAudioSpec()}");

        // Create video sections
        var section1 = new VideoSection
        {
            Id = Guid.NewGuid().ToString(),
            VideoId = savedVideo.Id,
            Index = 0,
            StartTime = 0,
            EndTime = 5.0,
            Description = "Intro",
            IsIncluded = true
        };

        var section2 = new VideoSection
        {
            Id = Guid.NewGuid().ToString(),
            VideoId = savedVideo.Id,
            Index = 1,
            StartTime = 5.0,
            EndTime = 15.5,
            Description = "Main action",
            IsIncluded = true
        };

        savedVideo.Sections.Add(section1);
        savedVideo.Sections.Add(section2);
        await videoRepository.UpdateAsync(savedVideo);
        Console.WriteLine($"✓ Added {savedVideo.Sections.Count} video sections");

        // Create download tasks
        Console.WriteLine("\n📥 Demo: Create Download Tasks");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var tasks = new List<DownloadTask>();

        for (int i = 0; i < 3; i++)
        {
            var task = new DownloadTask
            {
                Id = Guid.NewGuid().ToString(),
                VideoId = savedVideo.Id,
                Url = $"https://coub.com/view/{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                OutputPath = $"/downloads/coub_{i}.mp4",
                State = ProcessingState.Pending,
                Format = VideoFormat.MP4,
                Quality = VideoQuality.High,
                AudioLoop = AudioLoopStrategy.Repeat,
                MaxRetries = 3
            };

            var savedTask = await taskRepository.CreateAsync(task);
            tasks.Add(savedTask);
            Console.WriteLine($"✓ Created task {i + 1}: {savedTask.Url}");
        }

        // Create batch job
        Console.WriteLine("\n📦 Demo: Create Batch Job");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var batch = await batchService.CreateBatchJobAsync(
            "Cats Collection Batch",
            "/downloads/cats",
            settings);

        Console.WriteLine($"✓ Created batch job: {batch.Name} (ID: {batch.Id})");
        Console.WriteLine($"  Output directory: {batch.OutputDirectory}");

        // Add tasks to batch
        await batchService.AddTasksAsync(batch.Id, tasks);
        Console.WriteLine($"✓ Added {tasks.Count} tasks to batch");

        // Demonstrate batch status
        Console.WriteLine("\n🔍 Demo: Batch Status Reporting");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var batchStatus = await batchService.GetBatchStatusAsync(batch.Id);
        Console.WriteLine($"  Batch: {batchStatus.Name}");
        Console.WriteLine($"  Total tasks: {batchStatus.TotalTasks}");
        Console.WriteLine($"  Completed: {batchStatus.CompletedTasks}");
        Console.WriteLine($"  Failed: {batchStatus.FailedTasks}");
        Console.WriteLine($"  Progress: {batchStatus.GetProgressPercent()}%");
        Console.WriteLine($"  State: {batchStatus.State}");

        // Demonstrate conversion settings
        Console.WriteLine("\n⚙️  Demo: Conversion Settings");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var ffmpegParams = settings.GetFFmpegCodecParams();
        Console.WriteLine($"  FFmpeg codec parameters: {ffmpegParams}");
        Console.WriteLine($"  Video codec: {settings.VideoCodec}");
        Console.WriteLine($"  Audio codec: {settings.AudioCodec}");
        Console.WriteLine($"  Frame rate: {settings.FrameRate} fps");
        Console.WriteLine($"  Use hardware acceleration: {settings.EnableHardwareAcceleration}");

        // Demonstrate FFmpeg availability check
        Console.WriteLine("\n🔧 Demo: Tool Availability");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var ffmpegAvailable = await conversionService.IsFfmpegAvailableAsync();
        Console.WriteLine($"  FFmpeg available: {(ffmpegAvailable ? "✓ Yes" : "✗ No")}");

        if (ffmpegAvailable)
        {
            var version = await conversionService.GetFfmpegVersionAsync();
            Console.WriteLine($"  FFmpeg version: {version}");
        }
        else
        {
            Console.WriteLine("  ⚠ FFmpeg not found. Install FFmpeg to enable video conversion.");
        }

        // Demonstrate video properties
        Console.WriteLine("\n📺 Demo: Video Properties");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var aspectRatio = savedVideo.GetAspectRatio();
        var isVertical = savedVideo.IsVerticalFormat();
        var requiredAudio = savedVideo.CalculateRequiredAudioDuration();

        Console.WriteLine($"  Aspect ratio: {aspectRatio:F2}:1");
        Console.WriteLine($"  Is vertical format: {isVertical}");
        Console.WriteLine($"  Required audio duration: {requiredAudio}s");
        Console.WriteLine($"  Total sections: {savedVideo.Sections.Count}");

        // Demonstrate audio properties
        Console.WriteLine("\n🔊 Demo: Audio Properties");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var loopedDuration = audioTrack.CalculateLoopedDuration();
        var audioSpec = audioTrack.GetAudioSpec();

        Console.WriteLine($"  Audio spec: {audioSpec}");
        Console.WriteLine($"  Original duration: {audioTrack.Duration}s");
        Console.WriteLine($"  Looped duration: {loopedDuration}s");
        Console.WriteLine($"  Loop strategy: {audioTrack.LoopStrategy}");
        Console.WriteLine($"  Volume level: {audioTrack.VolumeLevel}x");

        // Query repositories
        Console.WriteLine("\n💾 Demo: Repository Queries");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var allVideos = await videoRepository.GetAllAsync();
        Console.WriteLine($"  Total videos in repository: {allVideos.Count()}");

        var allTasks = await taskRepository.GetAllAsync();
        Console.WriteLine($"  Total tasks in repository: {allTasks.Count()}");

        var videosByCreator = await videoRepository.GetByCreatorAsync("Cats");
        Console.WriteLine($"  Videos by creator 'Cats': {videosByCreator.Count()}");

        var tasksByState = await taskRepository.GetByStateAsync(ProcessingState.Pending);
        Console.WriteLine($"  Pending tasks: {tasksByState.Count()}");

        var allBatches = await batchRepository.GetAllAsync();
        Console.WriteLine($"  Total batch jobs: {allBatches.Count()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n✗ Error during demonstration: {ex.Message}");
        if (ex.InnerException is not null)
            Console.WriteLine($"  Details: {ex.InnerException.Message}");
    }
}
