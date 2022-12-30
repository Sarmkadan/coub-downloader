// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Application.Services;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CoubDownloader.Examples;

/// <summary>
/// Custom conversion example: Download with specific quality, codec, and GPU acceleration
/// </summary>
public class CustomConversionExample
{
    public static async Task Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection();
            services.AddCoubDownloaderServices();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            var downloadService = serviceProvider.GetRequiredService<ICoubDownloadService>();
            var conversionService = serviceProvider.GetRequiredService<IVideoConversionService>();

            // Check FFmpeg availability
            Console.WriteLine("Checking FFmpeg installation...");
            var ffmpegAvailable = await conversionService.IsFfmpegAvailableAsync();
            if (!ffmpegAvailable)
            {
                Console.WriteLine("✗ FFmpeg not found. Please install FFmpeg first.");
                return;
            }

            var ffmpegVersion = await conversionService.GetFfmpegVersionAsync();
            Console.WriteLine($"✓ FFmpeg version: {ffmpegVersion}");

            // Create custom conversion settings
            var settings = new ConversionSettings
            {
                Id = Guid.NewGuid().ToString(),
                Format = VideoFormat.MP4,
                Quality = VideoQuality.UltraHigh,
                Width = 1920,
                Height = 1080,
                FrameRate = 60,
                VideoBitrate = 8000,
                AudioBitrate = 192,
                VideoCodec = "libx265",  // H.265/HEVC codec
                AudioCodec = "aac",
                EnableHardwareAcceleration = true,
                PreserveAspectRatio = true
            };

            Console.WriteLine("\nConversion Settings:");
            Console.WriteLine($"  Format: {settings.Format}");
            Console.WriteLine($"  Quality: {settings.Quality}");
            Console.WriteLine($"  Resolution: {settings.Width}x{settings.Height}");
            Console.WriteLine($"  Frame Rate: {settings.FrameRate} fps");
            Console.WriteLine($"  Video Bitrate: {settings.VideoBitrate} kbps");
            Console.WriteLine($"  Audio Bitrate: {settings.AudioBitrate} kbps");
            Console.WriteLine($"  Hardware Acceleration: {settings.EnableHardwareAcceleration}");

            // Download with custom settings
            Console.WriteLine("\nDownloading with custom settings...");
            var coubUrl = "https://coub.com/view/2a3b4c5d";
            var outputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "custom_video.mp4"
            );

            var result = await downloadService.DownloadAsync(coubUrl, settings);

            Console.WriteLine($"\n✓ Download completed!");
            Console.WriteLine($"  Output: {result.OutputPath}");
            Console.WriteLine($"  Size: {result.FileSizeBytes / 1024 / 1024} MB");
            Console.WriteLine($"  Codec: {result.Codec}");
            Console.WriteLine($"  Resolution: {result.Width}x{result.Height}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
