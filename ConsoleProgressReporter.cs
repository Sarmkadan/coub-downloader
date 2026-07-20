#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Models;

namespace CoubDownloader.Application.Services;

/// <summary>
/// Console progress reporter for batch processing
/// </summary>
public class ConsoleProgressReporter : IProgress<BatchProgress>
{
    private readonly string _batchName;
    private readonly object _lock = new object();

    public ConsoleProgressReporter(string batchName)
    {
        _batchName = batchName ?? "Batch";
    }

    public void Report(BatchProgress value)
    {
        if (value == null)
            return;

        lock (_lock)
        {
            Console.WriteLine($"[{_batchName}] {value.StatusMessage}");
        }
    }
}