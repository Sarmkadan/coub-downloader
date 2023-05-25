#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using CoubDownloader.Domain.Constants;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Exceptions;
using CoubDownloader.Domain.Models;
using CoubDownloader.Infrastructure.Integration;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for audio extraction and synchronization with video.
/// </summary>
public class AudioProcessingService : IAudioProcessingService
{
    private readonly IFFmpegWrapper _ffmpegWrapper;

    public AudioProcessingService(IFFmpegWrapper ffmpegWrapper)
    {
        _ffmpegWrapper = ffmpegWrapper;
    }

    public async Task<string> ExtractAudioAsync(
        string videoPath,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        try
        {
            if (!File.Exists(videoPath))
                throw new FileOperationException("Video file not found", videoPath, FileOperationType.Read);

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var result = await _ffmpegWrapper.ExtractAudioAsync(videoPath, outputPath);

            if (!result.Success)
                throw new ProcessExecutionException(
                    $"FFmpeg failed to extract audio (exit code {result.ExitCode})",
                    ApplicationConstants.FFmpegExecutable,
                    $"Extract audio from {videoPath}",
                    result.ExitCode,
                    result.Error);

            return outputPath;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException)
        {
            throw new AudioProcessingException($"Failed to extract audio from video", videoPath, ex);
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

        try
        {
            if (targetDuration <= 0)
                throw new ValidationException("Target duration must be greater than 0", nameof(targetDuration), targetDuration);

            if (!File.Exists(audioPath))
                throw new FileOperationException("Audio file not found", audioPath, FileOperationType.Read);

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            FFmpegResult result;
            string[] args = Array.Empty<string>();
            if (strategy == AudioLoopStrategy.Repeat)
            {
                // Hotfix: Use FFmpegWrapper's LoopAudioAsync for precise duration trimming
                // The original BuildRepeatCommand with aloop filter and estimated loopCount could cause 1-frame gaps.
                result = await _ffmpegWrapper.LoopAudioAsync(audioPath, targetDuration, outputPath);
            }
            else if (strategy == AudioLoopStrategy.Crossfade)
            {
                // Hotfix: Implement proper crossfade using FFmpeg filter complex.
                // Original implementation only copied the audio.
                var audioDuration = await GetAudioDurationAsync(audioPath, cancellationToken);
                if (audioDuration <= 0)
                {
                    throw new AudioProcessingException("Could not determine audio duration for crossfade.", audioPath);
                }

                var numLoops = (int)Math.Ceiling(targetDuration / audioDuration);
                var totalInputDuration = numLoops * audioDuration;

                var concatArgs = new List<string>();
                for (int i = 0; i < numLoops; i++)
                {
                    concatArgs.AddRange(new[] { "-i", audioPath });
                }

                var filterComplex = new List<string>();
                for (int i = 0; i < numLoops; i++)
                {
                    var delayMs = (long)Math.Round(i * audioDuration * 1000);
                    filterComplex.Add(FormattableString.Invariant($"[{i}:0]adelay={delayMs}|{delayMs}[a{i}]"));
                }
                filterComplex.Add(string.Join("", Enumerable.Range(0, numLoops).Select(i => $"[a{i}]")) + $"amix=inputs={numLoops}:duration=longest:dropout_transition=0,apad[aout]");

                args = concatArgs.Concat(new[]
                {
                    "-filter_complex", string.Join(";", filterComplex),
                    "-map", "[aout]",
                    "-c:a", "aac",
                    "-t", targetDuration.ToString("F3", CultureInfo.InvariantCulture),
                    "-y", outputPath
                }).ToArray();

                result = await _ffmpegWrapper.ExecuteAsync(args);
            }
            else // AudioLoopStrategy.Stretch: time-stretch the audio to the target duration
            {
                var audioDuration = await GetAudioDurationAsync(audioPath, cancellationToken);
                if (audioDuration <= 0)
                    throw new AudioProcessingException("Could not determine audio duration for stretch.", audioPath);

                var tempo = audioDuration / targetDuration;
                var atempoChain = BuildAtempoChain(tempo);

                args = new[]
                {
                    "-i", audioPath,
                    "-af", atempoChain,
                    "-c:a", "aac",
                    "-t", targetDuration.ToString("F3", CultureInfo.InvariantCulture),
                    "-y", outputPath
                };
                result = await _ffmpegWrapper.ExecuteAsync(args);
            }

            if (!result.Success)
                throw new ProcessExecutionException(
                    $"Failed to loop audio with strategy {strategy}",
                    ApplicationConstants.FFmpegExecutable,
                    string.Join(" ", args),
                    result.ExitCode,
                    result.Error);

            return outputPath;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException and not ValidationException)
        {
            throw new AudioProcessingException($"Failed to loop audio with strategy {strategy}", audioPath, ex);
        }
    }

    public async Task<string> ApplyAudioEffectsAsync(
        string audioPath,
        string outputPath,
        AudioTrack trackSettings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(trackSettings);

        try
        {
            if (!File.Exists(audioPath))
                throw new FileOperationException("Audio file not found", audioPath, FileOperationType.Read);

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var fadeInMs = trackSettings.FadeInMs / 1000.0;
            var fadeOutMs = trackSettings.FadeOutMs / 1000.0;
            var volume = trackSettings.VolumeLevel;

            var afFilter = new List<string>();

            if (trackSettings.FadeInMs > 0)
                afFilter.Add(FormattableString.Invariant($"afade=t=in:d={fadeInMs}"));

            if (trackSettings.FadeOutMs > 0)
                afFilter.Add(FormattableString.Invariant($"afade=t=out:st={trackSettings.Duration - fadeOutMs}:d={fadeOutMs}"));

            if (Math.Abs(volume - 1.0) > 0.01)
                afFilter.Add(FormattableString.Invariant($"volume={volume}"));

            var filterChain = string.Join(",", afFilter);
            var args = string.IsNullOrEmpty(filterChain)
                ? new[] { "-i", audioPath, "-c:a", "aac", "-y", outputPath }
                : new[] { "-i", audioPath, "-af", filterChain, "-c:a", "aac", "-y", outputPath };

            var result = await _ffmpegWrapper.ExecuteAsync(args);
            if (!result.Success)
                throw new ProcessExecutionException(
                    "Failed to apply audio effects",
                    ApplicationConstants.FFmpegExecutable,
                    string.Join(" ", args),
                    result.ExitCode,
                    result.Error);

            return outputPath;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException)
        {
            throw new AudioProcessingException("Failed to apply audio effects", audioPath, ex);
        }
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

        try
        {
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
                var args = new[] { "-i", videoPath, "-i", loopedAudioPath, "-c:v", "copy", "-c:a", "aac", "-map", "0:v:0", "-map", "1:a:0", "-y", outputPath };
                var result = await _ffmpegWrapper.ExecuteAsync(args);

                if (!result.Success)
                    throw new ProcessExecutionException(
                        "Failed to sync audio with video",
                        ApplicationConstants.FFmpegExecutable,
                        string.Join(" ", args),
                        result.ExitCode,
                        result.Error);

                return outputPath;
            }
            finally
            {
                if (File.Exists(loopedAudioPath))
                    File.Delete(loopedAudioPath);
            }
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException)
        {
            throw new AudioProcessingException("Failed to sync audio with video", videoPath, ex);
        }
    }

    public async Task<double> GetAudioDurationAsync(string audioPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);

        try
        {
            if (!File.Exists(audioPath))
                throw new FileOperationException("Audio file not found", audioPath, FileOperationType.Read);

            var mediaInfo = await _ffmpegWrapper.GetMediaInfoAsync(audioPath);
            if (mediaInfo?.DurationInSeconds == null)
                throw new AudioProcessingException($"Could not determine audio duration for file: {audioPath}", audioPath);

            return mediaInfo.DurationInSeconds.Value;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException)
        {
            throw new AudioProcessingException($"Could not determine audio duration for file: {audioPath}", audioPath, ex);
        }
    }

    public async Task<string> AdjustVolumeAsync(
        string audioPath,
        string outputPath,
        double volumeLevel,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        try
        {
            if (volumeLevel < 0 || volumeLevel > 2.0)
                throw new ValidationException("Volume level must be between 0 and 2.0", nameof(volumeLevel), volumeLevel);

            if (!File.Exists(audioPath))
                throw new FileOperationException("Audio file not found", audioPath, FileOperationType.Read);

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var args = new[] { "-i", audioPath, "-af", FormattableString.Invariant($"volume={volumeLevel}"), "-c:a", "aac", "-y", outputPath };

            var result = await _ffmpegWrapper.ExecuteAsync(args);
            if (!result.Success)
                throw new ProcessExecutionException(
                    "Failed to adjust audio volume",
                    ApplicationConstants.FFmpegExecutable,
                    string.Join(" ", args),
                    result.ExitCode,
                    result.Error);

            return outputPath;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException and not ProcessExecutionException and not ValidationException)
        {
            throw new AudioProcessingException("Failed to adjust audio volume", audioPath, ex);
        }
    }

    /// <summary>
    /// Build an atempo filter chain for the given tempo factor.
    /// A single atempo instance only accepts 0.5-2.0, so factors outside
    /// that range are decomposed into a chain of atempo filters.
    /// </summary>
    private static string BuildAtempoChain(double tempo)
    {
        if (tempo <= 0 || double.IsNaN(tempo) || double.IsInfinity(tempo))
            throw new ValidationException("Tempo factor must be a positive finite number", nameof(tempo), tempo);

        var factors = new List<double>();

        while (tempo > 2.0)
        {
            factors.Add(2.0);
            tempo /= 2.0;
        }

        while (tempo < 0.5)
        {
            factors.Add(0.5);
            tempo /= 0.5;
        }

        factors.Add(tempo);

        return string.Join(",", factors.Select(f => FormattableString.Invariant($"atempo={f:0.######}")));
    }

    /// <summary>Extract duration from audio/video file</summary>
    private async Task<double> ExtractDurationAsync(string filePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            // Hotfix: Use FFmpegWrapper to get actual media duration via ffprobe.
            var mediaInfo = await _ffmpegWrapper.GetMediaInfoAsync(filePath);
            if (mediaInfo?.DurationInSeconds == null)
                throw new AudioProcessingException($"Could not determine duration for file: {filePath}", filePath);

            return mediaInfo.DurationInSeconds.Value;
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException)
        {
            throw new AudioProcessingException($"Could not determine duration for file: {filePath}", filePath, ex);
        }
    }

    /// <summary>Extract duration from video file</summary>
    private async Task<double> ExtractVideoDurationAsync(string videoPath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoPath);

        try
        {
            // Hotfix: Use FFmpegWrapper to get actual video duration via ffprobe.
            return await ExtractDurationAsync(videoPath, cancellationToken);
        }
        catch (CoubDownloaderException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FileOperationException)
        {
            throw new AudioProcessingException($"Could not determine video duration for file: {videoPath}", videoPath, ex);
        }
    }
}
