# VideoEditorService

The `VideoEditorService` class provides a high-level API for performing video editing operations such as trimming, merging, applying effects, and generating previews. It manages editing sessions and maintains an edit history log. All editing operations are asynchronous and return a `VideoEditResult` indicating success or failure. The class is sealed and cannot be inherited.

## API

### `public sealed class VideoEditorService`

The main service class. Instantiate it to begin editing videos.

---

### `public Task<VideoEditSession> CreateSessionAsync`

Creates a new editing session. A session represents a working context for a video file and holds the current state of edits.

- **Returns**: A `Task<VideoEditSession>` representing the asynchronous operation. The session object can be used with other methods.
- **Throws**: `InvalidOperationException` if the service is not properly initialized or if a session cannot be created due to missing dependencies.

---

### `public async Task<VideoEditResult> TrimVideoAsync`

Trims a video within the specified session to a given start and end time.

- **Parameters**: Accepts a `VideoEditSession` and the trim boundaries (start and end times).
- **Returns**: A `VideoEditResult` indicating whether the operation succeeded and containing any error details.
- **Throws**: `ArgumentNullException` if the session is null. `InvalidOperationException` if the session is in an invalid state.

---

### `public async Task<VideoEditResult> MergeVideosAsync`

Merges multiple video sources into a single video within the current session.

- **Parameters**: Accepts a `VideoEditSession` and a collection of video sources (e.g., file paths or streams).
- **Returns**: A `VideoEditResult` indicating success or failure.
- **Throws**: `ArgumentNullException` if the session or source collection is null. `InvalidOperationException` if the session is not active.

---

### `public async Task<VideoEditResult> ApplyEffectsAsync`

Applies a set of visual or audio effects to the video in the session.

- **Parameters**: Accepts a `VideoEditSession` and an effects configuration object.
- **Returns**: A `VideoEditResult` indicating success or failure.
- **Throws**: `ArgumentNullException` if the session or effects configuration is null. `InvalidOperationException` if the effects cannot be applied to the current video format.

---

### `public async Task<VideoEditResult> GeneratePreviewAsync`

Generates a preview of the current edited state of the video. The preview is typically a lower-resolution or shorter clip for quick review.

- **Parameters**: Accepts a `VideoEditSession` and an output path or stream for the preview.
- **Returns**: A `VideoEditResult` indicating success or failure.
- **Throws**: `ArgumentNullException` if the session or output destination is null. `InvalidOperationException` if the session has no edits to preview.

---

### `public async Task<VideoEditResult> ApplySessionAsync`

Applies all pending edits in the session to produce the final output video.

- **Parameters**: Accepts a `VideoEditSession` and an output destination (file path or stream).
- **Returns**: A `VideoEditResult` indicating success or failure.
- **Throws**: `ArgumentNullException` if the session or output destination is null. `InvalidOperationException` if the session contains no edits or is in an invalid state.

---

### `public IReadOnlyList<string> GetEditHistory { get; }`

Gets a read-only list of strings describing each edit operation performed in the current session. The list is ordered chronologically.

- **Value**: An `IReadOnlyList<string>` where each string is a human-readable description of an edit (e.g., "Trimmed from 00:01:00 to 00:02:30").
- **Throws**: Never throws.

## Usage

### Example 1: Trim a video and retrieve edit history

```csharp
using var service = new VideoEditorService();

// Create a session for the source video
VideoEditSession session = await service.CreateSessionAsync();

// Trim the video to the first 30 seconds
VideoEditResult trimResult = await service.TrimVideoAsync(session, TimeSpan.Zero, TimeSpan.FromSeconds(30));
if (!trimResult.IsSuccess)
{
    Console.WriteLine($"Trim failed: {trimResult.ErrorMessage}");
    return;
}

// Apply the session to produce the final file
VideoEditResult applyResult = await service.ApplySessionAsync(session, "output_trimmed.mp4");
if (applyResult.IsSuccess)
{
    Console.WriteLine("Trimmed video saved.");
}

// Display edit history
foreach (string edit in service.GetEditHistory)
{
    Console.WriteLine(edit);
}
```

### Example 2: Merge two videos and generate a preview

```csharp
using var service = new VideoEditorService();

// Create a session
VideoEditSession session = await service.CreateSessionAsync();

// Merge two source videos
var sources = new[] { "intro.mp4", "main.mp4" };
VideoEditResult mergeResult = await service.MergeVideosAsync(session, sources);
if (!mergeResult.IsSuccess)
{
    Console.WriteLine($"Merge failed: {mergeResult.ErrorMessage}");
    return;
}

// Generate a low-res preview
VideoEditResult previewResult = await service.GeneratePreviewAsync(session, "preview_lowres.mp4");
if (previewResult.IsSuccess)
{
    Console.WriteLine("Preview generated.");
}
```

## Notes

- **Thread safety**: Instance members of `VideoEditorService` are not guaranteed to be thread-safe. If multiple threads access the same instance concurrently, external synchronization is required. For concurrent editing tasks, create separate service instances.
- **Session lifecycle**: A `VideoEditSession` is tied to the service instance that created it. Using a session with a different service instance may produce undefined behavior.
- **Null arguments**: All methods that accept session or configuration parameters throw `ArgumentNullException` when a required argument is null.
- **Error handling**: Always check the `IsSuccess` property of `VideoEditResult` before proceeding. The `ErrorMessage` property provides details on failure.
- **Edit history**: The `GetEditHistory` property reflects only edits performed through the current service instance. It is cleared when a new session is created.
- **Disposal**: The service may hold unmanaged resources (e.g., temporary files, codec handles). Dispose the service instance when it is no longer needed (if it implements `IDisposable`).
