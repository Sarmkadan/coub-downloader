# DomainEventExtensions

Provides a set of static extension methods for domain events, specifically those related to video download operations. These utilities simplify common tasks such as cloning an event, extracting metadata (video ID, output file, error message, duration, file size), determining success or failure, generating a human-readable log message, and filtering events by video association.

## API

### `DeepClone<TEvent>(this TEvent event)`
- **Purpose**: Creates a deep copy of the event instance.
- **Parameters**:  
  `event` – The event to clone.
- **Returns**: A new instance of `TEvent` that is a deep copy of the original.
- **Throws**: `ArgumentNullException` if `event` is `null`.

### `GetVideoId(this DomainEvent event)`
- **Purpose**: Extracts the video identifier from the event.
- **Parameters**:  
  `event` – The domain event.
- **Returns**: A `string` containing the video ID, or `null` if the event does not carry a video ID.
- **Throws**: `ArgumentNullException` if `event` is `null`.

### `GetOutputFile(this DomainEvent event)`
- **Purpose**: Retrieves the output file path associated with the event.
- **Parameters**:  
  `event` – The domain event.
- **Returns**: A `string` with the file path, or `null` if no output file is defined.
- **Throws**: `ArgumentNullException` if `event` is `null`.

### `GetError(this DomainEvent event)`
- **Purpose**: Gets the error message if the event represents a failure.
- **Parameters**:  
  `event` – The domain event.
- **Returns**: A `string` containing the error description, or `null` if the event is not a failure.
- **Throws**: `ArgumentNullException` if `event` is `null`.

### `IsFailure(this DomainEvent event)`
- **Purpose**: Indicates whether the event represents a failed operation.
- **Parameters**:  
  `event` – The domain event.
- **Returns**: `true` if the event is a failure; otherwise `false`.
- **Throws**: `ArgumentNullException` if `event` is `null`.

### `IsSuccess(this DomainEvent event)`
- **Purpose**: Indicates whether the event represents a successful operation.
- **Parameters**:  
  `event` – The domain event.
- **Returns**: `true` if the event is a success; otherwise `false`.
- **Throws**: `ArgumentNullException` if `event` is `null`.

### `GetDuration(this DomainEvent event)`
- **Purpose**: Returns the duration associated with the event (e.g., video length).
- **Parameters**:  
  `event` – The domain event.
- **Returns**: A `TimeSpan` representing the duration.
- **Throws**:  
  - `ArgumentNullException` if `event` is `null`.  
  - `InvalidOperationException` if the event does not contain duration data (e.g., it is a failure event).

### `GetFileSize(this DomainEvent event)`
- **Purpose**: Returns the file size in bytes associated with the event.
- **Parameters**:  
  `event` – The domain event.
- **Returns**: A `long` representing the file size.
- **Throws**:  
  - `ArgumentNullException` if `event` is `null`.  
  - `InvalidOperationException` if the event does not contain file size data.

### `ToLogMessage(this DomainEvent event)`
- **Purpose**: Produces a formatted string suitable for logging the event’s details.
- **Parameters**:  
  `event` – The domain event.
- **Returns**: A `string` containing a human-readable representation of the event.
- **Throws**: `ArgumentNullException` if `event` is `null`.

### `IsForVideo(this DomainEvent event)`
- **Purpose**: Determines whether the event is associated with a specific video download operation.
- **Parameters**:  
  `event` – The domain event.
- **Returns**: `true` if the event is related to a video; otherwise `false`.
- **Throws**: `ArgumentNullException` if `event` is `null`.

## Usage

### Example 1: Checking download outcome and logging

```csharp
DomainEvent downloadEvent = GetDownloadEvent();

if (downloadEvent.IsSuccess())
{
    string videoId = downloadEvent.GetVideoId();
    string outputFile = downloadEvent.GetOutputFile();
    TimeSpan duration = downloadEvent.GetDuration();
    long fileSize = downloadEvent.GetFileSize();

    Console.WriteLine($"Download succeeded: {videoId} -> {outputFile} ({duration}, {fileSize} bytes)");
}
else
{
    string error = downloadEvent.GetError();
    Console.WriteLine($"Download failed: {error}");
}

// Log the full event details
Logger.Info(downloadEvent.ToLogMessage());
```

### Example 2: Cloning an event for retry

```csharp
DomainEvent failedEvent = GetFailedDownloadEvent();

if (failedEvent.IsFailure())
{
    // Create a deep clone to preserve original state
    var retryEvent = failedEvent.DeepClone();
    
    // Modify the clone (e.g., increment retry count) and re-queue
    retryEvent.RetryCount++;
    RetryQueue.Enqueue(retryEvent);
}
```

## Notes

- **Null handling**: All extension methods throw `ArgumentNullException` if the target event is `null`. Always validate the event instance before calling these methods.
- **Event type expectations**: Methods such as `GetDuration`, `GetFileSize`, `GetVideoId`, and `GetOutputFile` assume the event is of a concrete type that carries the respective data. Calling them on an event that does not contain the expected information (e.g., a generic failure event) will result in an `InvalidOperationException`.
- **Thread safety**: The extension methods themselves are stateless and do not modify the event instance. They are safe to call from multiple threads concurrently, provided the event object is not mutated during the call. Since domain events are typically immutable, no additional synchronization is required.
- **Deep cloning**: `DeepClone` uses serialization or memberwise copying to produce an independent copy. The cloned object will not share references with the original, making it safe for scenarios where the event needs to be modified or stored separately.
