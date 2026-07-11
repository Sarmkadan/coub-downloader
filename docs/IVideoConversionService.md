# IVideoConversionService

The `IVideoConversionService` interface defines the contract for accessing metadata and configuration parameters associated with a video conversion operation within the `coub-downloader` project. It serves as a data transfer object or state holder that exposes detailed technical specifications of a video stream, including codec information, dimensional properties, bitrate settings, and file size metrics, allowing consumers to inspect the characteristics of a media file before or after processing without exposing the underlying implementation details of the conversion engine.

## API

### `Format`
```csharp
public string? Format { get; }
```
Retrieves the container format of the video file (e.g., "mp4", "webm", "mkv"). This property may return `null` if the format has not been detected or if the source stream does not report a specific container type.

### `Width`
```csharp
public int Width { get; }
```
Gets the horizontal resolution of the video stream in pixels. This value represents the encoded width of the video frame.

### `Height`
```csharp
public int Height { get; }
```
Gets the vertical resolution of the video stream in pixels. This value represents the encoded height of the video frame.

### `Duration`
```csharp
public double Duration { get; }
```
Returns the total duration of the video in seconds as a floating-point number. This value includes fractional seconds for precise timing calculations.

### `VideoCodec`
```csharp
public string? VideoCodec { get; }
```
Identifies the video codec used for encoding the stream (e.g., "h264", "vp9", "av1"). Returns `null` if the codec is unknown or if the stream contains no video track.

### `AudioCodec`
```csharp
public string? AudioCodec { get; }
```
Identifies the audio codec used for encoding the stream (e.g., "aac", "opus", "mp3"). Returns `null` if the codec is unknown or if the stream contains no audio track.

### `VideoBitrate`
```csharp
public int VideoBitrate { get; }
```
Gets the target or actual bitrate of the video stream in bits per second (bps). A value of `0` may indicate a variable bitrate (VBR) stream where an average is not pre-calculated or if the information is unavailable.

### `AudioBitrate`
```csharp
public int AudioBitrate { get; }
```
Gets the target or actual bitrate of the audio stream in bits per second (bps). Similar to `VideoBitrate`, a value of `0` may signify unavailable data or variable bitrate encoding.

### `FrameRate`
```csharp
public int FrameRate { get; }
```
Returns the frame rate of the video stream in frames per second (fps). This is provided as an integer; fractional frame rates are typically rounded or represented by their numerator in specific contexts depending on the underlying parser.

### `HasAudio`
```csharp
public bool HasAudio { get; }
```
Indicates whether the video stream contains an audio track. Returns `true` if an audio stream is present and detectable, otherwise `false`.

### `FileSizeBytes`
```csharp
public long FileSizeBytes { get; }
```
Gets the total size of the media file in bytes. This property uses a 64-bit integer to support large video files exceeding 2GB.

## Usage

### Inspecting Media Metadata
The following example demonstrates how to retrieve and display key technical details from a conversion service instance to verify output specifications before archiving.

```csharp
public void LogVideoDetails(IVideoConversionService videoService)
{
    if (videoService == null) return;

    Console.WriteLine($"Container: {videoService.Format ?? "Unknown"}");
    Console.WriteLine($"Resolution: {videoService.Width}x{videoService.Height}");
    Console.WriteLine($"Duration: {videoService.Duration:F2} seconds");
    Console.WriteLine($"Codecs: Video={videoService.VideoCodec}, Audio={videoService.AudioCodec}");
    
    if (videoService.HasAudio)
    {
        Console.WriteLine($"Audio Bitrate: {videoService.AudioBitrate} bps");
    }
    
    Console.WriteLine($"Total Size: {videoService.FileSizeBytes / 1024.0 / 1024.0:F2} MB");
}
```

### Validating Conversion Requirements
This example illustrates a validation check to ensure a video meets specific criteria (such as minimum resolution and presence of audio) before proceeding with further processing steps.

```csharp
public bool IsValidForArchive(IVideoConversionService videoService)
{
    if (videoService == null) return false;

    // Require at least 720p resolution
    if (videoService.Width < 1280 || videoService.Height < 720)
    {
        return false;
    }

    // Require audio track and known video codec
    if (!videoService.HasAudio || string.IsNullOrEmpty(videoService.VideoCodec))
    {
        return false;
    }

    // Ensure file size is within reasonable limits (e.g., under 500MB)
    const long MaxSizeBytes = 500 * 1024 * 1024;
    if (videoService.FileSizeBytes > MaxSizeBytes)
    {
        return false;
    }

    return true;
}
```

## Notes

*   **Nullability**: Consumers must handle `null` values for `Format`, `VideoCodec`, and `AudioCodec`. These properties are nullable because certain streams may lack identifiable headers or may be processed in a state where metadata extraction has not yet completed or failed.
*   **Zero Values**: `VideoBitrate` and `AudioBitrate` may return `0`. This does not necessarily indicate an error but often signifies that the stream uses Variable Bitrate (VBR) encoding where a constant bitrate value is not applicable or available at the time of inspection.
*   **Integer Frame Rate**: The `FrameRate` property returns an `int`. Sources with fractional frame rates (e.g., 23.976 fps) will be truncated or rounded depending on the underlying implementation. Precision-critical applications should not rely solely on this integer representation for timing calculations.
*   **Thread Safety**: As an interface exposing primarily data properties, `IVideoConversionService` implementations are expected to be read-only snapshots of state. However, without explicit documentation on the concrete implementation, it should be assumed that accessing these properties is safe only if the underlying conversion process has completed. Modifying the state of the service during an active conversion operation from a separate thread may lead to inconsistent reads.
*   **File Size Limits**: The `FileSizeBytes` property utilizes a `long` (Int64), correctly supporting files larger than 4GB. Care should be taken when casting this value to `int` for legacy APIs to avoid overflow exceptions.
