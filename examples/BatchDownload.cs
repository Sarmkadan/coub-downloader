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
/// Batch download example: Process multiple videos with progress tracking
/// </summary>
public class BatchDownloadExample
{
    public static async Task Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection();
            services.AddCoubDownloaderServices();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            var batchService = serviceProvider.GetRequiredService<IBatchProcessingService>();
            var taskRepository = serviceProvider.GetRequiredService<IDownloadTaskRepository>();

            // Create batch job
            var outputDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "batch_coubs"
            );
            Directory.CreateDirectory(outputDir);

            var settings = new ConversionSettings
            {
                Id = Guid.NewGuid().ToString(),
                Format = VideoFormat.MP4,
                Quality = VideoQuality.High
            };

            Console.WriteLine("Creating batch job...");
            var batch = await batchService.CreateBatchJobAsync(
                "Coub Batch Download",
                outputDir,
                settings
            );

            // Add multiple tasks
            var urls = new[]
            {
                "https://coub.com/view/2a3b4c5d",
                "https://coub.com/view/3b4c5d6e",
                "https://coub.com/view/4c5d6e7f",
                "https://coub.com/view/5d6e7f8g"
            };

            var tasks = new List<DownloadTask>();
            for (int i = 0; i < urls.Length; i++)
            {
                var task = new DownloadTask
                {
                    Id = Guid.NewGuid().ToString(),
                    Url = urls[i],
                    OutputPath = Path.Combine(outputDir, $"video_{i + 1}.mp4"),
                    State = ProcessingState.Pending,
                    Format = VideoFormat.MP4,
                    Quality = VideoQuality.High
                };
                tasks.Add(task);
            }

            Console.WriteLine($"Adding {tasks.Count} tasks to batch...");
            await batchService.AddTasksAsync(batch.Id, tasks);

            // Monitor batch progress
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (batch.State != BatchJobState.Completed && stopwatch.Elapsed < TimeSpan.FromMinutes(10))
            {
                var status = await batchService.GetBatchStatusAsync(batch.Id);
                var progress = status.GetProgressPercent();

                Console.WriteLine($"\n Progress: {progress}%");
                Console.WriteLine($"  Completed: {status.CompletedTasks}/{status.TotalTasks}");
                Console.WriteLine($"  Failed: {status.FailedTasks}");
                Console.WriteLine($"  State: {status.State}");

                if (progress >= 100) break;
                await Task.Delay(2000);
            }

            Console.WriteLine("\n✓ Batch processing completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
