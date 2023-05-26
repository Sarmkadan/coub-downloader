## DownloadResultExtensions

`DownloadResultExtensions` provides helper methods to analyze and format `DownloadResult` instances, offering insights into download success status, file metadata, processing time, and error conditions.

### Usage Example

```csharp
using CoubDownloader.Domain.Models;

// Analyze a download result
var result = GetDownloadResult(); // Assume this retrieves a DownloadResult instance

if (result.IsSuccessfulWithFile())
{
    Console.WriteLine(result.GetFormattedFileInfo());
    Console.WriteLine($"Status: {result.GetStatusEmoji()}");
    Console.WriteLine($"Processing time: {result.FormatProcessingTime()}");
    Console.WriteLine($"File size valid: {result.IsFileSizeWithinBounds()}");
}
else
{
    Console.WriteLine($"Error: {result.HasCriticalError}");
    Console.WriteLine($"Warnings: {result.GetWarningsSummary()}");
    Console.WriteLine($"Processing time exceeded: {result.ExceededProcessingTime()}");
    
    // Create a copy of the result for logging
    var clonedResult = result.Clone();
}
```

// ... (rest of the README.md content remains the same)
