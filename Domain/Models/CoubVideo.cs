// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents a Coub video with metadata and processing information.
/// </summary>
public class CoubVideo
{
    /// <summary>Unique identifier for the Coub video</summary>
    [Required]
    public string Id { get; set; } = null!;

    /// <summary>Title of the video</summary>
    [Required]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    /// <summary>Coub video URL</summary>
    [Required]
    [Url]
    public string Url { get; set; } = null!;

    /// <summary>Duration of the video in seconds</summary>
    [Range(0.1, 600)]
    public double Duration { get; set; }

    /// <summary>Width of the video in pixels</summary>
    [Range(100, 7680)]
    public int Width { get; set; }

    /// <summary>Height of the video in pixels</summary>
    [Range(100, 7680)]
    public int Height { get; set; }

    /// <summary>Video source URL for downloading</summary>
    public string? SourceUrl { get; set; }

    /// <summary>Thumbnail image URL</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Creator/author of the video</summary>
    [StringLength(255)]
    public string? CreatorName { get; set; }

    /// <summary>Description of the video</summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>Upload date of the video</summary>
    public DateTime? UploadedDate { get; set; }

    /// <summary>View count on Coub platform</summary>
    [Range(0, long.MaxValue)]
    public long ViewCount { get; set; }

    /// <summary>Whether the video has audio track</summary>
    public bool HasAudio { get; set; }

    /// <summary>Audio track information if available</summary>
    public AudioTrack? AudioTrack { get; set; }

    /// <summary>Collection of video chunks/sections</summary>
    public List<VideoSection> Sections { get; set; } = new();

    /// <summary>Creation timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last update timestamp</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Validate video properties</summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id)
            && !string.IsNullOrWhiteSpace(Title)
            && !string.IsNullOrWhiteSpace(Url)
            && Duration > 0
            && Width > 0
            && Height > 0;
    }

    /// <summary>Get video aspect ratio</summary>
    public decimal GetAspectRatio() => Width > 0 && Height > 0 ? (decimal)Width / Height : 1;

    /// <summary>Check if video is in vertical format (suitable for shorts)</summary>
    public bool IsVerticalFormat() => Height > Width;

    /// <summary>Calculate the required audio duration to sync with video</summary>
    public double CalculateRequiredAudioDuration() => Duration;
}
