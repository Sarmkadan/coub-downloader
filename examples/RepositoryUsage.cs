// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CoubDownloader.Examples;

/// <summary>
/// Repository usage example: Persist and query video data
/// </summary>
public class RepositoryUsageExample
{
    public static async Task Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection();
            services.AddCoubDownloaderServices();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            // Get repository instances
            var videoRepository = serviceProvider.GetRequiredService<ICoubVideoRepository>();
            var taskRepository = serviceProvider.GetRequiredService<IDownloadTaskRepository>();
            var resultRepository = serviceProvider.GetRequiredService<IDownloadResultRepository>();

            Console.WriteLine("=== Video Repository Operations ===\n");

            // Create and save a video
            var video = new CoubVideo
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Amazing Coub Video",
                Url = "https://coub.com/view/2a3b4c5d",
                Duration = 15.5,
                Width = 1920,
                Height = 1080,
                CreatorName = "VideoCreator",
                ViewCount = 10000,
                HasAudio = true,
                Description = "This is a test video",
                UploadedDate = DateTime.UtcNow.AddDays(-7)
            };

            Console.WriteLine("Creating video...");
            var savedVideo = await videoRepository.CreateAsync(video);
            Console.WriteLine($"✓ Video created: {savedVideo.Title}");
            Console.WriteLine($"  ID: {savedVideo.Id}");
            Console.WriteLine($"  Aspect Ratio: {savedVideo.GetAspectRatio():F2}:1");
            Console.WriteLine($"  Is Vertical: {savedVideo.IsVerticalFormat()}");

            // Add audio track
            var audioTrack = new AudioTrack
            {
                Id = Guid.NewGuid().ToString(),
                VideoId = savedVideo.Id,
                Duration = 15.5,
                SampleRate = 44100,
                Channels = 2,
                Bitrate = 128,
                Codec = "aac",
                LoopStrategy = AudioLoopStrategy.Repeat
            };

            savedVideo.AudioTrack = audioTrack;
            await videoRepository.UpdateAsync(savedVideo);
            Console.WriteLine($"✓ Audio track added: {audioTrack.GetAudioSpec()}");

            // Query videos
            Console.WriteLine("\nQuerying videos...");
            var allVideos = await videoRepository.GetAllAsync();
            Console.WriteLine($"Total videos: {allVideos.Count()}");

            var videosByCreator = await videoRepository.GetByCreatorAsync("VideoCreator");
            Console.WriteLine($"Videos by 'VideoCreator': {videosByCreator.Count()}");

            Console.WriteLine("\n=== Download Task Repository Operations ===\n");

            // Create download tasks
            var tasks = new List<DownloadTask>();
            for (int i = 0; i < 3; i++)
            {
                var task = new DownloadTask
                {
                    Id = Guid.NewGuid().ToString(),
                    VideoId = savedVideo.Id,
                    Url = $"https://coub.com/view/{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                    OutputPath = $"/downloads/video_{i}.mp4",
                    State = ProcessingState.Pending,
                    Format = VideoFormat.MP4,
                    Quality = VideoQuality.High,
                    AudioLoop = AudioLoopStrategy.Repeat,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var savedTask = await taskRepository.CreateAsync(task);
                tasks.Add(savedTask);
                Console.WriteLine($"✓ Task created: {savedTask.Id}");
            }

            // Query tasks by state
            var pendingTasks = await taskRepository.GetByStateAsync(ProcessingState.Pending);
            Console.WriteLine($"\nPending tasks: {pendingTasks.Count()}");

            // Update task state
            if (pendingTasks.Any())
            {
                var firstTask = pendingTasks.First();
                firstTask.State = ProcessingState.InProgress;
                await taskRepository.UpdateAsync(firstTask);
                Console.WriteLine($"✓ Updated task state to: {firstTask.State}");
            }

            Console.WriteLine("\n=== Download Result Repository Operations ===\n");

            // Create download result
            var result = new DownloadResult
            {
                Id = Guid.NewGuid().ToString(),
                VideoId = savedVideo.Id,
                TaskId = tasks.First().Id,
                OutputPath = "/downloads/video_final.mp4",
                FileSizeBytes = 52428800,  // 50 MB
                Duration = 15.5,
                Width = 1920,
                Height = 1080,
                Codec = "h264",
                State = DownloadState.Completed,
                CompletedAt = DateTime.UtcNow
            };

            Console.WriteLine("Creating download result...");
            var savedResult = await resultRepository.CreateAsync(result);
            Console.WriteLine($"✓ Result saved");
            Console.WriteLine($"  Output: {savedResult.OutputPath}");
            Console.WriteLine($"  Size: {savedResult.FileSizeBytes / 1024 / 1024} MB");
            Console.WriteLine($"  State: {savedResult.State}");

            // Query results
            var allResults = await resultRepository.GetAllAsync();
            Console.WriteLine($"\nTotal results: {allResults.Count()}");

            var completedResults = allResults.Where(r => r.State == DownloadState.Completed);
            Console.WriteLine($"Completed downloads: {completedResults.Count()}");

            Console.WriteLine("\n✓ Repository operations completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
