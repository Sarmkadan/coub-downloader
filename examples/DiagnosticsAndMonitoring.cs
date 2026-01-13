// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Application.Services;
using CoubDownloader.Infrastructure.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace CoubDownloader.Examples;

/// <summary>
/// Diagnostics and monitoring example: Check system health and performance
/// </summary>
public class DiagnosticsAndMonitoringExample
{
    public static async Task Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection();
            services.AddCoubDownloaderServices();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            var diagnosticsService = serviceProvider.GetRequiredService<IDiagnosticsService>();
            var conversionService = serviceProvider.GetRequiredService<IVideoConversionService>();
            var performanceMonitor = serviceProvider.GetRequiredService<IPerformanceMonitor>();

            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          SYSTEM DIAGNOSTICS AND MONITORING            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

            // Check FFmpeg
            Console.WriteLine("🔧 Checking FFmpeg Installation...");
            var ffmpegAvailable = await conversionService.IsFfmpegAvailableAsync();
            if (ffmpegAvailable)
            {
                var version = await conversionService.GetFfmpegVersionAsync();
                Console.WriteLine($"✓ FFmpeg is available");
                Console.WriteLine($"  Version: {version}");
            }
            else
            {
                Console.WriteLine($"✗ FFmpeg is not installed or not in PATH");
                Console.WriteLine($"  Install FFmpeg from: https://ffmpeg.org/download.html");
            }

            // System information
            Console.WriteLine("\n💻 System Information...");
            Console.WriteLine($"  OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
            Console.WriteLine($"  Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"  Processors: {Environment.ProcessorCount}");
            Console.WriteLine($"  Available Memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB");

            // Disk space check
            Console.WriteLine("\n💾 Storage Check...");
            var downloadDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );

            try
            {
                var driveInfo = new System.IO.DriveInfo(Path.GetPathRoot(downloadDir));
                Console.WriteLine($"  Download Directory: {downloadDir}");
                Console.WriteLine($"  Total Space: {driveInfo.TotalSize / 1024 / 1024 / 1024} GB");
                Console.WriteLine($"  Available Space: {driveInfo.AvailableFreeSpace / 1024 / 1024 / 1024} GB");
                Console.WriteLine($"  Used Space: {(driveInfo.TotalSize - driveInfo.AvailableFreeSpace) / 1024 / 1024 / 1024} GB");
            }
            catch
            {
                Console.WriteLine($"  Could not determine storage information");
            }

            // Network connectivity
            Console.WriteLine("\n🌐 Network Connectivity...");
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    var response = await client.GetAsync("https://coub.com", System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
                    Console.WriteLine($"✓ Coub.com is reachable");
                    Console.WriteLine($"  Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Cannot reach Coub.com: {ex.Message}");
            }

            // Performance metrics
            Console.WriteLine("\n📊 Performance Metrics...");
            Console.WriteLine($"  Current Memory Usage: {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} MB");
            Console.WriteLine($"  Peak Memory Usage: {Process.GetCurrentProcess().PeakWorkingSet64 / 1024 / 1024} MB");
            Console.WriteLine($"  CPU Time: {Process.GetCurrentProcess().UserProcessorTime.TotalSeconds:F2}s");
            Console.WriteLine($"  Total Processor Time: {Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds:F2}s");

            // Garbage collection
            Console.WriteLine("\n🗑️  Garbage Collection Stats...");
            Console.WriteLine($"  Gen 0 Collections: {GC.GetCollectionCount(0)}");
            Console.WriteLine($"  Gen 1 Collections: {GC.GetCollectionCount(1)}");
            Console.WriteLine($"  Gen 2 Collections: {GC.GetCollectionCount(2)}");
            Console.WriteLine($"  Total Memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB");

            // Performance benchmark
            Console.WriteLine("\n⚡ Performance Benchmark...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Simulate some work
            var list = new List<int>();
            for (int i = 0; i < 1000000; i++)
            {
                list.Add(i);
            }

            stopwatch.Stop();
            Console.WriteLine($"  List allocation (1M items): {stopwatch.ElapsedMilliseconds}ms");

            // Hardware acceleration check
            Console.WriteLine("\n🎮 Hardware Acceleration...");
            Console.WriteLine($"  NVIDIA NVENC: {CheckNvidiaGpu()}");
            Console.WriteLine($"  AMD VCE: {CheckAmdGpu()}");
            Console.WriteLine($"  Intel Quick Sync: {CheckIntelGpu()}");

            Console.WriteLine("\n✓ Diagnostics completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static string CheckNvidiaGpu()
    {
        try
        {
            // This would require NVIDIA's Video Codec SDK
            return "Not detected (install NVIDIA drivers for GPU acceleration)";
        }
        catch
        {
            return "Not available";
        }
    }

    private static string CheckAmdGpu()
    {
        try
        {
            return "Not detected (install AMD drivers for GPU acceleration)";
        }
        catch
        {
            return "Not available";
        }
    }

    private static string CheckIntelGpu()
    {
        try
        {
            return "Not detected (install Intel Media Driver for GPU acceleration)";
        }
        catch
        {
            return "Not available";
        }
    }
}
