// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CoubDownloader.Examples;

/// <summary>
/// Event handling example: Subscribe to download events and react to them
/// </summary>
public class EventHandlingExample
{
    public static async Task Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection();
            services.AddCoubDownloaderServices();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            var eventBus = serviceProvider.GetRequiredService<IEventBus>();
            var downloadService = serviceProvider.GetRequiredService<ICoubDownloadService>();

            // Subscribe to download started event
            eventBus.Subscribe<DownloadStartedEvent>(async @event =>
            {
                Console.WriteLine($"📥 Download Started");
                Console.WriteLine($"  URL: {@event.VideoUrl}");
                Console.WriteLine($"  Title: {@event.VideoTitle}");
                Console.WriteLine($"  Time: {DateTime.Now:HH:mm:ss}");
            });

            // Subscribe to download progress event
            eventBus.Subscribe<DownloadProgressEvent>(async @event =>
            {
                Console.WriteLine($"⏳ Progress: {@event.ProgressPercent}%");
                if (@event.DownloadedBytes > 0)
                {
                    Console.WriteLine($"  Downloaded: {@event.DownloadedBytes / 1024 / 1024} MB");
                }
            });

            // Subscribe to download completed event
            eventBus.Subscribe<DownloadCompletedEvent>(async @event =>
            {
                Console.WriteLine($"✓ Download Completed");
                Console.WriteLine($"  Output: {@event.OutputPath}");
                Console.WriteLine($"  Size: {@event.FileSizeBytes / 1024 / 1024} MB");
                Console.WriteLine($"  Duration: {@event.Duration:F2}s");
                Console.WriteLine($"  Time: {DateTime.Now:HH:mm:ss}");
            });

            // Subscribe to download failed event
            eventBus.Subscribe<DownloadFailedEvent>(async @event =>
            {
                Console.WriteLine($"✗ Download Failed");
                Console.WriteLine($"  Error: {@event.Error}");
                Console.WriteLine($"  Retry Attempt: {@event.RetryAttempt}");
                Console.WriteLine($"  Time: {DateTime.Now:HH:mm:ss}");
            });

            // Subscribe to conversion started event
            eventBus.Subscribe<ConversionStartedEvent>(async @event =>
            {
                Console.WriteLine($"🎬 Conversion Started");
                Console.WriteLine($"  Input: {@event.InputPath}");
                Console.WriteLine($"  Output: {@event.OutputPath}");
                Console.WriteLine($"  Quality: {@event.Quality}");
            });

            // Subscribe to conversion completed event
            eventBus.Subscribe<ConversionCompletedEvent>(async @event =>
            {
                Console.WriteLine($"✓ Conversion Completed");
                Console.WriteLine($"  Output: {@event.OutputPath}");
                Console.WriteLine($"  Duration: {@event.Duration}ms");
            });

            // Subscribe to conversion failed event
            eventBus.Subscribe<ConversionFailedEvent>(async @event =>
            {
                Console.WriteLine($"✗ Conversion Failed");
                Console.WriteLine($"  Error: {@event.Error}");
            });

            Console.WriteLine("Event handlers registered. Starting download...\n");

            // Trigger events by downloading a video
            var coubUrl = "https://coub.com/view/2a3b4c5d";
            var outputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "event_demo.mp4"
            );

            try
            {
                var result = await downloadService.DownloadAsync(coubUrl);
                Console.WriteLine("\n✓ Download with events completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Download failed: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}

// Event classes (these would normally be in the domain)
public class DownloadStartedEvent
{
    public string VideoUrl { get; set; } = null!;
    public string VideoTitle { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class DownloadProgressEvent
{
    public int ProgressPercent { get; set; }
    public long DownloadedBytes { get; set; }
    public long TotalBytes { get; set; }
}

public class DownloadCompletedEvent
{
    public string OutputPath { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public double Duration { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class DownloadFailedEvent
{
    public string Error { get; set; } = null!;
    public int RetryAttempt { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ConversionStartedEvent
{
    public string InputPath { get; set; } = null!;
    public string OutputPath { get; set; } = null!;
    public VideoQuality Quality { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ConversionCompletedEvent
{
    public string OutputPath { get; set; } = null!;
    public long DurationMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ConversionFailedEvent
{
    public string Error { get; set; } = null!;
    public string InputPath { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
