// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Middleware;

/// <summary>Structured logging service with severity levels and timestamps</summary>
public interface ILoggingService
{
    void LogInfo(string message, string? category = null);
    void LogWarning(string message, string? category = null);
    void LogError(string message, Exception? ex = null, string? category = null);
    void LogDebug(string message, string? category = null);
}

/// <summary>File-based logging implementation</summary>
public class FileLoggingService : ILoggingService
{
    private readonly string _logPath;
    private readonly object _lockObj = new();

    public FileLoggingService(string logDirectory = "./logs")
    {
        if (!Directory.Exists(logDirectory))
            Directory.CreateDirectory(logDirectory);

        _logPath = Path.Combine(logDirectory, $"coub-downloader-{DateTime.Now:yyyy-MM-dd}.log");
    }

    public void LogInfo(string message, string? category = null)
    {
        WriteLog("INFO", message, category, null);
    }

    public void LogWarning(string message, string? category = null)
    {
        WriteLog("WARN", message, category, null);
    }

    public void LogError(string message, Exception? ex = null, string? category = null)
    {
        var details = ex?.ToString() ?? "";
        WriteLog("ERROR", message, category, details);
    }

    public void LogDebug(string message, string? category = null)
    {
        #if DEBUG
        WriteLog("DEBUG", message, category, null);
        #endif
    }

    private void WriteLog(string level, string message, string? category, string? details)
    {
        lock (_lockObj)
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var categoryStr = string.IsNullOrEmpty(category) ? "" : $"[{category}] ";
                var logEntry = $"{timestamp} | {level,-5} | {categoryStr}{message}";

                if (!string.IsNullOrEmpty(details))
                    logEntry += $"\n{details}";

                File.AppendAllText(_logPath, logEntry + Environment.NewLine);

                // Also write to console for immediate visibility
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = level switch
                {
                    "ERROR" => ConsoleColor.Red,
                    "WARN" => ConsoleColor.Yellow,
                    "INFO" => ConsoleColor.Green,
                    _ => ConsoleColor.Gray
                };

                Console.WriteLine(logEntry);
                Console.ForegroundColor = originalColor;
            }
            catch
            {
                // Silently fail if logging fails
            }
        }
    }
}

/// <summary>In-memory logging for testing and debugging</summary>
public class MemoryLoggingService : ILoggingService
{
    private readonly List<LogEntry> _logs = [];
    private readonly object _lockObj = new();

    public IReadOnlyList<LogEntry> GetLogs() => _logs.AsReadOnly();

    public void LogInfo(string message, string? category = null)
    {
        AddLog("INFO", message, category);
    }

    public void LogWarning(string message, string? category = null)
    {
        AddLog("WARN", message, category);
    }

    public void LogError(string message, Exception? ex = null, string? category = null)
    {
        var msg = ex == null ? message : $"{message}: {ex.Message}";
        AddLog("ERROR", msg, category);
    }

    public void LogDebug(string message, string? category = null)
    {
        AddLog("DEBUG", message, category);
    }

    private void AddLog(string level, string message, string? category)
    {
        lock (_lockObj)
        {
            _logs.Add(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Category = category ?? "General",
                Message = message
            });
        }
    }
}

/// <summary>Represents a single log entry</summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "";
    public string Category { get; set; } = "";
    public string Message { get; set; } = "";

    public override string ToString()
    {
        return $"{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level,-5} | [{Category}] {Message}";
    }
}
