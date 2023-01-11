// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Enums;
using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for audio extraction and loop synchronization.
/// </summary>
public interface IAudioProcessingService
{
    /// <summary>Extract audio track from video file</summary>
    Task<string> ExtractAudioAsync(string videoPath, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>Loop audio to match video duration</summary>
    Task<string> LoopAudioAsync(string audioPath, double targetDuration, string outputPath, AudioLoopStrategy strategy, CancellationToken cancellationToken = default);

    /// <summary>Apply audio effects (fade in/out)</summary>
    Task<string> ApplyAudioEffectsAsync(string audioPath, string outputPath, AudioTrack trackSettings, CancellationToken cancellationToken = default);

    /// <summary>Sync audio with video duration</summary>
    Task<string> SyncAudioWithVideoAsync(string audioPath, string videoPath, string outputPath, AudioLoopStrategy strategy, CancellationToken cancellationToken = default);

    /// <summary>Get audio duration in seconds</summary>
    Task<double> GetAudioDurationAsync(string audioPath, CancellationToken cancellationToken = default);

    /// <summary>Adjust audio volume level</summary>
    Task<string> AdjustVolumeAsync(string audioPath, string outputPath, double volumeLevel, CancellationToken cancellationToken = default);
}
