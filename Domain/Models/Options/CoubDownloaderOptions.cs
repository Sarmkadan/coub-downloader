using System.ComponentModel.DataAnnotations;

namespace CoubDownloader.Domain.Models.Options;

public class DownloadOptions
{
    [Required]
    public string OutputPath { get; set; } = "./downloads";
    
    public string CachePath { get; set; } = "./cache";
    
    [Range(0, 100)]
    public int MaxRetries { get; set; } = 3;
    
    [Range(1, 3600)]
    public int TimeoutSeconds { get; set; } = 300;
    
    public bool EnableCaching { get; set; } = true;
    
    [Range(0.1, 100.0)]
    public double MaxCacheSizeGb { get; set; } = 1.0;
    
    [Range(1, 50)]
    public int ParallelDownloads { get; set; } = 4;
}

public class ConversionOptions
{
    [Required]
    public string DefaultQuality { get; set; } = "High";
    
    [Required]
    public string DefaultFormat { get; set; } = "MP4";
    
    [Range(1, 120)]
    public int DefaultFrameRate { get; set; } = 30;
    
    public bool EnableHardwareAcceleration { get; set; } = false;
    
    [Required]
    public string FFmpegPath { get; set; } = "ffmpeg";
    
    [Required]
    public string FFprobePath { get; set; } = "ffprobe";
    
    [Range(1, 64)]
    public int ThreadCount { get; set; } = 4;
}

public class AudioOptions
{
    [Required]
    public string DefaultLoopStrategy { get; set; } = "Repeat";
    
    [Range(8000, 192000)]
    public int DefaultSampleRate { get; set; } = 44100;
    
    [Range(1, 8)]
    public int DefaultChannels { get; set; } = 2;
    
    [Range(32, 320)]
    public int DefaultBitrate { get; set; } = 128;
}

public class ApiOptions
{
    [Required, Url]
    public string CoubBaseUrl { get; set; } = "https://coub.com";
    
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;
    
    [Range(1, 1000)]
    public int RateLimitPerMinute { get; set; } = 60;
    
    [Required]
    public string RetryPolicy { get; set; } = "exponential";
    
    public bool VerifySSL { get; set; } = true;
}

public class LoggingOptions
{
    public Dictionary<string, string> LogLevel { get; set; } = new() 
    { 
        { "Default", "Information" }, 
        { "Microsoft", "Warning" }, 
        { "System", "Warning" } 
    };
    
    public FileLoggingOptions File { get; set; } = new();
}

public class FileLoggingOptions
{
    public bool Enabled { get; set; } = false;
    
    [Required]
    public string Path { get; set; } = "./logs/app.log";
    
    [Range(1024, 1073741824)]
    public int MaxFileSizeBytes { get; set; } = 10485760;
    
    [Range(1, 100)]
    public int MaxBackupFiles { get; set; } = 3;
}

public class CoubDownloaderOptions
{
    public DownloadOptions Download { get; set; } = new();
    public ConversionOptions Conversion { get; set; } = new();
    public AudioOptions Audio { get; set; } = new();
    public ApiOptions Api { get; set; } = new();
    public LoggingOptions Logging { get; set; } = new();
}
