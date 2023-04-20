#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents video processing extensions and metadata for Coub videos.
/// This record contains additional processing information and settings.
/// </summary>
public sealed record CoubVideoProcessingExtensions
{
    /// <summary>Unique identifier for the video extensions</summary>
    [Required]
    public string Id { get; init; } = null!;

    /// <summary>Video ID this extensions belongs to</summary>
    [Required]
    public string VideoId { get; init; } = null!;

    /// <summary>Processing priority level (0-100)</summary>
    [Range(0, 100)]
    public int Priority { get; init; } = 50;

    /// <summary>Whether to enable hardware acceleration for processing</summary>
    public bool EnableHardwareAcceleration { get; init; } = true;

    /// <summary>Target video codec (h264, h265, vp9, av1)</summary>
    [StringLength(20)]
    public string VideoCodec { get; init; } = "h264";

    /// <summary>Target audio codec (aac, opus, mp3, flac)</summary>
    [StringLength(20)]
    public string AudioCodec { get; init; } = "aac";

    /// <summary>Target video bitrate in kbps</summary>
    [Range(100, 100000)]
    public int VideoBitrate { get; init; } = 2500;

    /// <summary>Target audio bitrate in kbps</summary>
    [Range(32, 1000)]
    public int AudioBitrate { get; init; } = 192;

    /// <summary>Target video resolution width</summary>
    [Range(100, 7680)]
    public int TargetWidth { get; init; } = 1920;

    /// <summary>Target video resolution height</summary>
    [Range(100, 7680)]
    public int TargetHeight { get; init; } = 1080;

    /// <summary>Whether to preserve original aspect ratio</summary>
    public bool PreserveAspectRatio { get; init; } = true;

    /// <summary>Maximum duration in seconds for processing</summary>
    [Range(1.0, 600.0)]
    public double MaxDuration { get; init; } = 30.0;

    /// <summary>Additional processing tags/categories</summary>
    public List<string> Tags { get; init; } = new();

    /// <summary>Custom processing profile name</summary>
    [StringLength(100)]
    public string? ProfileName { get; init; }

    /// <summary>Creation timestamp</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Last update timestamp</summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Validate the extensions properties</summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id)
            && !string.IsNullOrWhiteSpace(VideoId)
            && Priority >= 0 && Priority <= 100
            && VideoBitrate >= 100 && VideoBitrate <= 100000
            && AudioBitrate >= 32 && AudioBitrate <= 1000
            && TargetWidth >= 100 && TargetWidth <= 7680
            && TargetHeight >= 100 && TargetHeight <= 7680
            && MaxDuration >= 1.0 && MaxDuration <= 600.0;
    }
}