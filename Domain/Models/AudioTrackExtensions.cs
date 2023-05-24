#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using CoubDownloader.Domain.Models;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Extension methods for <see cref="AudioTrack"/> providing additional functionality
/// for audio track manipulation and analysis.
/// </summary>
public static class AudioTrackExtensions
{
    private const double BytesPerMegabyte = 1024.0 * 1024.0;

    /// <summary>
    /// Gets the total duration in seconds including all loops.
    /// </summary>
    /// <param name="audioTrack">The audio track.</param>
    /// <returns>Total duration in seconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioTrack"/> is null.</exception>
    public static double GetTotalDuration(this AudioTrack audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);
        return audioTrack.CalculateLoopedDuration();
    }

    /// <summary>
    /// Gets the duration of a single loop iteration in seconds.
    /// </summary>
    /// <param name="audioTrack">The audio track.</param>
    /// <returns>Duration of single loop in seconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioTrack"/> is null.</exception>
    public static double GetSingleLoopDuration(this AudioTrack audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);
        return audioTrack.Duration;
    }

    /// <summary>
    /// Calculates the fade-in duration ratio (0.0 to 1.0).
    /// </summary>
    /// <param name="audioTrack">The audio track.</param>
    /// <returns>Fade-in ratio.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioTrack"/> is null.</exception>
    public static double GetFadeInRatio(this AudioTrack audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);

        if (audioTrack.FadeInMs <= 0 || audioTrack.Duration <= 0)
        {
            return 0.0;
        }

        return Math.Min(1.0, audioTrack.FadeInMs / (audioTrack.Duration * 1000.0));
    }

    /// <summary>
    /// Calculates the fade-out duration ratio (0.0 to 1.0).
    /// </summary>
    /// <param name="audioTrack">The audio track.</param>
    /// <returns>Fade-out ratio.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioTrack"/> is null.</exception>
    public static double GetFadeOutRatio(this AudioTrack audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);

        if (audioTrack.FadeOutMs <= 0 || audioTrack.Duration <= 0)
        {
            return 0.0;
        }

        return Math.Min(1.0, audioTrack.FadeOutMs / (audioTrack.Duration * 1000.0));
    }

    /// <summary>
    /// Determines if the audio track has any fade effects applied.
    /// </summary>
    /// <param name="audioTrack">The audio track.</param>
    /// <returns>True if any fade is applied, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioTrack"/> is null.</exception>
    public static bool HasFadeEffects(this AudioTrack audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);
        return audioTrack.FadeInMs > 0 || audioTrack.FadeOutMs > 0;
    }

    /// <summary>
    /// Gets the audio format description including channels and codec.
    /// </summary>
    /// <param name="audioTrack">The audio track.</param>
    /// <returns>Formatted audio format string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioTrack"/> is null.</exception>
    public static string GetAudioFormat(this AudioTrack audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);
        return $"{audioTrack.GetAudioSpec()} ({audioTrack.Channels} channels)";
    }

    /// <summary>
    /// Calculates the estimated file size in megabytes.
    /// </summary>
    /// <param name="audioTrack">The audio track.</param>
    /// <returns>Estimated file size in MB.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioTrack"/> is null.</exception>
    /// <exception cref="OverflowException">Thrown when the calculated file size exceeds <see cref="double.MaxValue"/>.</exception>
    public static double GetEstimatedFileSizeMb(this AudioTrack audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);

        // Calculate bytes per second: sampleRate * bitsPerSample * channels / 8
        // Assuming 16-bit samples (common for most audio formats)
        const int bitsPerSample = 16;
        double bytesPerSecond = audioTrack.SampleRate * bitsPerSample * audioTrack.Channels / 8.0;
        double bytesPerLoop = bytesPerSecond * audioTrack.Duration;
        double totalBytes = bytesPerLoop * audioTrack.LoopCount;

        return totalBytes / BytesPerMegabyte; // Convert to MB
    }

    /// <summary>
    /// Determines if the audio track needs volume normalization.
    /// </summary>
    /// <param name="audioTrack">The audio track.</param>
    /// <returns>True if volume is not at default level, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioTrack"/> is null.</exception>
    public static bool NeedsVolumeNormalization(this AudioTrack audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);
        return Math.Abs(audioTrack.VolumeLevel - 1.0) > 0.01;
    }

    /// <summary>
    /// Gets the audio track's age in days since creation.
    /// </summary>
    /// <param name="audioTrack">The audio track.</param>
    /// <returns>Age in days.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioTrack"/> is null.</exception>
    public static double GetAgeInDays(this AudioTrack audioTrack)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);
        return (DateTime.UtcNow - audioTrack.CreatedAt).TotalDays;
    }

    /// <summary>
    /// Determines if the audio track is relatively new (created within last 7 days).
    /// </summary>
    /// <param name="audioTrack">The audio track.</param>
    /// <param name="daysThreshold">The threshold in days to consider as "recent". Default is 7.</param>
    /// <returns>True if recently created, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioTrack"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="daysThreshold"/> is less than 0.</exception>
    public static bool IsRecentlyCreated(this AudioTrack audioTrack, int daysThreshold = 7)
    {
        ArgumentNullException.ThrowIfNull(audioTrack);
        ArgumentOutOfRangeException.ThrowIfNegative(daysThreshold);

        return audioTrack.GetAgeInDays() <= daysThreshold;
    }
}