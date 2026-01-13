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
/// Shorts conversion example: Convert videos to vertical format for TikTok/Instagram Reels
/// </summary>
public class ShortsConversionExample
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

            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     COUB TO SHORTS CONVERSION EXAMPLE                  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

            // Create Shorts conversion settings (9:16 aspect ratio)
            var shortsSettings = new ConversionSettings
            {
                Id = Guid.NewGuid().ToString(),
                Format = VideoFormat.MP4,
                Quality = VideoQuality.High,
                Width = 1080,      // Common Shorts width
                Height = 1920,     // 9:16 aspect ratio
                FrameRate = 30,
                VideoBitrate = 5000,
                AudioBitrate = 128,
                PreserveAspectRatio = false,  // Allow aspect ratio change
                EnableHardwareAcceleration = true
            };

            Console.WriteLine("Shorts Settings:");
            Console.WriteLine($"  Format: {shortsSettings.Format}");
            Console.WriteLine($"  Quality: {shortsSettings.Quality}");
            Console.WriteLine($"  Resolution: {shortsSettings.Width}x{shortsSettings.Height}");
            Console.WriteLine($"  Aspect Ratio: 9:16 (Vertical)");
            Console.WriteLine($"  Frame Rate: {shortsSettings.FrameRate} fps");
            Console.WriteLine($"  Preserve Aspect: {shortsSettings.PreserveAspectRatio}");

            // Alternative settings for different platforms
            Console.WriteLine("\n📱 Platform-Specific Settings:\n");

            // TikTok
            var tiktokSettings = new ConversionSettings
            {
                Format = VideoFormat.MP4,
                Quality = VideoQuality.High,
                Width = 1080,
                Height = 1920,
                FrameRate = 30,
                VideoBitrate = 6000
            };
            Console.WriteLine("TikTok:");
            Console.WriteLine($"  Resolution: {tiktokSettings.Width}x{tiktokSettings.Height}");
            Console.WriteLine($"  Bitrate: {tiktokSettings.VideoBitrate} kbps");
            Console.WriteLine($"  Max Duration: 10 minutes (recommended: 15-60s)");

            // Instagram Reels
            var instagramSettings = new ConversionSettings
            {
                Format = VideoFormat.MP4,
                Quality = VideoQuality.High,
                Width = 1080,
                Height = 1920,
                FrameRate = 30,
                VideoBitrate = 5500
            };
            Console.WriteLine("\nInstagram Reels:");
            Console.WriteLine($"  Resolution: {instagramSettings.Width}x{instagramSettings.Height}");
            Console.WriteLine($"  Bitrate: {instagramSettings.VideoBitrate} kbps");
            Console.WriteLine($"  Max Duration: 90 seconds");

            // YouTube Shorts
            var youtubeSettings = new ConversionSettings
            {
                Format = VideoFormat.MP4,
                Quality = VideoQuality.High,
                Width = 1080,
                Height = 1920,
                FrameRate = 60,
                VideoBitrate = 8000
            };
            Console.WriteLine("\nYouTube Shorts:");
            Console.WriteLine($"  Resolution: {youtubeSettings.Width}x{youtubeSettings.Height}");
            Console.WriteLine($"  Bitrate: {youtubeSettings.VideoBitrate} kbps");
            Console.WriteLine($"  Max Duration: 60 seconds");
            Console.WriteLine($"  Frame Rate: {youtubeSettings.FrameRate} fps (60fps recommended)");

            // Download and convert
            Console.WriteLine("\nDownloading and converting to Shorts format...");
            var coubUrl = "https://coub.com/view/2a3b4c5d";
            var outputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "shorts_video.mp4"
            );

            try
            {
                var result = await downloadService.DownloadAsync(coubUrl, shortsSettings);

                Console.WriteLine($"\n✓ Conversion completed!");
                Console.WriteLine($"  Output: {result.OutputPath}");
                Console.WriteLine($"  Size: {result.FileSizeBytes / 1024 / 1024} MB");
                Console.WriteLine($"  Resolution: {result.Width}x{result.Height}");
                Console.WriteLine($"  Duration: {result.Duration:F2}s");
                Console.WriteLine($"  Codec: {result.Codec}");

                // Recommendations
                Console.WriteLine("\n💡 Post-Production Tips:");
                Console.WriteLine("  • Add captions for engagement");
                Console.WriteLine("  • Use trending audio clips");
                Console.WriteLine("  • Optimize for mobile viewing");
                Console.WriteLine("  • Add hooks in first 2 seconds");
                Console.WriteLine("  • Use full screen space (9:16 aspect ratio)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Conversion failed: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
