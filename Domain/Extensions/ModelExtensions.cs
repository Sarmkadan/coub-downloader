#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Domain.Extensions;

/// <summary>Extension methods for domain models</summary>
public static class CoubVideoExtensions
{
    /// <summary>Gets the aspect ratio as a decimal value (width/height).</summary>
    /// <param name="video">The video instance.</param>
    /// <returns>The aspect ratio as width divided by height, or 16:9 (1.78) if height is zero.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is <see langword="null"/>.</exception>
    public static double GetAspectRatio(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);
        return video.Height > 0 ? (double)video.Width / video.Height : 16.0 / 9.0;
    }

    /// <summary>Determines whether the video is in vertical format (TikTok/Shorts style).</summary>
    /// <param name="video">The video instance.</param>
    /// <returns><see langword="true"/> if the video's aspect ratio is less than 1 (portrait orientation); otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is <see langword="null"/>.</exception>
    public static bool IsVerticalFormat(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);
        return video.GetAspectRatio() < 1;
    }

    /// <summary>Determines whether the video is in HD quality (at least 720p).</summary>
    /// <param name="video">The video instance.</param>
    /// <returns><see langword="true"/> if the video's resolution is 1280x720 or higher; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is <see langword="null"/>.</exception>
    public static bool IsHdQuality(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);
        return video.Width >= 1280 && video.Height >= 720;
    }

    /// <summary>Determines whether the video is in 4K quality (at least 2160p).</summary>
    /// <param name="video">The video instance.</param>
    /// <returns><see langword="true"/> if the video's resolution is 3840x2160 or higher; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is <see langword="null"/>.</exception>
    public static bool Is4kQuality(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);
        return video.Width >= 3840 && video.Height >= 2160;
    }

    /// <summary>Calculates the required audio duration to match the video duration through looping.</summary>
    /// <param name="video">The video instance.</param>
    /// <returns>
    /// The total duration required from the audio track to cover the video duration through looping,
    /// or 0 when the video has no audio track or the audio track has a non-positive duration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is <see langword="null"/>.</exception>
    public static double CalculateRequiredAudioDuration(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);

        if (video.AudioTrack is null || video.AudioTrack.Duration <= 0)
            return 0;

        var requiredLoops = Math.Ceiling(video.Duration / video.AudioTrack.Duration);
        return video.AudioTrack.Duration * requiredLoops;
    }

    /// <summary>Gets the video classification based on its duration.</summary>
    /// <param name="video">The video instance.</param>
    /// <returns>A string representing the duration category: "Short", "Medium", "Long", or "Extra Long".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is <see langword="null"/>.</exception>
    public static string GetDurationCategory(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);
        return video.Duration switch
        {
            < 6 => "Short",
            < 15 => "Medium",
            < 30 => "Long",
            _ => "Extra Long"
        };
    }

    /// <summary>Formats the view count into a human-readable string with appropriate units.</summary>
    /// <param name="video">The video instance.</param>
    /// <returns>A formatted string representing the view count (e.g., "1.2M", "45K", or the raw number).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="video.ViewCount"/> is negative.</exception>
    public static string GetFormattedViewCount(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);
        ArgumentOutOfRangeException.ThrowIfNegative(video.ViewCount);

        return video.ViewCount switch
        {
            >= 1_000_000 => $"{video.ViewCount / 1_000_000}M",
            >= 1_000 => $"{video.ViewCount / 1_000}K",
            _ => video.ViewCount.ToString()
        };
    }
}

/// <summary>Extension methods for AudioTrack</summary>
public static class AudioTrackExtensions
{
    /// <summary>Gets a formatted audio specification string containing sample rate, channels, codec, and bitrate.</summary>
    /// <param name="track">The audio track instance.</param>
    /// <returns>A formatted string in the format "44100Hz 2ch AAC 192kbps".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="track"/> is <see langword="null"/>.</exception>
    public static string GetAudioSpec(this AudioTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        return $"{track.SampleRate}Hz {track.Channels}ch {track.Codec} {track.Bitrate}kbps";
    }

    /// <summary>Calculates the total duration after applying loop count.</summary>
    /// <param name="track">The audio track instance.</param>
    /// <returns>The total duration in seconds after applying loop count (duration × (loopCount + 1)).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="track"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="track.Duration"/> is negative.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="track.LoopCount"/> is negative.</exception>
    public static double CalculateLoopedDuration(this AudioTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        ArgumentOutOfRangeException.ThrowIfNegative(track.Duration);
        ArgumentOutOfRangeException.ThrowIfNegative(track.LoopCount);

        return track.Duration * (track.LoopCount + 1);
    }

    /// <summary>Determines whether the audio track uses a lossless codec (FLAC or PCM).</summary>
    /// <param name="track">The audio track instance.</param>
    /// <returns><see langword="true"/> if the codec is FLAC or PCM (case-insensitive); otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="track"/> is <see langword="null"/>.</exception>
    public static bool IsLossless(this AudioTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        return track.Codec.Contains("flac", StringComparison.OrdinalIgnoreCase) ||
               track.Codec.Contains("pcm", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Determines whether the audio track is stereo (2 channels).</summary>
    /// <param name="track">The audio track instance.</param>
    /// <returns><see langword="true"/> if the track has 2 channels; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="track"/> is <see langword="null"/>.</exception>
    public static bool IsStereo(this AudioTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        return track.Channels == 2;
    }

    /// <summary>Determines whether the audio track is mono (1 channel).</summary>
    /// <param name="track">The audio track instance.</param>
    /// <returns><see langword="true"/> if the track has 1 channel; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="track"/> is <see langword="null"/>.</exception>
    public static bool IsMono(this AudioTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        return track.Channels == 1;
    }

    /// <summary>Determines whether the audio track supports surround sound (more than 2 channels).</summary>
    /// <param name="track">The audio track instance.</param>
    /// <returns><see langword="true"/> if the track has more than 2 channels; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="track"/> is <see langword="null"/>.</exception>
    public static bool IsSurround(this AudioTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        return track.Channels > 2;
    }
}

/// <summary>Extension methods for ConversionSettings</summary>
public static class ConversionSettingsExtensions
{
    /// <summary>Gets the FFmpeg codec parameters string for video conversion.</summary>
    /// <param name="settings">The conversion settings instance.</param>
    /// <returns>A formatted string containing FFmpeg codec parameters for video and audio.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="settings.VideoBitrate"/> or <paramref name="settings.AudioBitrate"/> is negative.</exception>
    public static string GetFFmpegCodecParams(this ConversionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentOutOfRangeException.ThrowIfNegative(settings.VideoBitrate);
        ArgumentOutOfRangeException.ThrowIfNegative(settings.AudioBitrate);

        return $"-c:v {settings.VideoCodec} -c:a {settings.AudioCodec} " +
               $"-b:v {settings.VideoBitrate}k -b:a {settings.AudioBitrate}k";
    }

    /// <summary>Calculates the total bitrate by summing video and audio bitrates.</summary>
    /// <param name="settings">The conversion settings instance.</param>
    /// <returns>The sum of video and audio bitrates in kbps.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="OverflowException">Thrown when the sum of bitrates exceeds <see cref="int.MaxValue"/>.</exception>
    public static int GetTotalBitrate(this ConversionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        checked
        {
            return settings.VideoBitrate + settings.AudioBitrate;
        }
    }

    /// <summary>Determines whether hardware acceleration should be used based on settings and availability.</summary>
    /// <param name="settings">The conversion settings instance.</param>
    /// <returns><see langword="true"/> if hardware acceleration is enabled in settings and available; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <see langword="null"/>.</exception>
    public static bool ShouldUseHardwareAcceleration(this ConversionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return settings.EnableHardwareAcceleration && IsHardwareAvailable();
    }

    /// <summary>Estimates the output file size in bytes based on bitrate and duration.</summary>
    /// <param name="settings">The conversion settings instance.</param>
    /// <param name="durationSeconds">The duration of the media in seconds.</param>
    /// <returns>The estimated output file size in bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="durationSeconds"/> is negative.</exception>
    /// <exception cref="OverflowException">Thrown when the calculation exceeds <see cref="long.MaxValue"/>.</exception>
    public static long EstimateOutputSize(this ConversionSettings settings, double durationSeconds)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentOutOfRangeException.ThrowIfNegative(durationSeconds);
        ArgumentOutOfRangeException.ThrowIfNegative(settings.GetTotalBitrate());

        checked
        {
            var bitrate = settings.GetTotalBitrate();
            return (long)(bitrate * 1000 / 8.0 * durationSeconds); // bits to bytes
        }
    }

    private static bool IsHardwareAvailable()
    {
        // Check if GPU acceleration is available via CUDA
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CUDA_VISIBLE_DEVICES"));
    }
}

/// <summary>Extension methods for BatchJob</summary>
public static class BatchJobExtensions
{
    /// <summary>Calculates the progress percentage of the batch job.</summary>
    /// <param name="batch">The batch job instance.</param>
    /// <returns>The progress percentage (0-100), or 0 if there are no tasks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="batch"/> is <see langword="null"/>.</exception>
    public static int GetProgressPercent(this BatchJob batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        return batch.Tasks.Count == 0 ? 0 : (batch.Tasks.Count(t => t.State == ProcessingState.Completed) * 100) / batch.Tasks.Count;
    }

    /// <summary>Determines whether all tasks in the batch are completed (either successfully or with failure).</summary>
    /// <param name="batch">The batch job instance.</param>
    /// <returns><see langword="true"/> if all tasks are completed or failed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="batch"/> is <see langword="null"/>.</exception>
    public static bool IsCompleted(this BatchJob batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        return batch.Tasks.Count > 0 &&
               batch.Tasks.All(t => t.State == ProcessingState.Completed || t.State == ProcessingState.Failed);
    }

    /// <summary>Calculates the estimated time remaining for the batch job to complete.</summary>
    /// <param name="batch">The batch job instance.</param>
    /// <returns>The estimated time remaining, or <see langword="null"/> if no tasks are completed yet.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="batch"/> is <see langword="null"/>.</exception>
    public static TimeSpan? GetEstimatedTimeRemaining(this BatchJob batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        var completedState = ProcessingState.Completed;
        var completedTasks = batch.Tasks.Where(t => t.State == completedState).ToList();

        if (completedTasks.Count == 0) return null;

        var avgTimePerTask = batch.Tasks
            .Where(t => t.State == completedState && t.CompletedAt.HasValue)
            .Average(t => (t.CompletedAt!.Value - t.CreatedAt).TotalSeconds);

        var remainingTasks = batch.Tasks.Count(t => t.State != completedState);
        return TimeSpan.FromSeconds(avgTimePerTask * remainingTasks);
    }

    /// <summary>Calculates the success rate of the batch job (completed tasks / total tasks).</summary>
    /// <param name="batch">The batch job instance.</param>
    /// <returns>The success rate as a value between 0 and 1, or 0 if there are no tasks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="batch"/> is <see langword="null"/>.</exception>
    public static double GetSuccessRate(this BatchJob batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        return batch.Tasks.Count == 0 ? 0 : (double)batch.Tasks.Count(t => t.State == ProcessingState.Completed) / batch.Tasks.Count;
    }
}

/// <summary>Extension methods for DownloadResult</summary>
public static class DownloadResultExtensions
{
    /// <summary>Formats the file size into a human-readable string with appropriate units.</summary>
    /// <param name="result">The download result instance.</param>
    /// <returns>A formatted string representing the file size (e.g., "2.50 GB", "450.25 MB", "12.34 KB", or "1234 B").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="result.OutputFileSizeBytes"/> is negative.</exception>
    public static string GetFormattedFileSize(this DownloadResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentOutOfRangeException.ThrowIfNegative(result.OutputFileSizeBytes);

        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;

        return result.OutputFileSizeBytes switch
        {
            >= GB => $"{result.OutputFileSizeBytes / (double)GB:F2} GB",
            >= MB => $"{result.OutputFileSizeBytes / (double)MB:F2} MB",
            >= KB => $"{result.OutputFileSizeBytes / (double)KB:F2} KB",
            _ => $"{result.OutputFileSizeBytes} B"
        };
    }

    /// <summary>Calculates the download speed in bytes per second.</summary>
    /// <param name="result">The download result instance.</param>
    /// <returns>The download speed in bytes per second, or 0 if file size is 0 or processing time is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="result.ProcessingTimeMs"/> is negative.</exception>
    public static double GetDownloadSpeed(this DownloadResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentOutOfRangeException.ThrowIfNegative(result.ProcessingTimeMs);

        if (result.OutputFileSizeBytes == 0 || result.ProcessingTimeMs == 0)
            return 0;

        var durationSeconds = result.ProcessingTimeMs / 1000.0;
        return result.OutputFileSizeBytes / durationSeconds;
    }

    /// <summary>Formats the download speed into a human-readable string with appropriate units.</summary>
    /// <param name="result">The download result instance.</param>
    /// <returns>A formatted string representing the download speed (e.g., "2.50 MB/s" or "12.34 KB/s").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <see langword="null"/>.</exception>
    public static string GetFormattedDownloadSpeed(this DownloadResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var speedBytesPerSec = GetDownloadSpeed(result);
        const long MB = 1024 * 1024;

        if (speedBytesPerSec >= MB)
            return $"{speedBytesPerSec / MB:F2} MB/s";

        return $"{speedBytesPerSec / 1024:F2} KB/s";
    }
}