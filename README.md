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

## AudioTrackExtensions

`AudioTrackExtensions` provides helper methods to analyze and format audio track instances, offering insights into audio track duration, fade effects, format, and estimated file size. It also provides methods to check if an audio track needs volume normalization and its age. 

### Usage Example

```csharp
using CoubDownloader.Domain.Models;

// Analyze an audio track
var audioTrack = GetAudioTrack(); // Assume this retrieves an audio track instance

Console.WriteLine($"Total duration: {audioTrack.GetTotalDuration()}");
Console.WriteLine($"Single loop duration: {audioTrack.GetSingleLoopDuration()}");
Console.WriteLine($"Fade in ratio: {audioTrack.GetFadeInRatio()}");
Console.WriteLine($"Fade out ratio: {audioTrack.GetFadeOutRatio()}");
Console.WriteLine($"Has fade effects: {audioTrack.HasFadeEffects()}");
Console.WriteLine($"Audio format: {audioTrack.GetAudioFormat()}");
Console.WriteLine($"Estimated file size (MB): {audioTrack.GetEstimatedFileSizeMb()}");
Console.WriteLine($"Needs volume normalization: {audioTrack.NeedsVolumeNormalization()}");
Console.WriteLine($"Age in days: {audioTrack.GetAgeInDays()}");
Console.WriteLine($"Is recently created: {audioTrack.IsRecentlyCreated()}");
```

// ... (rest of the README.md content remains the same)
