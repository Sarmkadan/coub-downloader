# IFileAdapter

The `IFileAdapter` interface provides a contract for file system operations and media processing tasks, primarily integrating with FFmpeg and FFprobe to perform video conversion, audio extraction, concatenation, looping, and metadata retrieval.

## API

### `IsAvailableAsync` (async Task<bool>)
Determines whether the underlying FFmpeg executable is available and functional.
- **Return value**: `true` if FFmpeg is available and returns success; `false` if FFmpeg returns failure or throws an exception.
- **Exceptions**: Any exception thrown during execution is caught and results in `false`.

### `GetVersionAsync` (async Task<string>)
Retrieves the version string of the FFmpeg executable.
- **Return value**: The version string if FFmpeg returns valid output; otherwise, `"Unknown"`.
- **Exceptions**: Any exception thrown during execution is caught and results in `"Unknown"`.

### `ConvertVideoAsync` (async Task)
Converts a video file to a specified format using FFmpeg.
- **Parameters**: Input path, output path, hardware acceleration flag, and optional format.
- **Behavior**: When hardware acceleration is disabled, the `-hwaccel` argument is omitted.
- **Exceptions**: No exceptions are thrown; failures are handled internally.

### `ExtractAudioAsync` (async Task)
Extracts audio from a video file and saves it to a specified path.
- **Parameters**: Input video path and output audio path.
- **Exceptions**: No exceptions are thrown; failures are handled internally.

### `ConcatenateVideosAsync` (async Task)
Concatenates multiple video files into a single output file.
- **Parameters**: List of input file paths and output file path.
- **Behavior**: Uses a temporary file for intermediate processing and cleans it up afterward.
- **Exceptions**: No exceptions are thrown; failures are handled internally.

### `LoopAudioAsync` (async Task)
Loops an audio file for a specified duration.
- **Parameters**: Input audio path, output path, and loop duration.
- **Exceptions**: No exceptions are thrown; failures are handled internally.

### `GetMediaInfoAsync` (async Task<MediaInfo?>)
Retrieves media metadata (e.g., duration, streams) from a file using FFprobe.
- **Parameters**: Input file path.
- **Return value**: A `MediaInfo` object if FFprobe returns valid output; otherwise, `null`.
- **Exceptions**: Errors are logged, and `null` is returned.

### `ExecuteAsync` (async Task<ProcessExecutionResult>)
Executes a shell command and returns the result.
- **Parameters**: Command string and optional timeout.
- **Return value**: A `ProcessExecutionResult` indicating success or failure.
- **Behavior**: If a timeout is specified, the process is forcibly terminated if it exceeds the limit.
- **Exceptions**: No exceptions are thrown; failures are encapsulated in the result.

## Usage

### Example 1: Converting a Video with Hardware Acceleration
