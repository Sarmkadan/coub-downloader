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
/// Audio processing example: Handle audio looping and synchronization
/// </summary>
public class AudioProcessingExample
{
    public static async Task Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection();
            services.AddCoubDownloaderServices();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();

            var audioService = serviceProvider.GetRequiredService<IAudioProcessingService>();

            // Create audio track with short duration
            var audioTrack = new AudioTrack
            {
                Id = Guid.NewGuid().ToString(),
                VideoId = Guid.NewGuid().ToString(),
                Duration = 6.5,  // 6.5 seconds
                SampleRate = 44100,
                Channels = 2,
                Bitrate = 128,
                Codec = "aac",
                LoopStrategy = AudioLoopStrategy.Repeat,
                LoopCount = 0,  // Repeat as needed
                VolumeLevel = 1.0
            };

            Console.WriteLine("Audio Track Information:");
            Console.WriteLine($"  Duration: {audioTrack.Duration}s");
            Console.WriteLine($"  Sample Rate: {audioTrack.SampleRate} Hz");
            Console.WriteLine($"  Channels: {audioTrack.Channels}");
            Console.WriteLine($"  Bitrate: {audioTrack.Bitrate} kbps");
            Console.WriteLine($"  Codec: {audioTrack.Codec}");
            Console.WriteLine($"  Loop Strategy: {audioTrack.LoopStrategy}");
            Console.WriteLine($"  Audio Spec: {audioTrack.GetAudioSpec()}");

            // Calculate looped duration to match video (15.5 seconds)
            var videoDuration = 15.5;
            var loopedDuration = audioTrack.CalculateLoopedDuration();

            Console.WriteLine($"\nAudio Looping Calculation:");
            Console.WriteLine($"  Original Audio Duration: {audioTrack.Duration}s");
            Console.WriteLine($"  Video Duration: {videoDuration}s");
            Console.WriteLine($"  Looped Audio Duration: {loopedDuration}s");

            // Calculate required loops
            var requiredLoops = Math.Ceiling(videoDuration / audioTrack.Duration);
            Console.WriteLine($"  Required Loops: {requiredLoops}");

            // Different loop strategies
            Console.WriteLine($"\nLoop Strategies:");

            // Strategy 1: Repeat (simple concatenation)
            var repeatStrategy = new AudioTrack
            {
                Duration = audioTrack.Duration,
                LoopStrategy = AudioLoopStrategy.Repeat,
                LoopCount = 2
            };
            Console.WriteLine($"  Repeat: {repeatStrategy.LoopStrategy} - Concatenate audio {repeatStrategy.LoopCount} times");

            // Strategy 2: Fade (fade between loops)
            var fadeStrategy = new AudioTrack
            {
                Duration = audioTrack.Duration,
                LoopStrategy = AudioLoopStrategy.Fade,
                LoopCount = 2
            };
            Console.WriteLine($"  Fade: {fadeStrategy.LoopStrategy} - Add crossfade between loops");

            // Strategy 3: Silent (pad with silence)
            var silentStrategy = new AudioTrack
            {
                Duration = audioTrack.Duration,
                LoopStrategy = AudioLoopStrategy.Silent,
                LoopCount = 0
            };
            Console.WriteLine($"  Silent: {silentStrategy.LoopStrategy} - Pad with silence to reach target duration");

            // Volume adjustment
            Console.WriteLine($"\nVolume Adjustment:");
            var volumeLevels = new[] { 0.5, 0.8, 1.0, 1.2, 1.5 };
            foreach (var level in volumeLevels)
            {
                audioTrack.VolumeLevel = level;
                Console.WriteLine($"  Volume: {level}x");
            }

            Console.WriteLine("\n✓ Audio processing example completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
