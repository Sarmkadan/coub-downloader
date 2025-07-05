// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace CoubDownloader.Infrastructure.Configuration;

/// <summary>Application configuration management</summary>
public class ApplicationConfiguration
{
    public DownloadSettings Download { get; set; } = new();
    public ConversionSettings Conversion { get; set; } = new();
    public CacheSettings Cache { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
    public ApiSettings Api { get; set; } = new();
}

/// <summary>Download-related settings</summary>
public class DownloadSettings
{
    public string OutputDirectory { get; set; } = "./downloads";
    public int MaxConcurrentDownloads { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 300;
    public int MaxRetries { get; set; } = 3;
    public bool VerifyFileIntegrity { get; set; } = true;
    public long MaxFileSizeBytes { get; set; } = 1024L * 1024L * 1024L; // 1GB
}

/// <summary>Video conversion settings</summary>
public class ConversionSettings
{
    public string FfmpegPath { get; set; } = "ffmpeg";
    public bool EnableHardwareAcceleration { get; set; } = true;
    public int MaxConcurrentConversions { get; set; } = 2;
    public int TimeoutSeconds { get; set; } = 600;
    public string VideoCodec { get; set; } = "libx264";
    public string AudioCodec { get; set; } = "aac";
    public int DefaultQuality { get; set; } = 80;
}

/// <summary>Caching settings</summary>
public class CacheSettings
{
    public bool Enabled { get; set; } = true;
    public int DefaultTtlSeconds { get; set; } = 3600;
    public int MaxEntriesCount { get; set; } = 10000;
    public string CacheType { get; set; } = "memory"; // "memory", "redis"
}

/// <summary>Logging settings</summary>
public class LoggingSettings
{
    public string LogLevel { get; set; } = "Info"; // "Debug", "Info", "Warning", "Error"
    public string LogDirectory { get; set; } = "./logs";
    public int MaxLogFileSizeMb { get; set; } = 10;
    public int RetentionDays { get; set; } = 30;
    public bool EnableConsoleOutput { get; set; } = true;
    public bool EnableFileOutput { get; set; } = true;
}

/// <summary>API-related settings</summary>
public class ApiSettings
{
    public int RateLimitPerWindow { get; set; } = 100;
    public int RateLimitWindowSeconds { get; set; } = 60;
    public int HttpTimeoutSeconds { get; set; } = 30;
    public List<string> AllowedOrigins { get; set; } = [];
}

/// <summary>Configuration loader and manager</summary>
public class ConfigurationManager
{
    private ApplicationConfiguration? _configuration;
    private readonly string _configPath;
    private readonly object _lockObj = new();

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
                if (directory != null && !Directory.Exists(directory))
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
        if (_configuration != null)
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
            if (current == null) return null;

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

        if (current != null)
        {
            var property = current.GetType().GetProperty(lastPart);
            if (property != null && property.CanWrite)
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
