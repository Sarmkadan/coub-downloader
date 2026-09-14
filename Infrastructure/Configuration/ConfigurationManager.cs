#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace CoubDownloader.Infrastructure.Configuration;

/// <summary>Application configuration management</summary>
public class ApplicationConfiguration
{
    /// <summary>Download-related settings</summary>
    public DownloadSettings Download { get; set; } = new();
    /// <summary>Video conversion settings</summary>
    public ConversionSettings Conversion { get; set; } = new();
    /// <summary>Caching settings</summary>
    public CacheSettings Cache { get; set; } = new();
    /// <summary>Logging settings</summary>
    public LoggingSettings Logging { get; set; } = new();
    /// <summary>API-related settings</summary>
    public ApiSettings Api { get; set; } = new();
}

/// <summary>Download-related settings</summary>
public class DownloadSettings
{
    /// <summary>Directory where downloaded files are stored</summary>
    public string OutputDirectory { get; set; } = "./downloads";
    /// <summary>Maximum number of simultaneous downloads</summary>
    public int MaxConcurrentDownloads { get; set; } = 3;
    /// <summary>Download timeout in seconds</summary>
    public int TimeoutSeconds { get; set; } = 300;
    /// <summary>Maximum number of download retry attempts</summary>
    public int MaxRetries { get; set; } = 3;
    /// <summary>Whether to verify file integrity after download</summary>
    public bool VerifyFileIntegrity { get; set; } = true;
    /// <summary>Maximum allowed file size in bytes</summary>
    public long MaxFileSizeBytes { get; set; } = 1024L * 1024L * 1024L; // 1GB
}

/// <summary>Video conversion settings</summary>
public class ConversionSettings
{
    /// <summary>Path to the FFmpeg executable</summary>
    public string FfmpegPath { get; set; } = "ffmpeg";
    /// <summary>Whether to enable hardware-accelerated encoding</summary>
    public bool EnableHardwareAcceleration { get; set; } = true;
    /// <summary>Maximum number of simultaneous conversions</summary>
    public int MaxConcurrentConversions { get; set; } = 2;
    /// <summary>Conversion timeout in seconds</summary>
    public int TimeoutSeconds { get; set; } = 600;
    /// <summary>Video codec used for conversion</summary>
    public string VideoCodec { get; set; } = "libx264";
    /// <summary>Audio codec used for conversion</summary>
    public string AudioCodec { get; set; } = "aac";
    /// <summary>Default output quality (0-100)</summary>
    public int DefaultQuality { get; set; } = 80;
}

/// <summary>Caching settings</summary>
public class CacheSettings
{
    /// <summary>Whether caching is enabled</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>Default cache entry time-to-live in seconds</summary>
    public int DefaultTtlSeconds { get; set; } = 3600;
    /// <summary>Maximum number of cache entries</summary>
    public int MaxEntriesCount { get; set; } = 10000;
    /// <summary>Cache backend type ("memory", "redis")</summary>
    public string CacheType { get; set; } = "memory"; // "memory", "redis"
}

/// <summary>Logging settings</summary>
public class LoggingSettings
{
    /// <summary>Logging level ("Debug", "Info", "Warning", "Error")</summary>
    public string LogLevel { get; set; } = "Info"; // "Debug", "Info", "Warning", "Error"
    /// <summary>Directory where log files are stored</summary>
    public string LogDirectory { get; set; } = "./logs";
    /// <summary>Maximum log file size in MB</summary>
    public int MaxLogFileSizeMb { get; set; } = 10;
    /// <summary>Number of days to retain log files</summary>
    public int RetentionDays { get; set; } = 30;
    /// <summary>Whether to output logs to console</summary>
    public bool EnableConsoleOutput { get; set; } = true;
    /// <summary>Whether to output logs to file</summary>
    public bool EnableFileOutput { get; set; } = true;
}

/// <summary>API-related settings</summary>
public class ApiSettings
{
    /// <summary>Maximum number of requests allowed per rate-limit window</summary>
    public int RateLimitPerWindow { get; set; } = 100;
    /// <summary>Rate-limit window duration in seconds</summary>
    public int RateLimitWindowSeconds { get; set; } = 60;
    /// <summary>HTTP request timeout in seconds</summary>
    public int HttpTimeoutSeconds { get; set; } = 30;
    /// <summary>Allowed CORS origins</summary>
    public List<string> AllowedOrigins { get; set; } = [];
}

/// <summary>Configuration loader and manager</summary>
public class ConfigurationManager
{
    private ApplicationConfiguration? _configuration;
    private readonly string _configPath;
    private readonly object _lockObj = new();

    /// <summary>Initializes a new instance of the ConfigurationManager class</summary>
    /// <param name="configPath">Path to the configuration file (default: "appsettings.json")</param>
    public ConfigurationManager(string configPath = "appsettings.json")
    {
        _configPath = configPath;
    }

    /// <summary>Load configuration from file</summary>
    public ApplicationConfiguration Load()
    {
        lock (_lockObj)
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    _configuration = CreateDefaultConfiguration();
                    SaveDefault();
                    return _configuration;
                }

                var json = File.ReadAllText(_configPath);
                _configuration = JsonSerializer.Deserialize<ApplicationConfiguration>(json)
                    ?? CreateDefaultConfiguration();

                return _configuration;
            }
            catch
            {
                _configuration = CreateDefaultConfiguration();
                return _configuration;
            }
        }
    }

    /// <summary>Get current configuration (lazy load)</summary>
    public ApplicationConfiguration GetConfiguration()
    {
        return _configuration ?? Load();
    }

    /// <summary>Save configuration to file</summary>
    public void Save(ApplicationConfiguration config)
    {
        lock (_lockObj)
        {
            try
            {
                var directory = Path.GetDirectoryName(_configPath);
                if (directory is not null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(_configPath, json);

                _configuration = config;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
            }
        }
    }

    /// <summary>Save default configuration</summary>
    private void SaveDefault()
    {
        if (_configuration is not null)
            Save(_configuration);
    }

    /// <summary>Create default configuration</summary>
    private ApplicationConfiguration CreateDefaultConfiguration()
    {
        return new ApplicationConfiguration
        {
            Download = new DownloadSettings(),
            Conversion = new ConversionSettings(),
            Cache = new CacheSettings(),
            Logging = new LoggingSettings(),
            Api = new ApiSettings()
        };
    }

    /// <summary>Get setting value by path (e.g., "Download.OutputDirectory")</summary>
    public string? GetSetting(string path)
    {
        var config = GetConfiguration();
        var parts = path.Split('.');

        object? current = config;

        foreach (var part in parts)
        {
            if (current is null) return null;

            var property = current.GetType().GetProperty(part);
            current = property?.GetValue(current);
        }

        return current?.ToString();
    }

    /// <summary>Set setting value by path</summary>
    public void SetSetting(string path, string value)
    {
        var config = GetConfiguration();
        var parts = path.Split('.');
        var lastPart = parts.Last();

        object? current = config;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            var property = current!.GetType().GetProperty(parts[i]);
            current = property?.GetValue(current);
        }

        if (current is not null)
        {
            var property = current.GetType().GetProperty(lastPart);
            if (property is not null && property.CanWrite)
            {
                try
                {
                    var typedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(current, typedValue);
                    Save(config);
                }
                catch
                {
                    // Failed to convert value
                }
            }
        }
    }
}
