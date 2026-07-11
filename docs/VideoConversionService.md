# VideoConversionService

The `VideoConversionService` encapsulates interactions with FFmpeg to perform common video processing tasks such as conversion, metadata extraction, audio track application, rescaling, and creating short-form clips. It provides asynchronous methods that invoke FFmpeg under the hood and return results as file paths or metadata objects.

## API

### ConvertVideoAsync
- **Purpose:** Transcodes a video file to a target format using FFmpeg.
- **Parameters:** None.
- **Return Value:** A `Task<string>` that resolves to the absolute path of the converted video file.
- **Exceptions:** 
  - `InvalidOperationException` if FFmpeg is not available on the system.
  - `IOException` if the source file cannot be read or the destination cannot be written.
  - `OperationCanceledException` if the operation is cancelled via a cancellation token (if supported by the underlying implementation).

### GetVideoMetadataAsync
- **Purpose:** Retrieves metadata (duration, resolution, codec, etc.) for a video file.
- **Parameters:** None.
- **Return Value:** A `Task<VideoMetadata>` that resolves to an object containing the video’s metadata.
- **Exceptions:** 
  - `InvalidOperationException` if FFmpeg is not available.
  - `IOException` if the video file cannot be accessed.
  - `FormatException` if the file is not a recognized video format.

### ApplyAudioTrackAsync
- **Purpose:** Merges an external audio track with a video file, replacing or mixing the existing audio.
- **Parameters:** None.
- **Return Value:** A `Task<string>` that resolves to the path of the video file with the applied audio track.
- **Exceptions:** 
  - `InvalidOperationException` if FFmpeg is not available.
  - `IOException` if either the video or audio file cannot be read/written.
  - `ArgumentException` if the audio track is incompatible with the video container.

### IsFfmpegAvailableAsync
- **Purpose:** Checks whether the FFmpeg executable is present and executable on the host system.
- **Parameters:** None.
- **Return Value:** A `Task<bool>` that resolves to `true` if FFmpeg is available, otherwise `false`.
- **Exceptions:** None (the method is designed to return `false` rather than throw when FFmpeg is missing).

### GetFfmpegVersionAsync
- **Purpose:** Queries the version string of the installed FFmpeg executable.
- **Parameters:** None.
- **Return Value:** A `Task<string>` that resolves to the version output (e.g., `"ffmpeg version 4.4.1"`).
- **Exceptions:** 
  - `InvalidOperationException` if FFmpeg is not available.
  - `IOException` if executing the version command fails.

### RescaleVideoAsync
- **Purpose:** Changes the resolution of a video file (e.g., downscaling to 720p).
- **Parameters:** None.
- **Return Value:** A `Task<string>` that resolves to the path of the rescaled video file.
- **Exceptions:** 
  - `InvalidOperationException` if FFmpeg is not available.
  - `IOException` if the source or destination file cannot be accessed.
  - `ArgumentException` if the requested resolution is invalid or unsupported.

### ConvertToShortsAsync
- **Purpose:** Creates a short-form vertical video (suitable for platforms like YouTube Shorts) from a source video, typically by cropping and resizing.
- **Parameters:** None.
- **Return Value:** A `Task<string>` that resolves to the path of the generated short video.
- **Exceptions:** 
  - `InvalidOperationException` if FFmpeg is not available.
  - `IOException` if file access fails.
  - `ArgumentException` if the source video dimensions are insufficient for the desired short format.

## Usage

```csharp
using System.Threading.Tasks;
using CoubDownloader.Services; // Adjust namespace as needed

public class ExampleWorker
{
    private readonly VideoConversionService _conversionService;

    public ExampleWorker(VideoConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    public async Task ProcessVideoAsync(string inputPath)
    {
        // Ensure FFmpeg is available before proceeding
        if (!await _conversionService.IsFfmpegAvailableAsync())
        {
            throw new InvalidOperationException("FFmpeg is required but not found.");
        }

        // Get metadata to inform downstream steps
        var metadata = await _conversionService.GetVideoMetadataAsync();

        // Convert to MP4 if needed
        var convertedPath = await _conversionService.ConvertVideoAsync();

        // Apply a custom audio track
        var finalPath = await _conversionService.ApplyAudioTrackAsync();

        // Further processing (e.g., rescaling) can be chained similarly
    }
}
```

```csharp
using System.Threading.Tasks;
using CoubDownloader.Services;

public class ShortsCreator
{
    private readonly VideoConversionService _service;

    public ShortsCreator(VideoConversionService service)
    {
        _service = service;
    }

    public async Task<string> CreateShortsFromVideo(string sourceVideo)
    {
        // Optionally verify FFmpeg version for feature compatibility
        var version = await _service.GetFfmpegVersionAsync();
        // version logging or validation could happen here

        // Generate the short-form video
        string shortsPath = await _service.ConvertToShortsAsync();
        return shortsPath;
    }
}
```

## Notes

- All methods are asynchronous and should be awaited; calling them without `await` may lead to unobserved exceptions.
- The class does not maintain mutable state that is altered by these methods; therefore, concurrent invocations from multiple threads are safe **provided** that the underlying file system paths used by each call are distinct. Sharing the same input or output paths across concurrent calls can result in race conditions or corrupted output.
- If FFmpeg becomes unavailable after a successful `IsFfmpegAvailableAsync` check, subsequent calls may still throw `InvalidOperationException`; it is recommended to treat the availability check as a best‑effort hint rather than a guarantee.
- Methods that return file paths assume the calling code has appropriate permissions to read from the source location and write to the destination location; insufficient permissions will surface as `IOException`.
- The service does not automatically clean up temporary files; callers are responsible for managing any intermediate files produced during processing.
