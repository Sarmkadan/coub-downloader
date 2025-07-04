// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Domain.Extensions;

/// <summary>Extension methods for domain models</summary>
public static class CoubVideoExtensions
{
    /// <summary>Get aspect ratio as decimal (width/height)</summary>
    public static double GetAspectRatio(this CoubVideo video)
    {
        return video.Height > 0 ? (double)video.Width / video.Height : 16.0 / 9.0;
    }

    /// <summary>Check if video is in vertical format (TikTok/Shorts style)</summary>
    public static bool IsVerticalFormat(this CoubVideo video)
    {
        return video.GetAspectRatio() < 1;
    }

    /// <summary>Check if video is HD quality</summary>
    public static bool IsHdQuality(this CoubVideo video)
    {
        return video.Width >= 1280 && video.Height >= 720;
    }

    /// <summary>Check if video is 4K quality</summary>
    public static bool Is4kQuality(this CoubVideo video)
    {
        return video.Width >= 3840 && video.Height >= 2160;
    }

    /// <summary>Calculate required audio duration to match video</summary>
    public static double CalculateRequiredAudioDuration(this CoubVideo video)
    {
        if (video.AudioTrack == null) return 0;

        var requiredLoops = Math.Ceiling(video.Duration / video.AudioTrack.Duration);
        return video.AudioTrack.Duration * requiredLoops;
    }

    /// <summary>Get video classification by duration</summary>
    public static string GetDurationCategory(this CoubVideo video)
    {
        return video.Duration switch
        {
            < 6 => "Short",
            < 15 => "Medium",
            < 30 => "Long",
            _ => "Extra Long"
        };
    }

    /// <summary>Get formatted view count</summary>
    public static string GetFormattedViewCount(this CoubVideo video)
    {
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
    /// <summary>Get audio specification string</summary>
    public static string GetAudioSpec(this AudioTrack track)
    {
        return $"{track.SampleRate}Hz {track.Channels}ch {track.Codec} {track.Bitrate}kbps";
    }

    /// <summary>Calculate total duration after looping</summary>
    public static double CalculateLoopedDuration(this AudioTrack track)
    {
        return track.Duration * (track.LoopCount + 1);
    }

    /// <summary>Check if audio quality is lossless</summary>
    public static bool IsLossless(this AudioTrack track)
    {
        return track.Codec.Contains("flac", StringComparison.OrdinalIgnoreCase) ||
               track.Codec.Contains("pcm", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Check if audio is stereo</summary>
    public static bool IsStereo(this AudioTrack track)
    {
        return track.Channels == 2;
    }

    /// <summary>Check if audio is mono</summary>
    public static bool IsMono(this AudioTrack track)
    {
        return track.Channels == 1;
    }

    /// <summary>Check if audio supports surround sound</summary>
    public static bool IsSurround(this AudioTrack track)
    {
        return track.Channels > 2;
    }
}

/// <summary>Extension methods for ConversionSettings</summary>
public static class ConversionSettingsExtensions
{
    /// <summary>Get FFmpeg codec parameters string</summary>
    public static string GetFFmpegCodecParams(this ConversionSettings settings)
    {
        return $"-c:v {settings.VideoCodec} -c:a {settings.AudioCodec} " +
               $"-b:v {settings.VideoBitrate}k -b:a {settings.AudioBitrate}k";
    }

    /// <summary>Calculate total bitrate</summary>
    public static int GetTotalBitrate(this ConversionSettings settings)
    {
        return settings.VideoBitrate + settings.AudioBitrate;
    }

    /// <summary>Check if hardware acceleration should be used</summary>
    public static bool ShouldUseHardwareAcceleration(this ConversionSettings settings)
    {
        return settings.EnableHardwareAcceleration && IsHardwareAvailable();
    }

    /// <summary>Estimate output file size</summary>
    public static long EstimateOutputSize(this ConversionSettings settings, double durationSeconds)
    {
        var bitrate = settings.GetTotalBitrate();
        return (long)(bitrate * 1000 / 8 * durationSeconds); // bits to bytes
    }

    private static bool IsHardwareAvailable()
    {
        // Check if GPU acceleration is available
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CUDA_VISIBLE_DEVICES"));
    }
}

/// <summary>Extension methods for BatchJob</summary>
public static class BatchJobExtensions
{
    /// <summary>Get progress percentage</summary>
    public static int GetProgressPercent(this BatchJob batch)
    {
        if (batch.Tasks.Count == 0) return 0;
        return (batch.Tasks.Count(t => t.State == ProcessingState.Completed) * 100) / batch.Tasks.Count;
    }

    /// <summary>Check if all tasks are completed</summary>
    public static bool IsCompleted(this BatchJob batch)
    {
        return batch.Tasks.Count > 0 &&
               batch.Tasks.All(t => t.State == ProcessingState.Completed || t.State == ProcessingState.Failed);
    }

    /// <summary>Get estimated completion time</summary>
    public static TimeSpan? GetEstimatedTimeRemaining(this BatchJob batch)
    {
        var completedState = ProcessingState.Completed;
        var completedTasks = batch.Tasks.Where(t => t.State == completedState).ToList();

        if (completedTasks.Count == 0)
            return null;

        var avgTimePerTask = batch.Tasks
            .Where(t => t.State == completedState && t.CompletedAt.HasValue)
            .Average(t => (t.CompletedAt!.Value - t.CreatedAt).TotalSeconds);

        var remainingTasks = batch.Tasks.Count(t => t.State != completedState);
        return TimeSpan.FromSeconds(avgTimePerTask * remainingTasks);
    }

    /// <summary>Calculate success rate</summary>
    public static double GetSuccessRate(this BatchJob batch)
    {
        if (batch.Tasks.Count == 0) return 0;
        return (double)batch.Tasks.Count(t => t.State == ProcessingState.Completed) / batch.Tasks.Count;
    }
}

/// <summary>Extension methods for DownloadResult</summary>
public static class DownloadResultExtensions
{
    /// <summary>Get human-readable file size</summary>
    public static string GetFormattedFileSize(this DownloadResult result)
    {
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

    /// <summary>Get download speed</summary>
    public static double GetDownloadSpeed(this DownloadResult result)
    {
        if (result.OutputFileSizeBytes == 0 || result.ProcessingTimeMs == 0)
            return 0;

        var durationSeconds = result.ProcessingTimeMs / 1000.0;
        return result.OutputFileSizeBytes / durationSeconds;
    }

    /// <summary>Get formatted download speed</summary>
    public static string GetFormattedDownloadSpeed(this DownloadResult result)
    {
        var speedBytesPerSec = GetDownloadSpeed(result);
        const long MB = 1024 * 1024;

        if (speedBytesPerSec >= MB)
            return $"{speedBytesPerSec / MB:F2} MB/s";

        return $"{speedBytesPerSec / 1024:F2} KB/s";
    }
}
