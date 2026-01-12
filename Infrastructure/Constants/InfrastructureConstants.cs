// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Constants;

/// <summary>Infrastructure-level constants</summary>
public static class InfrastructureConstants
{
    // HTTP Client Configuration
    public const int HttpTimeoutSeconds = 30;
    public const int MaxRedirects = 5;
    public const int RetryAttempts = 3;
    public const int RetryDelayMs = 1000;

    // Rate Limiting
    public const int RateLimitRequestsPerWindow = 100;
    public const int RateLimitWindowSeconds = 60;
    public const int ThrottleTokensPerSecond = 10;
    public const int ThrottleMaxTokens = 20;

    // Cache Configuration
    public const int CacheDefaultTtlSeconds = 3600;
    public const int CacheMaxEntries = 10000;

    // Logging
    public const int LogMaxFileSizeMb = 10;
    public const int LogRetentionDays = 30;

    // File Operations
    public const int BufferSizeBytes = 65536; // 64KB
    public const int MaxFileSizeMb = 1024; // 1GB
    public const int DiskSpaceThresholdMb = 100; // Minimum free space

    // Performance
    public const int PerformanceMetricsHistorySize = 1000;
    public const int GarbageCollectionInterval = 3600; // 1 hour

    // Background Jobs
    public const int CleanupWorkerIntervalHours = 1;
    public const int MonitoringWorkerIntervalMinutes = 5;
    public const int HealthCheckTimeoutSeconds = 5;

    // Connection Pools
    public const int MaxConnectionPoolSize = 10;
    public const int ConnectionTimeoutSeconds = 30;
    public const int ConnectionIdleTimeoutSeconds = 60;

    // API Endpoints
    public const string CoubApiBaseUrl = "https://coub.com/api/v2";
    public const string CoubWebsiteUrl = "https://coub.com";

    // FFmpeg
    public const string DefaultFfmpegPath = "ffmpeg";
    public const int FfmpegTimeoutSeconds = 600;
    public const int FfmpegConcurrentOperations = 2;

    // Webhook
    public const int WebhookTimeoutSeconds = 10;
    public const int WebhookMaxRetries = 3;
    public const int WebhookFailureDisableThreshold = 10;

    // Validation
    public const int MinPasswordLength = 8;
    public const int MaxUrlLength = 2048;
    public const int MinVideoQuality = 480;
    public const int MaxVideoQuality = 4320;
}

/// <summary>Error codes for the application</summary>
public static class ErrorCodes
{
    // General Errors
    public const string INVALID_ARGUMENT = "ERR_INVALID_ARGUMENT";
    public const string FILE_NOT_FOUND = "ERR_FILE_NOT_FOUND";
    public const string OPERATION_TIMEOUT = "ERR_OPERATION_TIMEOUT";
    public const string DISK_SPACE_INSUFFICIENT = "ERR_DISK_SPACE_INSUFFICIENT";

    // Download Errors
    public const string DOWNLOAD_FAILED = "ERR_DOWNLOAD_FAILED";
    public const string DOWNLOAD_INVALID_URL = "ERR_DOWNLOAD_INVALID_URL";
    public const string DOWNLOAD_RATE_LIMITED = "ERR_DOWNLOAD_RATE_LIMITED";

    // Conversion Errors
    public const string CONVERSION_FAILED = "ERR_CONVERSION_FAILED";
    public const string FFMPEG_NOT_FOUND = "ERR_FFMPEG_NOT_FOUND";
    public const string UNSUPPORTED_FORMAT = "ERR_UNSUPPORTED_FORMAT";

    // API Errors
    public const string API_ERROR = "ERR_API_ERROR";
    public const string API_UNAUTHORIZED = "ERR_API_UNAUTHORIZED";
    public const string API_RATE_LIMITED = "ERR_API_RATE_LIMITED";
}

/// <summary>Magic numbers for quality presets</summary>
public static class QualityPresets
{
    // Low Quality Preset
    public const int LowWidth = 854;
    public const int LowHeight = 480;
    public const int LowVideoBitrate = 1000;
    public const int LowAudioBitrate = 64;

    // Medium Quality Preset
    public const int MediumWidth = 1280;
    public const int MediumHeight = 720;
    public const int MediumVideoBitrate = 2500;
    public const int MediumAudioBitrate = 96;

    // High Quality Preset
    public const int HighWidth = 1920;
    public const int HighHeight = 1080;
    public const int HighVideoBitrate = 5000;
    public const int HighAudioBitrate = 128;

    // Ultra Quality Preset
    public const int UltraWidth = 3840;
    public const int UltraHeight = 2160;
    public const int UltraVideoBitrate = 15000;
    public const int UltraAudioBitrate = 192;
}

/// <summary>Magic numbers for vertical formats</summary>
public static class VerticalFormatPresets
{
    // TikTok Shorts Format
    public const int ShortsWidth = 1080;
    public const int ShortsHeight = 1920;
    public const int ShortsFrameRate = 30;

    // Instagram Reels Format
    public const int ReelsWidth = 1080;
    public const int ReelsHeight = 1920;
    public const int ReelsFrameRate = 30;

    // YouTube Shorts Format
    public const int YouTubeShortsWidth = 1080;
    public const int YouTubeShortsHeight = 1920;
    public const int YouTubeShortsFrameRate = 30;
}

/// <summary>Magic numbers for batch processing</summary>
public static class BatchProcessingLimits
{
    public const int MaxTasksPerBatch = 1000;
    public const int MaxConcurrentTasks = 5;
    public const int TaskTimeoutSeconds = 300;
    public const int BatchRetentionDays = 30;
}
