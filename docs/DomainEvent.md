# DomainEvent

Base class for domain events in the coub-downloader application. Provides common infrastructure for event identification, timestamping, and pub/sub functionality while serving as a foundation for specific event types related to video downloading and processing operations.

## API

### Properties

#### `Id`
Gets the unique identifier for this domain event instance.
- **Type**: `string`
- **Purpose**: Uniquely identifies the event within the system
- **Thread Safety**: Safe for concurrent read access

#### `OccurredAt`
Gets the timestamp when this domain event was created.
- **Type**: `DateTime`
- **Purpose**: Records when the event occurred in the system timeline
- **Thread Safety**: Safe for concurrent read access

#### `VideoId`
Gets the identifier of the video associated with this event.
- **Type**: `string`
- **Purpose**: Links the event to a specific video being processed
- **Thread Safety**: Safe for concurrent read access

#### `Url`
Gets the source URL of the video.
- **Type**: `string`
- **Purpose**: Specifies the original location of the video content
- **Thread Safety**: Safe for concurrent read access

#### `FilePath`
Gets the local file path where the video is stored.
- **Type**: `string`
- **Purpose**: Indicates the storage location of downloaded video content
- **Thread Safety**: Safe for concurrent read access

#### `FileSize`
Gets the size of the video file in bytes.
- **Type**: `long`
- **Purpose**: Provides information about the downloaded file's size
- **Thread Safety**: Safe for concurrent read access

#### `Error`
Gets the error message if the event represents a failure condition.
- **Type**: `string`
- **Purpose**: Contains error details when video processing fails
- **Thread Safety**: Safe for concurrent read access

#### `InputFile`
Gets the path to the input file for processing operations.
- **Type**: `string`
- **Purpose**: Specifies the source file for transformation operations
- **Thread Safety**: Safe for concurrent read access

#### `OutputFile`
Gets the path to the output file from processing operations.
- **Type**: `string`
- **Purpose**: Specifies the destination file for transformed content
- **Thread Safety**: Safe for concurrent read access

#### `Duration`
Gets the duration of the video content.
- **Type**: `TimeSpan`
- **Purpose**: Provides timing information about video length
- **Thread Safety**: Safe for concurrent read access

#### `BatchId`
Gets the identifier for batch processing operations.
- **Type**: `string`
- **Purpose**: Groups related events within batch operations
- **Thread Safety**: Safe for concurrent read access

#### `Name`
Gets the name or title of the video content.
- **Type**: `string`
- **Purpose**: Provides human-readable identification of the video
- **Thread Safety**: Safe for concurrent read access

### Methods

#### `Subscribe<TEvent>`
Registers a handler for the specified event type.
- **Generic Type**: `TEvent` - The type of event to subscribe to
- **Parameters**: None (handler registration mechanism not specified in signature)
- **Return Value**: `void`
- **Exceptions**: May throw if subscription fails due to invalid handler configuration
- **Thread Safety**: Not thread-safe; external synchronization required during subscription changes

#### `PublishAsync<TEvent>`
Publishes an event to all registered subscribers asynchronously.
- **Generic Type**: `TEvent` - The type of event to publish
- **Parameters**: None (event data mechanism not specified in signature)
- **Return Value**: `Task` - Represents the asynchronous operation completion
- **Exceptions**: May throw exceptions from subscriber handlers or if no subscribers exist
- **Thread Safety**: Not thread-safe; concurrent publishing may cause race conditions

#### `Unsubscribe<TEvent>`
Removes a previously registered handler for the specified event type.
- **Generic Type**: `TEvent` - The type of event to unsubscribe from
- **Parameters**: None (handler identification mechanism not specified in signature)
- **Return Value**: `void`
- **Exceptions**: May throw if unsubscription fails due to invalid handler reference
- **Thread Safety**: Not thread-safe; external synchronization required during subscription changes

## Usage

```csharp
// Publishing a video download completion event
public async Task HandleDownloadCompletionAsync(string videoId, string filePath, long fileSize)
{
    var downloadEvent = new VideoDownloadedEvent
    {
        Id = Guid.NewGuid().ToString(),
        OccurredAt = DateTime.UtcNow,
        VideoId = videoId,
        FilePath = filePath,
        FileSize = fileSize
    };
    
    await DomainEvent.PublishAsync<VideoDownloadedEvent>(downloadEvent);
}
```

```csharp
// Subscribing to error events for monitoring
public void SetupErrorMonitoring()
{
    DomainEvent.Subscribe<VideoProcessingErrorEvent>(async (errorEvent) =>
    {
        Console.WriteLine($"Video {errorEvent.VideoId} failed: {errorEvent.Error}");
        // Additional error handling logic
    });
}
```

## Notes

The duplicate `VideoId` and `OutputFile` property declarations suggest this type may serve as a base class for multiple specialized event types, each inheriting common properties. Thread safety concerns apply primarily to the subscription management methods (`Subscribe`, `PublishAsync`, `Unsubscribe`) which likely modify shared state; external locking should be implemented when these methods are called concurrently. The generic methods imply a type-safe event system where concrete event types derive from `DomainEvent` and are handled by strongly-typed subscribers.
