# VideoEditorServiceExtensions

Provides static extension methods for the `IVideoEditorService` interface, enabling common video editing operations such as trimming, rendering, preview generation, and effect application. These methods simplify the process of applying a sequence of edits to a video file and retrieving the result.

## API

### `TrimFirstSecondsAsync`
Trims the first specified number of seconds from the source video and returns the edited result.

- **Parameters**
  - `service`: The `IVideoEditorService` instance to extend.
  - `sourcePath`: The file path of the source video.
  - `seconds`: The number of seconds to remove from the beginning of the video.
  - `outputPath`: Optional. The file path where the trimmed video should be saved. If `null`, a temporary file is used.
  - `cancellationToken`: Optional. A token to monitor for cancellation requests.

- **Returns**
  A `Task<VideoEditResult>` representing the asynchronous operation. The result contains the path to the edited video and any metadata generated during processing.

- **Exceptions**
  Throws `ArgumentException` if `seconds` is negative or if `sourcePath` is invalid.
  Throws `OperationCanceledException` if the operation is canceled via the `cancellationToken`.

---

### `TrimAndRenderAsync`
Trims the video to the specified time range and renders the result to the given output path.

- **Parameters**
  - `service`: The `IVideoEditorService` instance to extend.
  - `sourcePath`: The file path of the source video.
  - `startTime`: The start time of the segment to keep.
  - `endTime`: The end time of the segment to keep.
  - `outputPath`: The file path where the trimmed and rendered video should be saved.
  - `cancellationToken`: Optional. A token to monitor for cancellation requests.

- **Returns**
  A `Task<VideoEditResult>` representing the asynchronous operation. The result contains the path to the edited video and any metadata.

- **Exceptions**
  Throws `ArgumentException` if `startTime` or `endTime` are invalid or if `outputPath` is not writable.
  Throws `OperationCanceledException` if the operation is canceled.

---

### `GenerateStandardPreviewAsync`
Generates a standard preview (e.g., a short loop or highlight clip) from the source video.

- **Parameters**
  - `service`: The `IVideoEditorService` instance to extend.
  - `sourcePath`: The file path of the source video.
  - `duration`: The desired duration of the preview in seconds.
  - `outputPath`: Optional. The file path where the preview should be saved. If `null`, a temporary file is used.
  - `cancellationToken`: Optional. A token to monitor for cancellation requests.

- **Returns**
  A `Task<VideoEditResult>` representing the asynchronous operation. The result contains the path to the generated preview.

- **Exceptions**
  Throws `ArgumentException` if `duration` is non-positive or if `sourcePath` is invalid.
  Throws `OperationCanceledException` if the operation is canceled.

---
### `ApplyEffectsAsync`
Applies a sequence of video effects (e.g., filters, overlays) to the source video and returns the edited result.

- **Parameters**
  - `service`: The `IVideoEditorService` instance to extend.
  - `sourcePath`: The file path of the source video.
  - `effects`: A collection of effect configurations to apply.
  - `outputPath`: Optional. The file path where the modified video should be saved. If `null`, a temporary file is used.
  - `cancellationToken`: Optional. A token to monitor for cancellation requests.

- **Returns**
  A `Task<VideoEditResult>` representing the asynchronous operation. The result contains the path to the edited video and any metadata.

- **Exceptions**
  Throws `ArgumentNullException` if `effects` is `null`.
  Throws `ArgumentException` if any effect configuration is invalid.
  Throws `OperationCanceledException` if the operation is canceled.

---
### `GetEditHistory`
Retrieves the history of edit operations applied during the current session.

- **Parameters**
  - `session`: The `VideoEditSession` instance to inspect.

- **Returns**
  An `IReadOnlyList<string>` of human-readable descriptions of each edit operation performed in the session, in chronological order.

- **Exceptions**
  Throws `ArgumentNullException` if `session` is `null`.

---
### `WithOperations`
Creates a new `VideoEditSession` with the specified sequence of edit operations.

- **Parameters**
  - `session`: The original `VideoEditSession` instance.
  - `operations`: A collection of edit operations to apply in sequence.

- **Returns**
  A new `VideoEditSession` instance containing the combined operations of the original session and the new operations.

- **Exceptions**
  Throws `ArgumentNullException` if `session` or `operations` is `null`.

## Usage

### Example 1: Trimming and Applying Effects
