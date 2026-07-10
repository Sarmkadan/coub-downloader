# FFmpegWrapper

A lightweight wrapper around the FFmpeg command-line tool, designed to simplify video and audio processing tasks such as conversion, extraction, concatenation, and media metadata retrieval in .NET applications. It abstracts FFmpeg execution, handles process management, and provides structured results with success status and output.

## API

### `public FFmpegWrapper`

Initializes a new instance of the `FFmpegWrapper` class. The wrapper uses the system-installed FFmpeg binary by default unless overridden via configuration. Hardware acceleration is disabled by default and can be enabled via the `UseHardwareAcceleration` property.

### `public async Task<bool> IsAvailableAsync()`

Determines whether FFmpeg is available and executable on the system.

- **Return value**: `true` if FFmpeg is found and executable; otherwise, `false`.
- **Exceptions**: May throw `FileNotFoundException` if FFmpeg is not found in the system path, or `UnauthorizedAccessException` if the executable is not accessible.

---

### `public async Task<string> GetVersionAsync()`

Retrieves the version string of the FFmpeg executable currently in use.

- **Return value**: A string representing the FFmpeg version (e.g., `"n5.1.2"`).
- **Exceptions**: Throws `InvalidOperationException` if FFmpeg is not available or fails to execute.

---

### `public async Task<FFmpegResult> ExecuteAsync(string arguments)`

Executes FFmpeg with the specified command-line arguments.

- **Parameters**:
  - `arguments`: A string containing valid FFmpeg command-line arguments (e.g., `"-i input.mp4 -c:v libx264 output.mp4"`).
- **Return value**: An `FFmpegResult` object containing `Success` status, `Output` text, and any error output.
- **Exceptions**: Throws `ArgumentNullException` if `arguments` is `null` or empty. Throws `InvalidOperationException` if FFmpeg is not available.

---

### `public async Task<FFmpegResult> ConvertVideoAsync(string inputPath, string outputPath, string videoCodec = "libx264", string audioCodec = "aac", int? videoBitrate = null, int? audioBitrate = null, int? frameRate = null, int? width = null, int? height = null)`

Converts a video file to a new format with optional transcoding parameters.

- **Parameters**:
  - `inputPath`: Path to the input video file.
  - `outputPath`: Path where the output video will be saved.
  - `videoCodec`: Target video codec (default: `"libx264"`).
  - `audioCodec`: Target audio codec (default: `"aac"`).
  - `videoBitrate`: Target video bitrate in kbps (optional).
  - `audioBitrate`: Target audio bitrate in kbps (optional).
  - `frameRate`: Target frame rate (optional).
  - `width`: Target width in pixels (optional; maintains aspect ratio if height is also specified).
  - `height`: Target height in pixels (optional; maintains aspect ratio if width is also specified).
- **Return value**: An `FFmpegResult` indicating success or failure and containing FFmpeg output.
- **Exceptions**: Throws `ArgumentNullException` if `inputPath` or `outputPath` is `null` or empty. Throws `FileNotFoundException` if `inputPath` does not exist. Throws `InvalidOperationException` if FFmpeg is not available.

---

### `public async Task<FFmpegResult> ExtractAudioAsync(string inputPath, string outputPath, string audioCodec = "aac", int? audioBitrate = null)`

Extracts the audio stream from a media file and saves it as a new audio file.

- **Parameters**:
  - `inputPath`: Path to the input media file.
  - `outputPath`: Path where the output audio file will be saved.
  - `audioCodec`: Target audio codec (default: `"aac"`).
  - `audioBitrate`: Target audio bitrate in kbps (optional).
- **Return value**: An `FFmpegResult` indicating success or failure and containing FFmpeg output.
- **Exceptions**: Throws `ArgumentNullException` if `inputPath` or `outputPath` is `null` or empty. Throws `FileNotFoundException` if `inputPath` does not exist. Throws `InvalidOperationException` if FFmpeg is not available.

---
### `public async Task<FFmpegResult> ConcatenateVideosAsync(string[] inputPaths, string outputPath)`

Concatenates multiple video files into a single output file using the concat demuxer.

- **Parameters**:
  - `inputPaths`: Array of paths to input video files (must be compatible in codec and format).
  - `outputPath`: Path where the concatenated video will be saved.
- **Return value**: An `FFmpegResult` indicating success or failure and containing FFmpeg output.
- **Exceptions**: Throws `ArgumentNullException` if `inputPaths` or `outputPath` is `null` or empty. Throws `ArgumentException` if `inputPaths` is empty or contains invalid paths. Throws `FileNotFoundException` if any input file does not exist. Throws `InvalidOperationException` if FFmpeg is not available.

---
### `public async Task<FFmpegResult> LoopAudioAsync(string inputPath, string outputPath, int loopCount)`

Loops the audio stream from the input file the specified number of times and saves it as a new audio file.

- **Parameters**:
  - `inputPath`: Path to the input media file.
  - `outputPath`: Path where the output audio file will be saved.
  - `loopCount`: Number of times to loop the audio (must be ≥ 1).
- **Return value**: An `FFmpegResult` indicating success or failure and containing FFmpeg output.
- **Exceptions**: Throws `ArgumentNullException` if `inputPath` or `outputPath` is `null` or empty. Throws `ArgumentOutOfRangeException` if `loopCount` < 1. Throws `FileNotFoundException` if `inputPath` does not exist. Throws `InvalidOperationException` if FFmpeg is not available.

---
### `public async Task<MediaInfo?> GetMediaInfoAsync(string inputPath)`

Retrieves detailed media information for the specified file by parsing FFmpeg output.

- **Parameters**:
  - `inputPath`: Path to the input media file.
- **Return value**: A `MediaInfo` object containing metadata such as duration, codecs, bitrates, resolution, and frame rate; `null` if the file is invalid or FFmpeg fails.
- **Exceptions**: Throws `ArgumentNullException` if `inputPath` is `null` or empty. Throws `FileNotFoundException` if `inputPath` does not exist. Throws `InvalidOperationException` if FFmpeg is not available.

---
### `public MediaInfo? Format`

Gets the last parsed media metadata from the most recent `GetMediaInfoAsync` call. Returns `null` if no media info has been retrieved or the last call failed.

- **Return value**: A `MediaInfo` object or `null`.
- **Thread Safety**: This property is not thread-safe. Concurrent access may lead to inconsistent results.

---
### `public string VideoCodec`

Gets the video codec of the last processed media file (from `Format`). Returns an empty string if no video stream is present or no media info is available.

- **Return value**: A string representing the video codec (e.g., `"h264"`).
- **Thread Safety**: Not thread-safe.

---
### `public string AudioCodec`

Gets the audio codec of the last processed media file (from `Format`). Returns an empty string if no audio stream is present or no media info is available.

- **Return value**: A string representing the audio codec (e.g., `"aac"`).
- **Thread Safety**: Not thread-safe.

---
### `public int VideoBitrate`

Gets the video bitrate (in kbps) of the last processed media file (from `Format`). Returns `0` if no video stream or bitrate is unavailable.

- **Return value**: An integer representing the video bitrate in kbps.
- **Thread Safety**: Not thread-safe.

---
### `public int AudioBitrate`

Gets the audio bitrate (in kbps) of the last processed media file (from `Format`). Returns `0` if no audio stream or bitrate is unavailable.

- **Return value**: An integer representing the audio bitrate in kbps.
- **Thread Safety**: Not thread-safe.

---
### `public int FrameRate`

Gets the average frame rate of the last processed media file (from `Format`). Returns `0` if no video stream or frame rate is unavailable.

- **Return value**: An integer representing the frame rate in frames per second.
- **Thread Safety**: Not thread-safe.

---
### `public int Width`

Gets the pixel width of the last processed media file (from `Format`). Returns `0` if no video stream or resolution is unavailable.

- **Return value**: An integer representing the width in pixels.
- **Thread Safety**: Not thread-safe.

---
### `public int Height`

Gets the pixel height of the last processed media file (from `Format`). Returns `0` if no video stream or resolution is unavailable.

- **Return value**: An integer representing the height in pixels.
- **Thread Safety**: Not thread-safe.

---
### `public bool UseHardwareAcceleration`

Gets or sets whether to enable hardware-accelerated video encoding/decoding (e.g., via `h264_nvenc` or `h264_amf`). Default is `false`.

- **Thread Safety**: Not thread-safe. Changes should be made before invoking async operations.

---
### `public bool Success`

Gets the success status of the last executed FFmpeg operation (from the most recent `ExecuteAsync` or derived method). Returns `false` if no operation has been performed.

- **Return value**: `true` if the last operation succeeded; otherwise, `false`.
- **Thread Safety**: Not thread-safe.

---
### `public string Output`

Gets the raw output (stdout + stderr) from the last executed FFmpeg operation. Returns an empty string if no operation has been performed.

- **Return value**: A string containing FFmpeg output.
- **Thread Safety**: Not thread-safe.

## Usage

### Example 1: Convert a video with hardware acceleration

```csharp
var ffmpeg = new FFmpegWrapper();
ffmpeg.UseHardwareAcceleration = true;

var result = await ffmpeg.ConvertVideoAsync(
    inputPath: "input.mp4",
    outputPath: "output_hw.mp4",
    videoCodec: "h264_nvenc",
    audioCodec: "aac",
    videoBitrate: 2500,
    frameRate: 60
);

if (result.Success)
{
    Console.WriteLine("Conversion succeeded.");
}
else
{
    Console.WriteLine($"Conversion failed: {result.Output}");
}
```

---

### Example 2: Extract audio and get metadata

```csharp
var ffmpeg = new FFmpegWrapper();

var extractResult = await ffmpeg.ExtractAudioAsync(
    inputPath: "video_with_audio.mp4",
    outputPath: "audio_only.aac",
    audioBitrate: 192
);

if (extractResult.Success)
{
    Console.WriteLine("Audio extracted.");
}
else
{
    Console.WriteLine($"Extraction failed: {extractResult.Output}");
}

var mediaInfo = await ffmpeg.GetMediaInfoAsync("video_with_audio.mp4");
if (mediaInfo != null)
{
    Console.WriteLine($"Duration: {mediaInfo.Duration}, Audio Codec: {ffmpeg.AudioCodec}");
}
```

## Notes

- **Thread Safety**: The `FFmpegWrapper` class is **not thread-safe**. Each instance should be used from a single thread, or external synchronization must be applied. Concurrent calls to async methods on the same instance may lead to race conditions in property access (e.g., `Format`, `Success`, `Output`) and inconsistent state.

- **FFmpeg Availability**: The wrapper assumes FFmpeg is installed and available in the system `PATH`. If FFmpeg is not found, methods will throw or return failure. Consider calling `IsAvailableAsync()` at startup to validate the environment.

- **Error Handling**: FFmpeg errors are surfaced via the `Output` property in `FFmpegResult`. Always check `Success` before relying on derived properties like `Format`.

- **Hardware Acceleration**: Enabling `UseHardwareAcceleration` does not guarantee success. The availability of hardware encoders depends on GPU drivers and FFmpeg build. If unsupported, FFmpeg may fall back to software encoding.

- **Media Metadata**: Properties like `VideoCodec`, `FrameRate`, etc., reflect the **last processed file** via `GetMediaInfoAsync`. They do not update automatically after file changes unless `GetMediaInfoAsync` is called again.

- **File Paths**: All file paths should be absolute or correctly resolved relative to the current working directory. Relative paths may cause failures in async contexts if the working directory changes.

- **Resource Cleanup**: The wrapper does not manage FFmpeg process lifecycle beyond execution. Long-running operations should be monitored to avoid resource exhaustion.
