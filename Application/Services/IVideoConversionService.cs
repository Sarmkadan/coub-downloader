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
    /// <summary>Convert video to target format with specified settings</summary>
    Task<string> ConvertVideoAsync(string inputPath, string outputPath, ConversionSettings settings, IProgress<int>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>Get video information/metadata using FFprobe</summary>
    Task<VideoMetadata> GetVideoMetadataAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>Apply audio track to video</summary>
    Task<string> ApplyAudioTrackAsync(string videoPath, string audioPath, string outputPath, ConversionSettings settings, CancellationToken cancellationToken = default);

    /// <summary>Check if FFmpeg is installed and available</summary>
    Task<bool> IsFfmpegAvailableAsync();

    /// <summary>Get FFmpeg version</summary>
    Task<string> GetFfmpegVersionAsync();

    /// <summary>Rescale video to target dimensions</summary>
    Task<string> RescaleVideoAsync(string inputPath, string outputPath, int width, int height, CancellationToken cancellationToken = default);
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
