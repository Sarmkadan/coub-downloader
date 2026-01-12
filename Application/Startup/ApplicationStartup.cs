// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Infrastructure.BackgroundJobs;
using CoubDownloader.Infrastructure.Configuration;
using CoubDownloader.Infrastructure.Diagnostics;
using CoubDownloader.Infrastructure.Middleware;
using CoubDownloader.Infrastructure.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace CoubDownloader.Application.Startup;

/// <summary>Application startup and initialization logic</summary>
public class ApplicationStartup
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggingService _logger;
    private readonly ConfigurationManager _configManager;
    private List<PeriodicBackgroundWorker> _backgroundWorkers = [];

    public ApplicationStartup(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILoggingService>();
        _configManager = serviceProvider.GetRequiredService<ConfigurationManager>();
    }

    /// <summary>Initialize the application</summary>
    public async Task InitializeAsync()
    {
        _logger.LogInfo("Starting application initialization...", "Startup");

        try
        {
            // Load configuration
            var config = _configManager.Load();
            _logger.LogInfo("Configuration loaded successfully", "Startup");

            // Create directories
            EnsureDirectories(config);

            // Log application information
            LogApplicationInfo();

            // Perform diagnostics
            var diagnostics = _serviceProvider.GetRequiredService<DiagnosticsService>();
            var report = diagnostics.PerformHealthCheck();

            if (!report.IsHealthy)
            {
                _logger.LogWarning("Application health check detected issues");
                foreach (var warning in report.Warnings)
                {
                    _logger.LogWarning(warning);
                }
            }

            // Start background workers
            await StartBackgroundWorkersAsync();

            _logger.LogInfo("Application initialization completed", "Startup");
        }
        catch (Exception ex)
        {
            _logger.LogError("Application initialization failed", ex, "Startup");
            throw;
        }
    }

    /// <summary>Graceful shutdown of the application</summary>
    public async Task ShutdownAsync()
    {
        _logger.LogInfo("Starting graceful shutdown...", "Startup");

        try
        {
            // Stop background workers
            await StopBackgroundWorkersAsync();

            // Flush logs
            _logger.LogInfo("Application shutdown completed", "Startup");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during shutdown", ex, "Startup");
        }
    }

    private void EnsureDirectories(ApplicationConfiguration config)
    {
        var directories = new[]
        {
            config.Download.OutputDirectory,
            config.Logging.LogDirectory,
            "./temp",
            "./config"
        };

        foreach (var dir in directories)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    _logger.LogDebug($"Created directory: {dir}", "Startup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to create directory {dir}: {ex.Message}", "Startup");
            }
        }
    }

    private void LogApplicationInfo()
    {
        var info = VersionHelper.GetApplicationInfo();

        _logger.LogInfo("╔═══════════════════════════════════════════════╗", "Startup");
        _logger.LogInfo($"║ Coub Downloader v{info.AppVersion,-33} ║", "Startup");
        _logger.LogInfo($"║ Author: Vladyslav Zaiets                   ║", "Startup");
        _logger.LogInfo($"║ https://sarmkadan.com                       ║", "Startup");
        _logger.LogInfo("╚═══════════════════════════════════════════════╝", "Startup");
        _logger.LogInfo($"Runtime: {info.RuntimeVersion}", "Startup");
        _logger.LogInfo($"OS: {info.OperatingSystem}", "Startup");
        _logger.LogInfo($"Processors: {info.ProcessorCount}", "Startup");
        _logger.LogInfo($"Architecture: {(info.Is64BitProcess ? "x64" : "x86")}", "Startup");
    }

    private async Task StartBackgroundWorkersAsync()
    {
        try
        {
            var monitoringWorker = _serviceProvider.GetRequiredService<MonitoringWorker>();
            var cleanupWorker = _serviceProvider.GetRequiredService<CleanupWorker>();

            monitoringWorker.Start();
            cleanupWorker.Start();

            _backgroundWorkers.Add(monitoringWorker);
            _backgroundWorkers.Add(cleanupWorker);

            _logger.LogInfo($"Started {_backgroundWorkers.Count} background workers", "Startup");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to start background workers");
        }
    }

    private async Task StopBackgroundWorkersAsync()
    {
        var tasks = _backgroundWorkers.Select(w => w.StopAsync()).ToList();
        await Task.WhenAll(tasks);

        _logger.LogInfo($"Stopped {_backgroundWorkers.Count} background workers", "Startup");
    }
}

/// <summary>Startup configuration builder</summary>
public class StartupConfigurationBuilder
{
    private readonly StartupConfiguration _config = new();

    public StartupConfigurationBuilder WithLoggingDirectory(string directory)
    {
        _config.LoggingDirectory = directory;
        return this;
    }

    public StartupConfigurationBuilder WithDownloadDirectory(string directory)
    {
        _config.DownloadDirectory = directory;
        return this;
    }

    public StartupConfigurationBuilder WithConfigFile(string filePath)
    {
        _config.ConfigFilePath = filePath;
        return this;
    }

    public StartupConfigurationBuilder WithFFmpegPath(string path)
    {
        _config.FFmpegPath = path;
        return this;
    }

    public StartupConfigurationBuilder EnableVerboseLogging()
    {
        _config.VerboseLogging = true;
        return this;
    }

    public StartupConfiguration Build()
    {
        return _config;
    }
}

/// <summary>Startup configuration</summary>
public class StartupConfiguration
{
    public string LoggingDirectory { get; set; } = "./logs";
    public string DownloadDirectory { get; set; } = "./downloads";
    public string ConfigFilePath { get; set; } = "appsettings.json";
    public string FFmpegPath { get; set; } = "ffmpeg";
    public bool VerboseLogging { get; set; }
}
