#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Models;
using CoubDownloader.Domain.Extensions;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Tests;

public static class CoubVideoTestsExtensions
{
    /// <summary>
    /// Creates a video with portrait orientation (vertical format)
    /// </summary>
    public static CoubVideo CreateVerticalVideo(this CoubVideoTests _) => new()
    {
        Id = "vertical123",
        Title = "Vertical Test Video",
        Url = "https://coub.com/view/vertical123",
        Duration = 10.0,
        Width = 720,
        Height = 1280
    };

    /// <summary>
    /// Creates a video with landscape orientation and HD quality
    /// </summary>
    public static CoubVideo CreateHdLandscapeVideo(this CoubVideoTests _) => new()
    {
        Id = "hd123",
        Title = "HD Landscape Video",
        Url = "https://coub.com/view/hd123",
        Duration = 25.0,
        Width = 1920,
        Height = 1080,
        ViewCount = 15000
    };

    /// <summary>
    /// Creates a video with 4K quality dimensions
    /// </summary>
    public static CoubVideo Create4kVideo(this CoubVideoTests _) => new()
    {
        Id = "4k123",
        Title = "4K Test Video",
        Url = "https://coub.com/view/4k123",
        Duration = 30.0,
        Width = 3840,
        Height = 2160,
        ViewCount = 500000
    };

    /// <summary>
    /// Creates a video with a short duration (under 5 seconds)
    /// </summary>
    public static CoubVideo CreateShortDurationVideo(this CoubVideoTests _) => new()
    {
        Id = "short123",
        Title = "Short Video",
        Url = "https://coub.com/view/short123",
        Duration = 3.0,
        Width = 800,
        Height = 600
    };

    /// <summary>
    /// Determines if the video is considered "popular" based on view count threshold
    /// </summary>
    /// <param name="video">The video to check</param>
    /// <param name="threshold">Minimum view count to be considered popular</param>
    /// <returns>True if video has at least the threshold views, false otherwise</returns>
    public static bool IsPopular(this CoubVideo video, long threshold = 10000)
    {
        return video.ViewCount >= threshold;
    }

    /// <summary>
    /// Gets the video's resolution category based on its dimensions
    /// </summary>
    /// <param name="video">The video to categorize</param>
    /// <returns>A string describing the resolution category</returns>
    public static string GetResolutionCategory(this CoubVideo video)
    {
        if (video.Is4kQuality())
        {
            return "4K";
        }

        if (video.IsHdQuality())
        {
            return "HD";
        }

        if (video.Width >= 1280 && video.Height >= 720)
        {
            return "SD";
        }

        return "Low";
    }

    /// <summary>
    /// Calculates the total duration including audio track looping
    /// </summary>
    /// <param name="video">The video with optional audio track</param>
    /// <returns>The total duration after accounting for audio looping</returns>
    public static double GetTotalDurationWithAudio(this CoubVideo video)
    {
        double baseDuration = video.Duration;

        if (video.AudioTrack != null)
        {
            baseDuration = video.CalculateRequiredAudioDuration();
        }

        return baseDuration;
    }

    /// <summary>
    /// Determines if the video meets minimum quality requirements for processing
    /// </summary>
    /// <param name="video">The video to validate</param>
    /// <returns>True if video is valid and has minimum quality, false otherwise</returns>
    public static bool IsProcessable(this CoubVideo video)
    {
        return video.IsValid() && video.Duration > 0 && video.Width > 0 && video.Height > 0;
    }
}
