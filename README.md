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

## VideoEditorServiceExtensions

`VideoEditorServiceExtensions` provides helper methods to edit and manipulate video instances, offering insights into trimming, rendering, and applying effects. It also provides methods to get the edit history and create a new video edit session.

### Usage Example

```csharp
using CoubDownloader.Application.Services;

// Trim the first 5 seconds of a video
var video = GetVideo(); // Assume this retrieves a video instance
var trimmedVideo = await VideoEditorServiceExtensions.TrimFirstSecondsAsync(video, 5);

// Trim and render a video
var trimmedAndRenderedVideo = await VideoEditorServiceExtensions.TrimAndRenderAsync(video, 5, 10);

// Generate a standard preview for a video
var preview = await VideoEditorServiceExtensions.GenerateStandardPreviewAsync(video);

// Apply effects to a video
var editedVideo = await VideoEditorServiceExtensions.ApplyEffectsAsync(video);

// Get the edit history of a video
var editHistory = VideoEditorServiceExtensions.GetEditHistory(video);

// Create a new video edit session
var editSession = VideoEditorServiceExtensions.WithOperations(video);
```

## EventHandlingExampleExtensions

`EventHandlingExampleExtensions` provides helper methods to analyze and format event handling results. It offers insights into progress status, output filename, error conditions, duration, file size, and retry status.

### Usage Example

```csharp
using CoubDownloader.Examples;

// Analyze an event handling result
var eventHandlingResult = GetEventHandlingResult(); // Assume this retrieves an event handling result instance

Console.WriteLine($"Progress status: {EventHandlingExampleExtensions.GetProgressStatus(eventHandlingResult)}");
Console.WriteLine($"Output filename: {EventHandlingExampleExtensions.GetOutputFilename(eventHandlingResult)}");
Console.WriteLine($"Has error: {EventHandlingExampleExtensions.HasError(eventHandlingResult)}");
Console.WriteLine($"Formatted duration: {EventHandlingExampleExtensions.GetFormattedDuration(eventHandlingResult)}");
Console.WriteLine($"Formatted file size: {EventHandlingExampleExtensions.GetFormattedFileSize(eventHandlingResult)}");
Console.WriteLine($"Retry status: {EventHandlingExampleExtensions.GetRetryStatus(eventHandlingResult)}");
``` 
