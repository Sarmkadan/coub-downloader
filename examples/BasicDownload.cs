// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CoubDownloader.Examples;

/// <summary>
/// Basic example: Download a single Coub video with default settings
/// </summary>
public class BasicDownloadExample
{
    public static async Task Main(string[] args)
    {
        try
        {
            // Setup dependency injection
            var services = new ServiceCollection();
            services.AddCoubDownloaderServices();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            // Get download service
            var downloadService = serviceProvider.GetRequiredService<ICoubDownloadService>();

            // Download single video
            Console.WriteLine("Downloading Coub video...");
            var coubUrl = "https://coub.com/view/2a3b4c5d";
            var outputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "coub_video.mp4"
            );

            var result = await downloadService.DownloadAsync(coubUrl);

            Console.WriteLine($"✓ Download completed!");
            Console.WriteLine($"  Output: {result.OutputPath}");
            Console.WriteLine($"  Size: {result.FileSizeBytes / 1024 / 1024} MB");
            Console.WriteLine($"  Duration: {result.Duration:F2}s");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
