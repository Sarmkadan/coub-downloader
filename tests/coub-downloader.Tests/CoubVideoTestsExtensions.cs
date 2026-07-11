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
    /// <param name="_">Test fixture parameter</param>
    /// <returns>A new <see cref="CoubVideo"/> instance with vertical orientation</returns>
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
    /// <param name="_">Test fixture parameter</param>
    /// <returns>A new <see cref="CoubVideo"/> instance with HD landscape orientation</returns>
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
    /// <param name="_">Test fixture parameter</param>
    /// <returns>A new <see cref="CoubVideo"/> instance with 4K resolution</returns>
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
    /// <param name="_">Test fixture parameter</param>
    /// <returns>A new <see cref="CoubVideo"/> instance with short duration</returns>
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is null</exception>
    public static bool IsPopular(this CoubVideo video, long threshold = 10000)
    {
        ArgumentNullException.ThrowIfNull(video);

        return video.ViewCount >= threshold;
    }

    /// <summary>
    /// Gets the video's resolution category based on its dimensions
    /// </summary>
    /// <param name="video">The video to categorize</param>
    /// <returns>A string describing the resolution category</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is null</exception>
    public static string GetResolutionCategory(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);

        return video switch
        {
            _ when video.Is4kQuality() => "4K",
            _ when video.IsHdQuality() => "HD",
            _ when video.Width >= 1280 && video.Height >= 720 => "SD",
            _ => "Low"
        };
    }

    /// <summary>
    /// Calculates the total duration including audio track looping
    /// </summary>
    /// <param name="video">The video with optional audio track</param>
    /// <returns>The total duration after accounting for audio looping</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is null</exception>
    public static double GetTotalDurationWithAudio(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);

        return video.AudioTrack is null
            ? video.Duration
            : video.CalculateRequiredAudioDuration();
    }

    /// <summary>
    /// Determines if the video meets minimum quality requirements for processing
    /// </summary>
    /// <param name="video">The video to validate</param>
    /// <returns>True if video is valid and has minimum quality, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is null</exception>
    public static bool IsProcessable(this CoubVideo video)
    {
        ArgumentNullException.ThrowIfNull(video);

        return video.IsValid() && video.Duration > 0 && video.Width > 0 && video.Height > 0;
    }
}
