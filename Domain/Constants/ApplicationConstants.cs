// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Constants;

/// <summary>
/// Application-wide constants for the Coub Downloader.
/// </summary>
public static class ApplicationConstants
{
    /// <summary>Application name</summary>
    public const string ApplicationName = "Coub Downloader";

    /// <summary>Application version</summary>
    public const string Version = "1.0.0";

    /// <summary>Default user agent for HTTP requests</summary>
    public const string DefaultUserAgent = "CoubDownloader/1.0 (+https://sarmkadan.com)";

    /// <summary>Coub API base URL</summary>
    public const string CoubApiBaseUrl = "https://coub.com/api/v2";

    /// <summary>Default output directory name</summary>
    public const string DefaultOutputDirectory = "CoubDownloads";

    /// <summary>Temporary processing directory name</summary>
    public const string TempDirectory = "temp";

    /// <summary>Maximum video duration in seconds (10 minutes)</summary>
    public const double MaxVideoDuration = 600.0;

    /// <summary>Minimum video duration in seconds</summary>
    public const double MinVideoDuration = 0.5;

    /// <summary>Maximum file size for output in bytes (4GB)</summary>
    public const long MaxOutputFileSize = 4_294_967_296L;

    /// <summary>HTTP request timeout in milliseconds</summary>
    public const int HttpRequestTimeoutMs = 30000;

    /// <summary>Download chunk size in bytes (1MB)</summary>
    public const int DownloadChunkSize = 1048576;

    /// <summary>Maximum number of retry attempts</summary>
    public const int MaxRetryAttempts = 3;

    /// <summary>Delay between retry attempts in milliseconds</summary>
    public const int RetryDelayMs = 5000;

    /// <summary>FFmpeg executable name</summary>
    public const string FFmpegExecutable = "ffmpeg";

    /// <summary>FFprobe executable name (for metadata extraction)</summary>
    public const string FFprobeExecutable = "ffprobe";
}

/// <summary>
/// FFmpeg and video processing related constants.
/// </summary>
public static class VideoProcessingConstants
{
    /// <summary>Default frame rate for video output</summary>
    public const int DefaultFrameRate = 30;

    /// <summary>Default video bitrate in kbps</summary>
    public const int DefaultVideoBitrate = 5000;

    /// <summary>Default audio bitrate in kbps</summary>
    public const int DefaultAudioBitrate = 128;

    /// <summary>Default audio sample rate in Hz</summary>
    public const int DefaultSampleRate = 44100;

    /// <summary>Default audio channels (2 = stereo)</summary>
    public const int DefaultAudioChannels = 2;

    /// <summary>Video codec for MP4 format</summary>
    public const string H264Codec = "h264";

    /// <summary>Video codec for WebM format</summary>
    public const string VP9Codec = "vp9";

    /// <summary>Audio codec AAC</summary>
    public const string AACCodec = "aac";

    /// <summary>Audio codec libopus</summary>
    public const string OpusCodec = "libopus";

    /// <summary>MP4 file extension</summary>
    public const string MP4Extension = ".mp4";

    /// <summary>WebM file extension</summary>
    public const string WebMExtension = ".webm";

    /// <summary>WAV file extension (for audio)</summary>
    public const string WAVExtension = ".wav";

    /// <summary>AAC file extension (for audio)</summary>
    public const string AACExtension = ".aac";

    /// <summary>Default preset for FFmpeg encoding (quality vs speed trade-off)</summary>
    public const string FFmpegPreset = "medium";

    /// <summary>CRF value for quality control (0-51, lower = better)</summary>
    public const int FFmpegCRF = 23;

    /// <summary>Fade effect duration in milliseconds</summary>
    public const int DefaultFadeDurationMs = 500;

    /// <summary>Transition effect duration in milliseconds</summary>
    public const int DefaultTransitionDurationMs = 300;
}

/// <summary>
/// Mobile shorts and vertical video format constants.
/// </summary>
public static class MobileShortsConstants
{
    /// <summary>TikTok optimal width</summary>
    public const int TikTokWidth = 1080;

    /// <summary>TikTok optimal height</summary>
    public const int TikTokHeight = 1920;

    /// <summary>Instagram Reels optimal width</summary>
    public const int InstagramReelsWidth = 1080;

    /// <summary>Instagram Reels optimal height</summary>
    public const int InstagramReelsHeight = 1920;

    /// <summary>YouTube Shorts optimal width</summary>
    public const int YouTubeShortsWidth = 1080;

    /// <summary>YouTube Shorts optimal height</summary>
    public const int YouTubeShortsHeight = 1920;

    /// <summary>Minimum duration for shorts in seconds</summary>
    public const double MinShortsDuration = 15.0;

    /// <summary>Maximum duration for shorts in seconds</summary>
    public const double MaxShortsDistance = 60.0;

    /// <summary>Optimal aspect ratio for vertical videos</summary>
    public const decimal OptimalAspectRatio = 9m / 16m;
}

/// <summary>
/// Database and file system related constants.
/// </summary>
public static class FileSystemConstants
{
    /// <summary>Configuration file name</summary>
    public const string ConfigFileName = "settings.json";

    /// <summary>Database file name (if using SQLite)</summary>
    public const string DatabaseFileName = "coubdownloader.db";

    /// <summary>Log file directory name</summary>
    public const string LogsDirectory = "logs";

    /// <summary>Cache directory name</summary>
    public const string CacheDirectory = ".cache";

    /// <summary>Maximum file path length</summary>
    public const int MaxFilePathLength = 260;

    /// <summary>Maximum file name length</summary>
    public const int MaxFileNameLength = 255;
}
