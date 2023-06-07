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

## DomainEvent

`DomainEvent` is the base class for all domain events in the Coub Downloader application. It provides a standardized way to represent domain-specific occurrences with unique identifiers and timestamps. All domain events inherit from this class and can include additional properties relevant to the specific event type.

### Key Features
- Automatic unique ID generation using GUID
- Timestamp recording at event creation time (UTC)
- Type-safe event handling through generic interfaces
- Supports in-process event bus for decoupled communication

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Events;
using CoubDownloader.Application.Services;

// Define a custom event by inheriting from DomainEvent
public class VideoProcessingCompletedEvent : DomainEvent
{
    public string VideoId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int QualityLevel { get; set; }
}

// Define an event handler
public class VideoProcessingCompletedEventHandler : IEventHandler<VideoProcessingCompletedEvent>
{
    public async Task HandleAsync(VideoProcessingCompletedEvent @event)
    {
        Console.WriteLine($"Processing completed for video {@event.VideoId} at {@event.OccurredAt:O}");
        Console.WriteLine($"Status: {@event.Status}, Quality: {@event.QualityLevel}");
        
        // Additional processing logic here
        await Task.CompletedTask;
    }
}

// Create and publish an event
var eventBus = new InProcessEventBus();
var handler = new VideoProcessingCompletedEventHandler();

// Subscribe the handler to the event type
eventBus.Subscribe(handler);

// Create and publish a domain event
var processingEvent = new VideoProcessingCompletedEvent
{
    VideoId = "abc123",
    Status = "Completed",
    QualityLevel = 1080
};

Console.WriteLine($"Event ID: {processingEvent.Id}");
Console.WriteLine($"Occurred At: {processingEvent.OccurredAt:O}");

// Publish the event asynchronously
await eventBus.PublishAsync(processingEvent);

// Unsubscribe when no longer needed
eventBus.Unsubscribe<VideoProcessingCompletedEvent>(handler);
```

## ICredentialManager

`ICredentialManager` provides a secure interface for storing, retrieving, validating, and deleting API keys and credentials for external services. It supports both in-memory storage for development and encrypted file-based storage for production environments, allowing safe credential management across different deployment scenarios.

### Usage Example

```csharp
using CoubDownloader.Infrastructure.Security;

// Create a credential manager (in-memory for development)
var credentialManager = new InMemoryCredentialManager();

// Store an API key for a service
credentialManager.StoreApiKey("coub-api", "your-api-key-here");

// Retrieve an API key
var apiKey = credentialManager.GetApiKey("coub-api");
Console.WriteLine($"Retrieved API key: {apiKey}");

// Validate an API key
var isValid = credentialManager.ValidateApiKey("coub-api", "your-api-key-here");
Console.WriteLine($"Key validation result: {isValid}");

// Delete an API key
credentialManager.DeleteApiKey("coub-api");

// Create an encrypted credential manager for production
var encryptedCredentialManager = new EncryptedCredentialManager(
    storePath: "/var/lib/coub-downloader/credentials.enc",
    encryptionKey: Environment.GetEnvironmentVariable("ENCRYPTION_KEY")
);

// Use the encrypted manager the same way
encryptedCredentialManager.StoreApiKey("coub-api", "production-api-key-here");
``` 
