// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents a section or segment of a video with specific timing and properties.
/// </summary>
public class VideoSection
{
    /// <summary>Unique identifier for the section</summary>
    [Required]
    public string Id { get; set; } = null!;

    /// <summary>Reference to the parent CoubVideo</summary>
    [Required]
    public string VideoId { get; set; } = null!;

    /// <summary>Section index/order</summary>
    [Range(0, int.MaxValue)]
    public int Index { get; set; }

    /// <summary>Start time in seconds</summary>
    [Range(0, double.MaxValue)]
    public double StartTime { get; set; }

    /// <summary>End time in seconds</summary>
    [Range(0, double.MaxValue)]
    public double EndTime { get; set; }

    /// <summary>Section description</summary>
    [StringLength(200)]
    public string? Description { get; set; }

    /// <summary>Whether this section should be included in final video</summary>
    public bool IsIncluded { get; set; } = true;

    /// <summary>Transition effect type between sections</summary>
    [StringLength(50)]
    public string? TransitionEffect { get; set; } = "cut";

    /// <summary>Duration of transition in milliseconds</summary>
    [Range(0, 5000)]
    public int TransitionDurationMs { get; set; } = 0;

    /// <summary>Calculate section duration</summary>
    public double GetDuration() => EndTime - StartTime;

    /// <summary>Validate section properties</summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id)
            && !string.IsNullOrWhiteSpace(VideoId)
            && StartTime >= 0
            && EndTime > StartTime;
    }
}
