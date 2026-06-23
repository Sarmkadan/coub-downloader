#nullable enable
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
    /// <summary>
    /// Extracts an audio track from a video file.
    /// </summary>
    /// <param name="videoPath">The path to the input video file.</param>
    /// <param name="outputPath">The path to save the extracted audio.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The path to the extracted audio file.</returns>
    Task<string> ExtractAudioAsync(string videoPath, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loops audio to match the target duration.
    /// </summary>
    /// <param name="audioPath">The path to the input audio file.</param>
    /// <param name="targetDuration">The target duration in seconds.</param>
    /// <param name="outputPath">The path to save the looped audio.</param>
    /// <param name="strategy">The loop strategy to use.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The path to the looped audio file.</returns>
    Task<string> LoopAudioAsync(string audioPath, double targetDuration, string outputPath, AudioLoopStrategy strategy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies audio effects such as fade in/out based on track settings.
    /// </summary>
    /// <param name="audioPath">The path to the input audio file.</param>
    /// <param name="outputPath">The path to save the processed audio.</param>
    /// <param name="trackSettings">The audio track settings.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The path to the processed audio file.</returns>
    Task<string> ApplyAudioEffectsAsync(string audioPath, string outputPath, AudioTrack trackSettings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes audio with the video duration.
    /// </summary>
    /// <param name="audioPath">The path to the audio file.</param>
    /// <param name="videoPath">The path to the video file.</param>
    /// <param name="outputPath">The path to save the synced audio.</param>
    /// <param name="strategy">The loop strategy to use.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The path to the synced audio file.</returns>
    Task<string> SyncAudioWithVideoAsync(string audioPath, string videoPath, string outputPath, AudioLoopStrategy strategy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the duration of an audio file in seconds.
    /// </summary>
    /// <param name="audioPath">The path to the audio file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The duration in seconds.</returns>
    Task<double> GetAudioDurationAsync(string audioPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjusts the volume level of an audio file.
    /// </summary>
    /// <param name="audioPath">The path to the input audio file.</param>
    /// <param name="outputPath">The path to save the audio file with adjusted volume.</param>
    /// <param name="volumeLevel">The volume multiplier (e.g., 1.0 for original volume).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The path to the audio file with adjusted volume.</returns>
    Task<string> AdjustVolumeAsync(string audioPath, string outputPath, double volumeLevel, CancellationToken cancellationToken = default);
}
