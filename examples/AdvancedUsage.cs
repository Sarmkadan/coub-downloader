using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Examples;

/// <summary>
/// Demonstrates advanced configuration, custom conversion settings,
/// and error handling.
/// </summary>
public class AdvancedUsage
{
    public static async Task RunAsync()
    {
        var services = new ServiceCollection();
        
        // 1. Advanced configuration via DI
        services.AddCoubDownloaderServices();
        services.Configure<ConversionSettings>(settings =>
        {
            settings.FfmpegPath = "/usr/bin/ffmpeg"; // Explicit path
            settings.EnableHardwareAcceleration = true;
        });
        
        var serviceProvider = services.BuildServiceProvider();
        var downloadService = serviceProvider.GetRequiredService<ICoubDownloadService>();

        // 2. Custom conversion settings
        var customSettings = new ConversionSettings
        {
            Format = VideoFormat.MP4,
            Quality = VideoQuality.HighDefinition, // Example enum value
            Width = 1920,
            Height = 1080,
            VideoCodec = "libx264"
        };

        try
        {
            Console.WriteLine("Starting advanced download...");
            var result = await downloadService.DownloadAsync(
                "https://coub.com/view/2a3b4c5d",
                customSettings
            );

            if (!result.Success)
            {
                // Inspect specific error details
                Console.WriteLine($"Error: {result.ErrorMessage}");
                if (result.Exception != null)
                {
                    Console.WriteLine($"Exception: {result.Exception.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            // Handle unexpected infrastructure failures
            Console.WriteLine($"Critical failure: {ex.Message}");
        }
    }
}
