#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Service for video format conversion using FFmpeg.
/// </summary>
public interface IVideoConversionService
{
    /// <summary>
    /// Converts a video to the target format with specified settings.
    /// </summary>
    /// <param name="inputPath">The path to the input video file.</param>
    /// <param name="outputPath">The path to save the converted video.</param>
    /// <param name="settings">The conversion settings.</param>
    /// <param name="progress">An optional progress reporter.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The path to the converted video file.</returns>
    Task<string> ConvertVideoAsync(string inputPath, string outputPath, ConversionSettings settings, IProgress<int>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets video metadata using FFprobe.
    /// </summary>
    /// <param name="filePath">The path to the video file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="VideoMetadata"/> object.</returns>
    Task<VideoMetadata> GetVideoMetadataAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies an audio track to a video file.
    /// </summary>
    /// <param name="videoPath">The path to the video file.</param>
    /// <param name="audioPath">The path to the audio file.</param>
    /// <param name="outputPath">The path to save the result.</param>
    /// <param name="settings">The conversion settings.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The path to the output video file.</returns>
    Task<string> ApplyAudioTrackAsync(string videoPath, string audioPath, string outputPath, ConversionSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if FFmpeg is installed and available.
    /// </summary>
    /// <returns><c>true</c> if FFmpeg is available; otherwise, <c>false</c>.</returns>
    Task<bool> IsFfmpegAvailableAsync();

    /// <summary>
    /// Gets the FFmpeg version information.
    /// </summary>
    /// <returns>The version string.</returns>
    Task<string> GetFfmpegVersionAsync();

    /// <summary>
    /// Rescales the video to target dimensions.
    /// </summary>
    /// <param name="inputPath">The path to the input video.</param>
    /// <param name="outputPath">The path to save the output video.</param>
    /// <param name="width">The target width.</param>
    /// <param name="height">The target height.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The path to the rescaled video.</returns>
    Task<string> RescaleVideoAsync(string inputPath, string outputPath, int width, int height, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts a video to YouTube Shorts / TikTok 9:16 vertical format (1080x1920).
    /// The source video is scaled to fit within the frame and centred over a blurred
    /// full-frame background so letterboxed content looks polished.
    /// </summary>
    /// <param name="inputPath">The path to the input video.</param>
    /// <param name="outputPath">The path to save the output video.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The path to the converted video.</returns>
    Task<string> ConvertToShortsAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Video metadata extracted from FFprobe.
/// </summary>
public class VideoMetadata
{
    public string? Format { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double Duration { get; set; }
    public string? VideoCodec { get; set; }
    public string? AudioCodec { get; set; }
    public int VideoBitrate { get; set; }
    public int AudioBitrate { get; set; }
    public int FrameRate { get; set; }
    public bool HasAudio { get; set; }
    public long FileSizeBytes { get; set; }
}
