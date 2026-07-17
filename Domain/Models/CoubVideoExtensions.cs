#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
using System.ComponentModel.DataAnnotations;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents video processing extensions and metadata for Coub videos.
/// This record contains additional processing information and settings.
/// </summary>
public sealed record CoubVideoProcessingExtensions
{
    /// <summary>Gets the unique identifier for the video extensions.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    [Required]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the video ID this extensions belongs to.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    [Required]
    public string VideoId { get; init; } = string.Empty;

    /// <summary>Gets or sets the processing priority level (0-100).</summary>
    [Range(0, 100)]
    public int Priority { get; init; } = 50;

    /// <summary>Gets or sets a value indicating whether to enable hardware acceleration for processing.</summary>
    public bool EnableHardwareAcceleration { get; init; } = true;

    /// <summary>Gets or sets the target video codec (h264, h265, vp9, av1).</summary>
    [StringLength(20)]
    public string VideoCodec { get; init; } = "h264";

    /// <summary>Gets or sets the target audio codec (aac, opus, mp3, flac).</summary>
    [StringLength(20)]
    public string AudioCodec { get; init; } = "aac";

    /// <summary>Gets or sets the target video bitrate in kbps.</summary>
    [Range(100, 100000)]
    public int VideoBitrate { get; init; } = 2500;

    /// <summary>Gets or sets the target audio bitrate in kbps.</summary>
    [Range(32, 1000)]
    public int AudioBitrate { get; init; } = 192;

    /// <summary>Gets or sets the target video resolution width.</summary>
    [Range(100, 7680)]
    public int TargetWidth { get; init; } = 1920;

    /// <summary>Gets or sets the target video resolution height.</summary>
    [Range(100, 7680)]
    public int TargetHeight { get; init; } = 1080;

    /// <summary>Gets or sets a value indicating whether to preserve original aspect ratio.</summary>
    public bool PreserveAspectRatio { get; init; } = true;

    /// <summary>Gets or sets the maximum duration in seconds for processing.</summary>
    [Range(1.0, 600.0)]
    public double MaxDuration { get; init; } = 30.0;

    /// <summary>Gets the additional processing tags/categories.</summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>Gets or sets the custom processing profile name.</summary>
    [StringLength(100)]
    public string? ProfileName { get; init; }

    /// <summary>Gets the creation timestamp.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Validates the extensions properties.
    /// </summary>
    /// <returns>True if the extensions are valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="Id"/> or <paramref name="VideoId"/> is null or whitespace.</exception>
    public bool IsValid()
    {
        ArgumentNullException.ThrowIfNullOrEmpty(Id);
        ArgumentNullException.ThrowIfNullOrEmpty(VideoId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Priority);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Priority, 100);
        ArgumentOutOfRangeException.ThrowIfLessThan(VideoBitrate, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(VideoBitrate, 100000);
        ArgumentOutOfRangeException.ThrowIfLessThan(AudioBitrate, 32);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(AudioBitrate, 1000);
        ArgumentOutOfRangeException.ThrowIfLessThan(TargetWidth, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(TargetWidth, 7680);
        ArgumentOutOfRangeException.ThrowIfLessThan(TargetHeight, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(TargetHeight, 7680);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxDuration, 1.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(MaxDuration, 600.0);

        return true;
    }
}