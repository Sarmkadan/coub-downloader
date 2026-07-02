#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Exceptions;

/// <summary>
/// Exception thrown when external process execution fails (FFmpeg, FFprobe, etc.).
/// </summary>
public class ProcessExecutionException : CoubDownloaderException
{
    public string? ProcessName { get; set; }
    public string? Arguments { get; set; }
    public int ExitCode { get; set; }
    public string? StandardError { get; set; }

    public ProcessExecutionException(string message) : base(message) { }

    public ProcessExecutionException(string message, string processName, int exitCode) : base(message)
    {
        ProcessName = processName;
        ExitCode = exitCode;
    }

    public ProcessExecutionException(string message, string processName, string arguments, int exitCode, string standardError) : base(message)
    {
        ProcessName = processName;
        Arguments = arguments;
        ExitCode = exitCode;
        StandardError = standardError;
    }

    public ProcessExecutionException(string message, Exception inner) : base(message, inner) { }

    public override string ToString()
    {
        var baseString = base.ToString();
        var details = new List<string>();
        if (ProcessName != null) details.Add($"Process: {ProcessName}");
        if (Arguments != null) details.Add($"Args: {Arguments}");
        if (ExitCode != 0) details.Add($"Exit Code: {ExitCode}");
        if (StandardError != null) details.Add($"Error: {StandardError}");

        if (details.Count > 0)
        {
            return $"{baseString}\n{string.Join(" | ", details)}";
        }
        return baseString;
    }
}