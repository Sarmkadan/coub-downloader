// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using CoubDownloader.Domain.Enums;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Configuration settings for video conversion and processing.
/// </summary>
public class ConversionSettings
{
    /// <summary>Unique identifier for the settings</summary>
    [Required]
    public string Id { get; set; } = null!;

    /// <summary>Output video format</summary>
    public VideoFormat Format { get; set; } = VideoFormat.MP4;

    /// <summary>Output video quality</summary>
    public VideoQuality Quality { get; set; } = VideoQuality.High;

    /// <summary>Target video bitrate in kbps</summary>
    [Range(500, 20000)]
    public int VideoBitrate { get; set; } = 5000;

    /// <summary>Audio bitrate in kbps</summary>
    [Range(32, 320)]
    public int AudioBitrate { get; set; } = 128;

    /// <summary>Video codec to use</summary>
    [StringLength(50)]
    public string VideoCodec { get; set; } = "h264";

    /// <summary>Audio codec to use</summary>
    [StringLength(50)]
    public string AudioCodec { get; set; } = "aac";

    /// <summary>Frames per second for output</summary>
    [Range(15, 120)]
    public int FrameRate { get; set; } = 30;

    /// <summary>Output video width</summary>
    [Range(100, 7680)]
    public int Width { get; set; } = 1920;

    /// <summary>Output video height</summary>
    [Range(100, 7680)]
    public int Height { get; set; } = 1080;

    /// <summary>Audio looping strategy</summary>
    public AudioLoopStrategy AudioLoopStrategy { get; set; } = AudioLoopStrategy.Repeat;

    /// <summary>Preserve original aspect ratio</summary>
    public bool PreserveAspectRatio { get; set; } = true;

    /// <summary>Enable hardware acceleration</summary>
    public bool EnableHardwareAcceleration { get; set; } = true;

    /// <summary>Use multi-threading for faster processing</summary>
    public bool UseMultiThreading { get; set; } = true;

    /// <summary>Number of processing threads</summary>
    [Range(1, 32)]
    public int ThreadCount { get; set; } = Environment.ProcessorCount;

    /// <summary>Apply fade effects</summary>
    public bool ApplyFades { get; set; } = false;

    /// <summary>Fade in duration in milliseconds</summary>
    [Range(0, 5000)]
    public int FadeInMs { get; set; } = 500;

    /// <summary>Fade out duration in milliseconds</summary>
    [Range(0, 5000)]
    public int FadeOutMs { get; set; } = 500;

    /// <summary>Creation timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Get FFmpeg codec parameters for conversion</summary>
    public string GetFFmpegCodecParams()
    {
        return $"-c:v {VideoCodec} -b:v {VideoBitrate}k -c:a {AudioCodec} -b:a {AudioBitrate}k";
    }

    /// <summary>Get quality-based default settings</summary>
    public void ApplyQualityPreset()
    {
        switch (Quality)
        {
            case VideoQuality.Low:
                Width = 854;
                Height = 480;
                VideoBitrate = 1500;
                FrameRate = 24;
                break;
            case VideoQuality.Medium:
                Width = 1280;
                Height = 720;
                VideoBitrate = 2500;
                FrameRate = 30;
                break;
            case VideoQuality.High:
                Width = 1920;
                Height = 1080;
                VideoBitrate = 5000;
                FrameRate = 30;
                break;
            case VideoQuality.Maximum:
                Width = 3840;
                Height = 2160;
                VideoBitrate = 15000;
                FrameRate = 60;
                break;
        }
    }

    /// <summary>Validate settings</summary>
    public bool IsValid()
    {
        return Width > 0 && Height > 0 && VideoBitrate > 0 && AudioBitrate > 0 && FrameRate > 0;
    }
}
