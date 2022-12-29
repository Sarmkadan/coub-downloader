// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents a Coub playlist sourced from a channel feed or tag page,
/// containing an ordered collection of video URLs ready for batch processing.
/// </summary>
public class CoubPlaylist
{
    /// <summary>Unique identifier composed of source type and slug (e.g., "channel_funny" or "tag_cats").</summary>
    [Required]
    public string Id { get; set; } = null!;

    /// <summary>Human-readable title derived from the channel name or tag slug.</summary>
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    /// <summary>Optional description of the playlist or its source channel.</summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Original URL of the Coub channel page or tag page used to discover videos.</summary>
    [Required]
    [Url]
    public string PlaylistUrl { get; set; } = null!;

    /// <summary>Ordered list of Coub video URLs contained in this playlist.</summary>
    public List<string> VideoUrls { get; set; } = new();

    /// <summary>Total number of videos discovered in the playlist.</summary>
    public int TotalVideos => VideoUrls.Count;

    /// <summary>Optional cap on how many videos to include; <c>null</c> means no limit.</summary>
    [Range(1, 500)]
    public int? MaxVideos { get; set; }

    /// <summary>UTC timestamp when this playlist record was created locally.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the last successful API fetch, or <c>null</c> if not yet fetched.</summary>
    public DateTime? FetchedAt { get; set; }

    /// <summary>Check whether the playlist has been fetched and contains at least one video URL.</summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Id)
        && !string.IsNullOrWhiteSpace(PlaylistUrl)
        && VideoUrls.Count > 0;

    /// <summary>Check whether the playlist contains no video URLs.</summary>
    public bool IsEmpty() => VideoUrls.Count == 0;

    /// <summary>Return the effective list of URLs, respecting <see cref="MaxVideos"/> if set.</summary>
    public IEnumerable<string> GetEffectiveVideoUrls() =>
        MaxVideos.HasValue ? VideoUrls.Take(MaxVideos.Value) : VideoUrls;
}
