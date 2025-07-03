// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Enums;

/// <summary>
/// Represents the supported video output formats.
/// </summary>
public enum VideoFormat
{
    /// <summary>MPEG-4 video format with H.264 codec</summary>
    MP4,

    /// <summary>WebM video format with VP9 codec</summary>
    WebM,

    /// <summary>Mobile shorts format optimized for TikTok/Instagram Reels</summary>
    MobileShorts,

    /// <summary>YouTube Shorts format (vertical video)</summary>
    YouTubeShorts,

    /// <summary>High-quality 4K format</summary>
    HighQuality4K
}

/// <summary>
/// Represents the processing state of a download task.
/// </summary>
public enum ProcessingState
{
    /// <summary>Task is pending execution</summary>
    Pending,

    /// <summary>Currently downloading from Coub</summary>
    Downloading,

    /// <summary>Converting video format</summary>
    Converting,

    /// <summary>Processing audio loops</summary>
    ProcessingAudio,

    /// <summary>Task completed successfully</summary>
    Completed,

    /// <summary>Task failed with error</summary>
    Failed,

    /// <summary>Task was cancelled by user</summary>
    Cancelled
}

/// <summary>
/// Represents the quality level of the video output.
/// </summary>
public enum VideoQuality
{
    /// <summary>Low quality, 480p resolution</summary>
    Low,

    /// <summary>Medium quality, 720p resolution</summary>
    Medium,

    /// <summary>High quality, 1080p resolution</summary>
    High,

    /// <summary>Maximum available quality</summary>
    Maximum
}

/// <summary>
/// Represents the audio looping strategy.
/// </summary>
public enum AudioLoopStrategy
{
    /// <summary>No looping, use original audio</summary>
    None,

    /// <summary>Sync audio with video duration by repeating</summary>
    Repeat,

    /// <summary>Stretch audio to match video duration</summary>
    Stretch,

    /// <summary>Cross-fade repeating audio segments</summary>
    Crossfade
}
