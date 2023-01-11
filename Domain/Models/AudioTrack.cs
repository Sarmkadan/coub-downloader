// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Represents an audio track extracted from a Coub video.
/// </summary>
public class AudioTrack
{
    /// <summary>Unique identifier for the audio track</summary>
    [Required]
    public string Id { get; set; } = null!;

    /// <summary>Reference to the parent CoubVideo</summary>
    [Required]
    public string VideoId { get; set; } = null!;

    /// <summary>Duration of the audio track in seconds</summary>
    [Range(0.1, 3600)]
    public double Duration { get; set; }

    /// <summary>Sample rate in Hz</summary>
    [Range(8000, 192000)]
    public int SampleRate { get; set; } = 44100;

    /// <summary>Number of audio channels (1=mono, 2=stereo)</summary>
    [Range(1, 8)]
    public int Channels { get; set; } = 2;

    /// <summary>Bitrate in kbps</summary>
    [Range(16, 320)]
    public int Bitrate { get; set; } = 128;

    /// <summary>Audio codec used</summary>
    [StringLength(50)]
    public string Codec { get; set; } = "aac";

    /// <summary>Path to the audio file</summary>
    public string? FilePath { get; set; }

    /// <summary>Audio looping strategy for synchronization</summary>
    public AudioLoopStrategy LoopStrategy { get; set; } = AudioLoopStrategy.Repeat;

    /// <summary>Number of times the audio should loop</summary>
    [Range(1, 1000)]
    public int LoopCount { get; set; } = 1;

    /// <summary>Fade-in duration in milliseconds</summary>
    [Range(0, 5000)]
    public int FadeInMs { get; set; } = 0;

    /// <summary>Fade-out duration in milliseconds</summary>
    [Range(0, 5000)]
    public int FadeOutMs { get; set; } = 0;

    /// <summary>Audio volume level (0.0 - 2.0)</summary>
    [Range(0.0, 2.0)]
    public double VolumeLevel { get; set; } = 1.0;

    /// <summary>Calculated time to sync audio with video</summary>
    public double SyncDuration { get; set; }

    /// <summary>Creation timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Validate audio track properties</summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id)
            && !string.IsNullOrWhiteSpace(VideoId)
            && Duration > 0
            && SampleRate > 0
            && Channels > 0
            && Bitrate > 0;
    }

    /// <summary>Calculate the actual audio duration after looping</summary>
    public double CalculateLoopedDuration()
    {
        return LoopStrategy switch
        {
            AudioLoopStrategy.None => Duration,
            AudioLoopStrategy.Repeat => Duration * LoopCount,
            AudioLoopStrategy.Stretch => Duration,
            AudioLoopStrategy.Crossfade => Duration * LoopCount,
            _ => Duration
        };
    }

    /// <summary>Get audio specification as a formatted string</summary>
    public string GetAudioSpec() => $"{SampleRate}Hz {Channels}ch {Bitrate}kbps {Codec}";
}
