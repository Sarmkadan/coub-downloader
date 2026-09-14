#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Middleware;

/// <summary>Structured logging service with severity levels and timestamps</summary>
public interface ILoggingService
{
    /// <summary>Logs an informational message</summary>
    /// <param name="message">The message to log</param>
    /// <param name="category">Optional category label for the entry</param>
    void LogInfo(string message, string? category = null);

    /// <summary>Logs a warning message</summary>
    /// <param name="message">The message to log</param>
    /// <param name="category">Optional category label for the entry</param>
    void LogWarning(string message, string? category = null);

    /// <summary>Logs an error message, optionally with exception details</summary>
    /// <param name="message">The message to log</param>
    /// <param name="ex">Optional exception whose details are appended</param>
    /// <param name="category">Optional category label for the entry</param>
    void LogError(string message, Exception? ex = null, string? category = null);

    /// <summary>Logs a debug message</summary>
    /// <param name="message">The message to log</param>
    /// <param name="category">Optional category label for the entry</param>
    void LogDebug(string message, string? category = null);
}

/// <summary>File-based logging implementation</summary>
public class FileLoggingService : ILoggingService
{
    private readonly string _logPath;
    private readonly object _lockObj = new();

    /// <summary>Initializes a new instance writing to a dated log file in the given directory</summary>
    /// <param name="logDirectory">Directory in which the log file is created</param>
    public FileLoggingService(string logDirectory = "./logs")
    {
        if (!Directory.Exists(logDirectory))
            Directory.CreateDirectory(logDirectory);

        _logPath = Path.Combine(logDirectory, $"coub-downloader-{DateTime.Now:yyyy-MM-dd}.log");
    }

    /// <summary>Writes an informational entry to the log file and console</summary>
    /// <param name="message">The message to log</param>
    /// <param name="category">Optional category label for the entry</param>
    public void LogInfo(string message, string? category = null)
    {
        WriteLog("INFO", message, category, null);
    }

    /// <summary>Writes a warning entry to the log file and console</summary>
    /// <param name="message">The message to log</param>
    /// <param name="category">Optional category label for the entry</param>
    public void LogWarning(string message, string? category = null)
    {
        WriteLog("WARN", message, category, null);
    }

    /// <summary>Writes an error entry to the log file and console, appending exception details</summary>
    /// <param name="message">The message to log</param>
    /// <param name="ex">Optional exception whose details are appended</param>
    /// <param name="category">Optional category label for the entry</param>
    public void LogError(string message, Exception? ex = null, string? category = null)
    {
        var details = ex?.ToString() ?? "";
        WriteLog("ERROR", message, category, details);
    }

    /// <summary>Writes a debug entry to the log file and console when compiled in DEBUG</summary>
    /// <param name="message">The message to log</param>
    /// <param name="category">Optional category label for the entry</param>
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

    /// <summary>Returns a read-only snapshot of all recorded log entries</summary>
    public IReadOnlyList<LogEntry> GetLogs() => _logs.AsReadOnly();

    /// <summary>Records an informational entry in memory</summary>
    /// <param name="message">The message to log</param>
    /// <param name="category">Optional category label for the entry</param>
    public void LogInfo(string message, string? category = null)
    {
        AddLog("INFO", message, category);
    }

    /// <summary>Records a warning entry in memory</summary>
    /// <param name="message">The message to log</param>
    /// <param name="category">Optional category label for the entry</param>
    public void LogWarning(string message, string? category = null)
    {
        AddLog("WARN", message, category);
    }

    /// <summary>Records an error entry in memory, appending the exception message</summary>
    /// <param name="message">The message to log</param>
    /// <param name="ex">Optional exception whose message is appended</param>
    /// <param name="category">Optional category label for the entry</param>
    public void LogError(string message, Exception? ex = null, string? category = null)
    {
        var msg = ex is null ? message : $"{message}: {ex.Message}";
        AddLog("ERROR", msg, category);
    }

    /// <summary>Records a debug entry in memory</summary>
    /// <param name="message">The message to log</param>
    /// <param name="category">Optional category label for the entry</param>
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
    /// <summary>UTC timestamp of when the entry was recorded</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Severity level of the entry (INFO, WARN, ERROR, DEBUG)</summary>
    public string Level { get; set; } = "";

    /// <summary>Category label for the entry, or "General" when none was supplied</summary>
    public string Category { get; set; } = "";

    /// <summary>The logged message text</summary>
    public string Message { get; set; } = "";

    /// <summary>Formats the entry as a single-line, console-friendly string</summary>
    public override string ToString()
    {
        return $"{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level,-5} | [{Category}] {Message}";
    }
}
