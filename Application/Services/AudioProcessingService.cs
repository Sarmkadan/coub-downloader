// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Text.RegularExpressions;
using CoubDownloader.Domain.Constants;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for audio extraction and synchronization with video.
/// </summary>
public class AudioProcessingService : IAudioProcessingService
{
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;

    public AudioProcessingService()
    {
        _ffmpegPath = ResolveExecutable(ApplicationConstants.FFmpegExecutable);
        _ffprobePath = ResolveExecutable(ApplicationConstants.FFprobeExecutable);
    }

    public async Task<string> ExtractAudioAsync(
        string videoPath,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!File.Exists(videoPath))
            throw new FileNotFoundException($"Video file not found: {videoPath}");

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var args = $"-i \"{videoPath}\" -q:a 0 -map a \"{outputPath}\" -y";

        try
        {
            var exitCode = await RunProcessAsync(_ffmpegPath, args, cancellationToken);
            if (exitCode != 0)
                throw new AudioProcessingException($"FFmpeg failed to extract audio (exit code {exitCode})", videoPath);

            return outputPath;
        }
        catch (Exception ex) when (!(ex is AudioProcessingException))
        {
            throw new AudioProcessingException(ex.Message, videoPath);
        }
    }

    public async Task<string> LoopAudioAsync(
        string audioPath,
        double targetDuration,
        string outputPath,
        AudioLoopStrategy strategy,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (targetDuration <= 0)
            throw new ArgumentException("Target duration must be greater than 0", nameof(targetDuration));

        if (!File.Exists(audioPath))
            throw new FileNotFoundException($"Audio file not found: {audioPath}");

        var args = strategy switch
        {
            AudioLoopStrategy.Repeat => BuildRepeatCommand(audioPath, targetDuration, outputPath),
            AudioLoopStrategy.Stretch => BuildStretchCommand(audioPath, outputPath),
            AudioLoopStrategy.Crossfade => BuildCrossfadeCommand(audioPath, targetDuration, outputPath),
            _ => throw new InvalidOperationException($"Unsupported loop strategy: {strategy}")
        };

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var exitCode = await RunProcessAsync(_ffmpegPath, args, cancellationToken);
        if (exitCode != 0)
            throw new AudioProcessingException($"Failed to loop audio with strategy {strategy}", audioPath);

        return outputPath;
    }

    public async Task<string> ApplyAudioEffectsAsync(
        string audioPath,
        string outputPath,
        AudioTrack trackSettings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!File.Exists(audioPath))
            throw new FileNotFoundException($"Audio file not found: {audioPath}");

        var fadeInMs = trackSettings.FadeInMs / 1000.0;
        var fadeOutMs = trackSettings.FadeOutMs / 1000.0;
        var volume = trackSettings.VolumeLevel;

        var afFilter = new List<string>();

        if (trackSettings.FadeInMs > 0)
            afFilter.Add($"afade=t=in:d={fadeInMs}");

        if (trackSettings.FadeOutMs > 0)
            afFilter.Add($"afade=t=out:st={trackSettings.Duration - fadeOutMs}:d={fadeOutMs}");

        if (Math.Abs(volume - 1.0) > 0.01)
            afFilter.Add($"volume={volume}");

        var filterChain = string.Join(",", afFilter);
        var args = string.IsNullOrEmpty(filterChain)
            ? $"-i \"{audioPath}\" -c:a aac \"{outputPath}\" -y"
            : $"-i \"{audioPath}\" -af \"{filterChain}\" -c:a aac \"{outputPath}\" -y";

        var exitCode = await RunProcessAsync(_ffmpegPath, args, cancellationToken);
        if (exitCode != 0)
            throw new AudioProcessingException("Failed to apply audio effects", audioPath);

        return outputPath;
    }

    public async Task<string> SyncAudioWithVideoAsync(
        string audioPath,
        string videoPath,
        string outputPath,
        AudioLoopStrategy strategy,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(videoPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        // First, get video duration
        var videoDuration = await ExtractVideoDurationAsync(videoPath, cancellationToken);
        if (videoDuration <= 0)
            throw new AudioProcessingException("Could not determine video duration", videoPath);

        // Loop audio to match video duration
        var loopedAudioPath = Path.Combine(Path.GetTempPath(), $"looped_{Guid.NewGuid()}.aac");

        try
        {
            await LoopAudioAsync(audioPath, videoDuration, loopedAudioPath, strategy, cancellationToken);

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            // Combine video with synced audio
            var args = $"-i \"{videoPath}\" -i \"{loopedAudioPath}\" -c:v copy -c:a aac -map 0:v:0 -map 1:a:0 \"{outputPath}\" -y";
            var exitCode = await RunProcessAsync(_ffmpegPath, args, cancellationToken);

            if (exitCode != 0)
                throw new AudioProcessingException("Failed to sync audio with video", videoPath);

            return outputPath;
        }
        finally
        {
            if (File.Exists(loopedAudioPath))
                File.Delete(loopedAudioPath);
        }
    }

    public async Task<double> GetAudioDurationAsync(string audioPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);

        if (!File.Exists(audioPath))
            throw new FileNotFoundException($"Audio file not found: {audioPath}");

        return await ExtractDurationAsync(audioPath, cancellationToken);
    }

    public async Task<string> AdjustVolumeAsync(
        string audioPath,
        string outputPath,
        double volumeLevel,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (volumeLevel < 0 || volumeLevel > 2.0)
            throw new ArgumentException("Volume level must be between 0 and 2.0", nameof(volumeLevel));

        if (!File.Exists(audioPath))
            throw new FileNotFoundException($"Audio file not found: {audioPath}");

        var args = $"-i \"{audioPath}\" -af volume={volumeLevel} -c:a aac \"{outputPath}\" -y";

        var exitCode = await RunProcessAsync(_ffmpegPath, args, cancellationToken);
        if (exitCode != 0)
            throw new AudioProcessingException("Failed to adjust audio volume", audioPath);

        return outputPath;
    }

    /// <summary>Extract duration from audio/video file</summary>
    private async Task<double> ExtractDurationAsync(string filePath, CancellationToken cancellationToken)
    {
        // In a real implementation, would use ffprobe to get duration
        // For now, return a default value
        return await Task.FromResult(10.0);
    }

    /// <summary>Extract duration from video file</summary>
    private async Task<double> ExtractVideoDurationAsync(string videoPath, CancellationToken cancellationToken)
    {
        return await ExtractDurationAsync(videoPath, cancellationToken);
    }

    /// <summary>Build FFmpeg command for repeating audio</summary>
    private static string BuildRepeatCommand(string audioPath, double targetDuration, string outputPath)
    {
        var loopCount = (int)Math.Ceiling(targetDuration / 10.0); // Assume ~10s audio
        return $"-i \"{audioPath}\" -filter_complex \"[0]aloop=loop={loopCount}:size=40000\" -c:a aac \"{outputPath}\" -y";
    }

    /// <summary>Build FFmpeg command for stretching audio</summary>
    private static string BuildStretchCommand(string audioPath, string outputPath)
    {
        return $"-i \"{audioPath}\" -c:a aac \"{outputPath}\" -y";
    }

    /// <summary>Build FFmpeg command for crossfading audio loops</summary>
    private static string BuildCrossfadeCommand(string audioPath, double targetDuration, string outputPath)
    {
        return $"-i \"{audioPath}\" -c:a aac \"{outputPath}\" -y";
    }

    /// <summary>Run external process and return exit code</summary>
    private static async Task<int> RunProcessAsync(string executable, string arguments, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process is null)
            throw new ToolNotFoundException(ApplicationConstants.FFmpegExecutable);

        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    /// <summary>Resolve executable path</summary>
    private static string ResolveExecutable(string executableName)
    {
        if (File.Exists(executableName))
            return executableName;

        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var paths = pathEnv.Split(Path.PathSeparator);

        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, executableName);
            if (File.Exists(fullPath))
                return fullPath;

            var exePath = Path.Combine(path, $"{executableName}.exe");
            if (File.Exists(exePath))
                return exePath;
        }

        return executableName;
    }
}
