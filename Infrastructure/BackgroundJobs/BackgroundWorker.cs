#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.BackgroundJobs;

/// <summary>Base class for background job workers</summary>
public abstract class BackgroundWorker
{
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _workerTask;
    protected bool IsRunning { get; private set; }

    /// <summary>Start the background worker</summary>
    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        IsRunning = true;

        _workerTask = Task.Run(async () =>
        {
            try
            {
                await ExecuteAsync(_cancellationTokenSource.Token);
            }
            finally
            {
                IsRunning = false;
            }
        });
    }

    /// <summary>Stop the background worker gracefully</summary>
    public async Task StopAsync()
    {
        if (!IsRunning)
        {
            return;
        }

        _cancellationTokenSource?.Cancel();

        if (_workerTask is not null)
        {
            try
            {
                await _workerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        IsRunning = false;
    }

    /// <summary>Execute the background work</summary>
    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
}

/// <summary>Periodic background worker that executes at regular intervals</summary>
public abstract class PeriodicBackgroundWorker : BackgroundWorker
{
    private readonly TimeSpan _interval;

    protected PeriodicBackgroundWorker(TimeSpan interval)
    {
        _interval = interval;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            try
            {
                await Task.Delay(_interval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>Perform the periodic work</summary>
    protected abstract Task DoWorkAsync(CancellationToken cancellationToken);

    /// <summary>Handle errors in background work</summary>
    protected virtual void OnError(Exception ex)
    {
        // Override to handle errors
    }
}

/// <summary>Cleanup worker that removes old files and expired data</summary>
public class CleanupWorker : PeriodicBackgroundWorker
{
    private readonly string _downloadDirectory;
    private readonly int _retentionDays;

    /// <summary>Create a cleanup worker that runs hourly</summary>
    /// <param name="downloadDirectory">Directory whose files are checked for expiry</param>
    /// <param name="retentionDays">Files older than this many days are deleted</param>
    public CleanupWorker(string downloadDirectory, int retentionDays = 30)
        : base(TimeSpan.FromHours(1))
    {
        _downloadDirectory = downloadDirectory;
        _retentionDays = retentionDays;
    }

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_downloadDirectory))
        {
            return;
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-_retentionDays);

        try
        {
            foreach (var file in Directory.GetFiles(_downloadDirectory))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc < cutoffDate)
                {
                    File.Delete(file);
                }
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            OnError(ex);
        }
    }

    protected override void OnError(Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Cleanup worker error: {ex.Message}");
    }
}

/// <summary>Monitoring worker for system health checks</summary>
public class MonitoringWorker : PeriodicBackgroundWorker
{
    /// <summary>Raised after each periodic health check with its result</summary>
    public event EventHandler<HealthCheckResult>? HealthCheckCompleted;

    /// <summary>Create a monitoring worker that runs every five minutes</summary>
    public MonitoringWorker() : base(TimeSpan.FromMinutes(5))
    {
    }

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        var result = PerformHealthCheck();
        HealthCheckCompleted?.Invoke(this, result);
        await Task.CompletedTask;
    }

    private HealthCheckResult PerformHealthCheck()
    {
        var result = new HealthCheckResult
        {
            Timestamp = DateTime.UtcNow,
            AvailableMemory = GC.GetTotalMemory(false),
            ProcessorCount = Environment.ProcessorCount,
            AvailableDiskSpace = GetAvailableDiskSpace()
        };

        return result;
    }

    private long GetAvailableDiskSpace()
    {
        try
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault();
            return drive?.AvailableFreeSpace ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    protected override void OnError(Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Monitoring worker error: {ex.Message}");
    }
}

/// <summary>Health check result data</summary>
public class HealthCheckResult
{
    /// <summary>UTC time at which the check was performed</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Managed heap size in bytes, as reported by <c>GC.GetTotalMemory(false)</c></summary>
    public long AvailableMemory { get; set; }

    /// <summary>Number of logical processors on the host</summary>
    public int ProcessorCount { get; set; }

    /// <summary>Free space in bytes on the first drive returned by <c>DriveInfo.GetDrives()</c></summary>
    public long AvailableDiskSpace { get; set; }

    /// <summary>True when at least 100 MB of disk space is free</summary>
    public bool IsHealthy => AvailableDiskSpace > 100 * 1024 * 1024; // At least 100MB free
}
