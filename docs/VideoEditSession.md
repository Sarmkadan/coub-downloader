# VideoEditSession

The `VideoEditSession` class serves as an immutable data container representing a configured video editing workflow within the `coub-downloader` project. It encapsulates all necessary metadata, source references, operation chains, and output configurations required to process video clips, including trimming, merging, and applying effects. The type enforces strict initialization requirements for critical paths and identifiers while providing functional methods to derive new session states with modified operation lists.

## API

### Properties

#### `SessionId`
```csharp
public required string SessionId { get; init; }
```
A unique identifier for the specific editing session. This property is mandatory and must be set during object initialization. It is typically used for tracking, logging, and correlating async processing tasks.

#### `SourceFilePath`
```csharp
public required string SourceFilePath { get; init; }
```
The absolute or relative file path to the primary source video file. This property is mandatory. The file must exist and be readable at the time of execution, though validation is not performed by the property setter itself.

#### `Operations`
```csharp
public IReadOnlyList<EditOperation> Operations { get; init; }
```
A read-only list containing the sequence of editing operations to be applied. The list is immutable from the outside; to modify the sequence, a new session instance must be created via `WithOperation`.

#### `CreatedAt`
```csharp
public DateTime CreatedAt { get; init; }
```
The UTC timestamp indicating when the session object was instantiated. This is automatically populated during creation.

#### `IsDirty`
```csharp
public bool IsDirty { get; init; }
```
A flag indicating whether the current session state has been modified relative to its last persisted or committed state. This is useful for optimization logic to determine if re-processing is necessary.

#### `Label`
```csharp
public required string Label { get; init; }
```
A human-readable label or name assigned to the session, often used for UI display or log identification. This property is mandatory.

#### `Timestamp`
```csharp
public DateTime Timestamp { get; init; }
```
A generic timestamp associated with the session, distinct from `CreatedAt`. Its specific semantic meaning depends on the context of the operation (e.g., source media timestamp or user request time).

#### `StartTime`
```csharp
public required TimeSpan StartTime { get; init; }
```
The starting point within the source timeline for the edit operation. This property is mandatory and defines the lower bound of the processing window.

#### `EndTime`
```csharp
public TimeSpan? EndTime { get; init; }
```
The optional ending point within the source timeline. If `null`, the operation typically extends to the end of the source clip.

#### `Mode`
```csharp
public TrimMode Mode { get; init; }
```
Specifies the strategy used for trimming operations (e.g., precise frame cutting vs. keyframe approximation).

#### `ClipPaths`
```csharp
public required IReadOnlyList<string> ClipPaths { get; init; }
```
A mandatory list of file paths representing individual video clips involved in the session, particularly relevant for merge operations.

#### `Strategy`
```csharp
public MergeStrategy Strategy { get; init; }
```
Defines the algorithm or approach used when merging multiple video clips (e.g., concatenation method, codec handling).

#### `TransitionDuration`
```csharp
public TimeSpan TransitionDuration { get; init; }
```
The duration of transitions applied between clips during a merge operation. Defaults to zero if no transition is desired.

#### `Effects`
```csharp
public required IReadOnlyList<VideoEffect> Effects { get; init; }
```
A mandatory list of video effects to be applied to the output. The order of effects in the list generally dictates the order of application.

#### `Type`
```csharp
public required VideoEffectType Type { get; init; }
```
Specifies the category or specific type of video effect associated with the current configuration context. This property is mandatory.

#### `Intensity`
```csharp
public double Intensity { get; init; }
```
A numeric value representing the strength or magnitude of an applied effect. The valid range depends on the specific `VideoEffectType`.

#### `Parameters`
```csharp
public IReadOnlyDictionary<string, string> Parameters { get; init; }
```
A read-only dictionary of key-value pairs providing additional configuration parameters for effects or processing steps.

#### `OutputFilePath`
```csharp
public required string OutputFilePath { get; init; }
```
The destination file path where the processed video will be saved. This property is mandatory. The directory must exist and be writable at execution time.

### Methods

#### `Create`
```csharp
public static VideoEditSession Create(...)
```
Factory method used to instantiate a new `VideoEditSession`.
*   **Parameters**: Accepts arguments corresponding to the `required` properties and optional configurations. Specific parameter signatures depend on the overload used, but generally include `sessionId`, `sourcePath`, `outputPath`, and initial operation sets.
*   **Return Value**: A new instance of `VideoEditSession`.
*   **Exceptions**: Throws `ArgumentNullException` if any required string or collection arguments are null. May throw `ArgumentException` if paths are invalid formats.

#### `WithOperation`
```csharp
public VideoEditSession WithOperation(EditOperation operation)
```
Creates a new `VideoEditSession` instance based on the current one, appending the specified operation to the `Operations` list. This adheres to immutability patterns.
*   **Parameters**:
    *   `operation`: The `EditOperation` to add to the sequence.
*   **Return Value**: A new `VideoEditSession` object with an updated `Operations` list.
*   **Exceptions**: Throws `ArgumentNullException` if `operation` is null.

## Usage

### Example 1: Initializing a Basic Trim Session
The following example demonstrates creating a session to trim a single video clip from 10 seconds to 30 seconds.

```csharp
using CoubDownloader.Models;

// Define the edit operation
var trimOp = new EditOperation(
    startTime: TimeSpan.FromSeconds(10),
    endTime: TimeSpan.FromSeconds(30),
    mode: TrimMode.Precise
);

// Create the session
var session = VideoEditSession.Create(
    sessionId: Guid.NewGuid().ToString(),
    sourceFilePath: "/media/input_raw.mp4",
    outputFilePath: "/media/output_trimmed.mp4",
    label: "Intro Trim",
    startTime: TimeSpan.FromSeconds(10),
    endTime: TimeSpan.FromSeconds(30),
    clipPaths: new List<string> { "/media/input_raw.mp4" },
    effects: new List<VideoEffect>(),
    type: VideoEffectType.None
);

// Add the operation immutably
var finalSession = session.WithOperation(trimOp);

Console.WriteLine($"Session {finalSession.SessionId} ready. Dirty: {finalSession.IsDirty}");
```

### Example 2: Configuring a Merge with Effects
This example illustrates setting up a session to merge two clips with a crossfade transition and a brightness effect.

```csharp
using CoubDownloader.Models;

var clips = new List<string> 
{ 
    "/media/clip_a.mp4", 
    "/media/clip_b.mp4" 
};

var effects = new List<VideoEffect>
{
    new VideoEffect 
    { 
        Type = VideoEffectType.Brightness, 
        Intensity = 1.2, 
        Parameters = new Dictionary<string, string> { { "gamma", "1.1" } } 
    }
};

var session = VideoEditSession.Create(
    sessionId: "merge-job-001",
    sourceFilePath: clips[0], // Primary source reference
    outputFilePath: "/media/merged_final.mp4",
    label: "Highlight Reel",
    startTime: TimeSpan.Zero,
    endTime: null,
    clipPaths: clips,
    effects: effects,
    type: VideoEffectType.Composite
);

// Configure merge specifics (assuming properties are set via initializer or specific builder in real impl)
// Note: In this immutable model, specific property updates usually require a 'WithX' method 
// or are handled during the Create step if the factory supports complex init.
// Here we assume the Create method handled the list population.

Console.WriteLine($"Merging {session.ClipPaths.Count} clips with {session.Effects.Count} effects.");
```

## Notes

*   **Immutability**: `VideoEditSession` is designed as an immutable record. Properties marked `required` must be set at construction. Modifications to the operation chain must be performed using `WithOperation`, which returns a new instance rather than altering the existing one. This ensures thread safety for read operations without locking.
*   **Path Validation**: The class does not validate file system existence or permissions upon property assignment. Errors related to missing `SourceFilePath`, invalid `OutputFilePath` directories, or inaccessible `ClipPaths` will only surface during the actual execution of the edit pipeline.
*   **Nullable EndTime**: When `EndTime` is `null`, the processing logic interprets this as "end of source." Consumers must ensure the underlying processing engine handles this nullability correctly to avoid truncating videos unexpectedly.
*   **Collection Safety**: Properties returning `IReadOnlyList` and `IReadOnlyDictionary` prevent external modification of the internal collections. However, if the objects contained within these collections (e.g., `VideoEffect`) are mutable, their internal state could theoretically change. It is recommended that domain objects like `VideoEffect` also be treated as immutable.
*   **Thread Safety**: Read access to any instance of `VideoEditSession` is thread-safe. Creating new instances via `WithOperation` or `Create` is also safe. No internal synchronization primitives are used, relying instead on the immutability of the state.
