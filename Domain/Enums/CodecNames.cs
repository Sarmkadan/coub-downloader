#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Enums;

/// <summary>
/// Well-known FFmpeg video codec identifiers usable wherever a codec name string is expected
/// (e.g. <see cref="CoubDownloader.Domain.Models.ConversionSettings.VideoCodec"/>).
/// </summary>
public static class VideoCodec
{
    /// <summary>H.264 / AVC video codec</summary>
    public const string H264 = "h264";

    /// <summary>H.265 / HEVC video codec</summary>
    public const string H265 = "h265";

    /// <summary>VP9 video codec</summary>
    public const string VP9 = "vp9";
}

/// <summary>
/// Well-known FFmpeg audio codec identifiers usable wherever a codec name string is expected
/// (e.g. <see cref="CoubDownloader.Domain.Models.ConversionSettings.AudioCodec"/>).
/// </summary>
public static class AudioCodec
{
    /// <summary>Advanced Audio Coding codec</summary>
    public const string AAC = "aac";

    /// <summary>MP3 audio codec</summary>
    public const string MP3 = "mp3";

    /// <summary>Opus audio codec</summary>
    public const string Opus = "opus";
}
