# AudioProcessingService

The `AudioProcessingService` class provides asynchronous operations for manipulating audio streams within the `coub-downloader` project. It handles core media tasks such as extraction, looping, effect application, synchronization with video tracks, duration analysis, and volume adjustment, returning file paths to the processed output or numerical data where applicable.

## API

### Constructor

**`public AudioProcessingService()`**

Initializes a new instance of the `AudioProcessingService` class.

### Methods

**`public async Task<string> ExtractAudioAsync(string sourcePath, string outputPath)`**

Extracts the audio track from a specified media file and saves it to a new location.
*   **Parameters**:
    *   `sourcePath`: The full path to the input media file containing the audio.
    *   `outputPath`: The destination path where the extracted audio file will be saved.
*   **Returns**: A task representing the asynchronous operation, containing the full path to the successfully created audio file.
*   **Throws**: Throws an exception if the source file does not exist, the format is unsupported, or write permissions are denied at the output location.

**`public async Task<string> LoopAudioAsync(string sourcePath, int loopCount, string outputPath)`**

Creates a new audio file by repeating the source audio a specified number of times.
*   **Parameters**:
    *   `sourcePath`: The path to the source audio file.
    *   `loopCount`: The number of times the audio should be repeated.
    *   `outputPath`: The destination path for the looped audio file.
*   **Returns**: A task containing the path to the generated looped audio file.
*   **Throws**: Throws an exception if `loopCount` is less than 1 or if file I/O errors occur.

**`public async Task<string> ApplyAudioEffectsAsync(string sourcePath, IEnumerable<string> effects, string outputPath)`**

Processes an audio file by applying a sequence of audio effects.
*   **Parameters**:
    *   `sourcePath`: The path to the input audio file.
    *   `effects`: A collection of strings defining the effects to apply (e.g., filter names or configuration strings).
    *   `outputPath`: The destination path for the processed file.
*   **Returns**: A task containing the path to the modified audio file.
*   **Throws**: Throws an exception if an unrecognized effect is provided or if the processing pipeline fails.

**`public async Task<string> SyncAudioWithVideoAsync(string videoPath, string audioPath, string outputPath)`**

Combines a video stream and a separate audio stream into a single media file, ensuring synchronization.
*   **Parameters**:
    *   `videoPath`: The path to the video-only file.
    *   `audioPath`: The path to the audio-only file.
    *   `outputPath`: The destination path for the merged media file.
*   **Returns**: A task containing the path to the final synchronized media file.
*   **Throws**: Throws an exception if the streams are incompatible or if synchronization fails due to timestamp mismatches.

**`public async Task<double> GetAudioDurationAsync(string filePath)`**

Retrieves the total duration of an audio file in seconds.
*   **Parameters**:
    *   `filePath`: The path to the audio file to analyze.
*   **Returns**: A task containing the duration as a `double` representing seconds.
*   **Throws**: Throws an exception if the file is corrupt, not found, or contains no valid audio stream.

**`public async Task<string> AdjustVolumeAsync(string sourcePath, double gainFactor, string outputPath)`**

Modifies the volume level of an audio file by a specific gain factor.
*   **Parameters**:
    *   `sourcePath`: The path to the source audio file.
    *   `gainFactor`: The multiplier for the volume (e.g., 0.5 for half volume, 2.0 for double).
    *   `outputPath`: The destination path for the volume-adjusted file.
*   **Returns**: A task containing the path to the adjusted audio file.
*   **Throws**: Throws an exception if the gain factor is invalid (e.g., negative) or if processing fails.

## Usage

The following example demonstrates extracting audio from a downloaded Coub video and then adjusting its volume before saving the final result.

```csharp
var processor = new AudioProcessingService();
string videoFile = @"C:\Downloads\coub_12345.mp4";
string extractedAudio = @"C:\Temp\audio_raw.mp3";
string finalAudio = @"C:\Temp\audio_adjusted.mp3";

// Extract audio from the video container
string audioPath = await processor.ExtractAudioAsync(videoFile, extractedAudio);

// Reduce volume by 20%
string resultPath = await processor.AdjustVolumeAsync(audioPath, 0.8, finalAudio);

Console.WriteLine($"Processed audio saved to: {resultPath}");
```

The next example illustrates synchronizing a separate audio track with a video file and verifying the duration of the result.

```csharp
var processor = new AudioProcessingService();
string videoOnly = @"C:\Media\video_no_sound.mp4";
string audioTrack = @"C:\Media\commentary.mp3";
string outputFile = @"C:\Output\final_coub.mp4";

// Merge video and audio
string syncedFile = await processor.SyncAudioWithVideoAsync(videoOnly, audioTrack, outputFile);

// Verify duration
double duration = await processor.GetAudioDurationAsync(syncedFile);
Console.WriteLine($"Synced media duration: {duration:F2} seconds");
```

## Notes

*   **File System Dependencies**: All methods rely on the existence of valid file paths. Ensure that the directory structure for `outputPath` exists before invoking these methods, as the service does not automatically create missing directories.
*   **Thread Safety**: The `AudioProcessingService` is stateless regarding internal mutable fields; however, the underlying asynchronous file I/O operations are not atomic across multiple instances targeting the same file. Do not invoke multiple methods simultaneously on the same input or output file paths to prevent race conditions and file lock conflicts.
*   **Resource Management**: As these methods involve heavy media processing, they may consume significant CPU and I/O resources. It is recommended to await tasks sequentially when processing large batches of files to avoid system resource exhaustion.
*   **Error Handling**: Exceptions thrown by this service typically wrap underlying system or codec errors. Callers should implement specific `catch` blocks for `IOException`, `UnauthorizedAccessException`, and custom media processing exceptions to handle failures gracefully.
