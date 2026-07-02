#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Exceptions;

/// <summary>
/// Exception thrown when file system operations fail (read, write, delete, etc.).
/// </summary>
public class FileOperationException : CoubDownloaderException
{
    public string? FilePath { get; set; }
    public FileOperationType OperationType { get; set; }

    public FileOperationException(string message) : base(message) { }

    public FileOperationException(string message, string filePath, FileOperationType operationType) : base(message)
    {
        FilePath = filePath;
        OperationType = operationType;
    }

    public FileOperationException(string message, Exception inner) : base(message, inner) { }

    public FileOperationException(string message, string filePath, FileOperationType operationType, Exception inner) : base(message, inner)
    {
        FilePath = filePath;
        OperationType = operationType;
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        var details = new List<string>();
        if (FilePath != null) details.Add($"File: {FilePath}");
        if (OperationType != FileOperationType.Unknown) details.Add($"Operation: {OperationType}");

        if (details.Count > 0)
        {
            return $"{baseString}\n{string.Join(" | ", details)}";
        }
        return baseString;
    }
}

/// <summary>
/// Type of file operation that failed.
/// </summary>
public enum FileOperationType
{
    Unknown,
    Read,
    Write,
    Delete,
    CreateDirectory,
    ExistsCheck
}