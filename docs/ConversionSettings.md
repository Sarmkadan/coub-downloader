# ConversionSettings

The `ConversionSettings` type encapsulates all user‑configurable options that drive the FFmpeg‑based conversion of a Coub video into a target file. It aggregates codec selection, bitrate, resolution, frame‑rate, threading, and post‑processing flags into a single immutable‑ish record that can be passed to the conversion pipeline.

## API

### Id
- **Purpose:** A unique identifier for the settings instance, useful for logging or caching presets.
- **Parameters:** None.
- **Return:** `string` – the identifier value.
- **Throws:** Does not throw.

### Format
- **Purpose:** Specifies the output container format (e.g., MP4, WebM) that FFmpeg will use.
- **Parameters:** None.
- **Return:** `VideoFormat` – the selected container format.
- **Throws:** Does not throw.

### Quality
- **Purpose:** Defines a quality preset that influences default bitrate and encoder tuning.
- **Parameters:** None.
- **Return:** `VideoQuality` – the quality level.
- **Throws:** Does not throw.

### VideoBitrate
- **Purpose:** Sets the target video bitrate in kilobits per second.
- **Parameters:** None.
- **Return:** `int` – the video bitrate value.
- **Throws:** Does not throw; however, supplying a value ≤ 0 may cause FFmpeg to fail when `GetFFmpegCodecParams` is invoked.

### AudioBitrate
- **Purpose:** Sets the target audio bitrate in kilobits per second.
- **Parameters:** None.
- **Return:** `int` – the audio bitrate value.
- **Throws:** Does not throw; non‑positive values may lead to FFmpeg errors.

### VideoCodec
- **Purpose:** Names the video encoder to be used by FFmpeg (e.g., `libx264`, `libx265`).
- **Parameters:** None.
- **Return:** `string` – the codec identifier.
- **Throws:** Does not throw; an empty or null string will cause `GetFFmpegCodecParams` to produce invalid arguments.

### AudioCodec
- **Purpose:** Names the audio encoder to be used by FFmpeg (e.g., `aac`, `opus`).
- **Parameters:** None.
- **Return:** `string` – the codec identifier.
- **Throws:** Does not throw; an empty or null string will cause `GetFFmpegCodecParams` to produce invalid arguments.

### FrameRate
- **Purpose:** Specifies the output frame rate in frames per second.
- **Parameters:** None.
- **Return:** `int` – the frame rate.
- **Throws:** Does not throw; values ≤ 0 are invalid for FFmpeg.

### Width
- **Purpose:** Defines the output video width in pixels.
- **Parameters:** None.
- **Return:** `int` – the width.
- **Throws:** Does not throw; a width of 0 or negative will result in an FFmpeg error.

### Height
- **Purpose:** Defines the output video height in pixels.
- **Parameters:** None.
- **Return:** `int` – the height.
- **Throws:** Does not throw; a height of 0 or negative will result in an FFmpeg error.

### AudioLoopStrategy
- **Purpose:** Determines how the audio track is looped or trimmed to match the video duration.
- **Parameters:** None.
- **Return:** `AudioLoopStrategy` – the selected looping behavior.
- **Throws:** Does not throw.

### PreserveAspectRatio
- **Purpose:** Indicates whether the output should retain the source aspect ratio; if true, scaling may add padding to meet the requested width/height.
- **Parameters:** None.
- **Return:** `bool` – true to preserve aspect ratio, false to stretch.
- **Throws:** Does not throw.

### EnableHardwareAcceleration
- **Purpose:** When true, instructs FFmpeg to use a hardware‑accelerated encoder (e.g., NVENC, QSV) if available.
- **Parameters:** None.
- **Return:** `bool` – true to enable hardware acceleration.
- **Throws:** Does not throw.

### UseMultiThreading
- **Purpose:** Controls whether FFmpeg is allowed to spawn multiple threads for encoding.
- **Parameters:** None.
- **Return:** `bool` – true to enable multithreading.
- **Throws:** Does not throw.

### ThreadCount
- **Purpose:** Sets the maximum number of threads FFmpeg may use when `UseMultiThreading` is true.
- **Parameters:** None.
- **Return:** `int` – the thread count.
- **Throws:** Does not throw; if `UseMultiThreading` is false, this value is ignored.

### ApplyFades
- **Purpose:** Specifies whether fade‑in and fade‑out effects should be applied to the output.
- **Parameters:** None.
- **Return:** `bool` – true to apply fades.
- **Throws:** Does not throw.

### FadeInMs
- **Purpose:** Duration of the fade‑in effect in milliseconds.
- **Parameters:** None.
- **Return:** `int` – fade‑in length.
- **Throws:** Does not throw; values < 0 are treated as zero.

### FadeOutMs
- **Purpose:** Duration of the fade‑out effect in milliseconds.
- **Parameters:** None.
- **Return:** `int` – fade‑out length.
- **Throws:** Does not throw; values < 0 are treated as zero.

### CreatedAt
- **Purpose:** Timestamp indicating when the `ConversionSettings` instance was instantiated.
- **Parameters:** None.
- **Return:** `DateTime` – the creation time (UTC).
- **Throws:** Does not throw.

### GetFFmpegCodecParams
- **Purpose:** Generates a string containing the FFmpeg codec‑related arguments derived from the current settings (e.g., `-c:v libx264 -b:v 2000k -c:a aac -b:a 128k`).
- **Parameters:** None.
- **Return:** `string` – ready‑to‑append FFmpeg arguments.
- **Throws:** `InvalidOperationException` if essential fields are missing or invalid (e.g., `VideoCodec` or `AudioCodec` is null/empty, or bitrates are ≤ 0).

## Usage

```csharp
using CoubDownloader.Conversion;

// Create a preset for high‑quality MP4 output with hardware acceleration.
var settings = new ConversionSettings
{
    Id = "yt-high",
    Format = VideoFormat.Mp4,
    Quality = VideoQuality.High,
    VideoBitrate = 5000,
    AudioBitrate = 192,
    VideoCodec = "libx264",
    AudioCodec = "aac",
    FrameRate = 30,
    Width = 1920,
    Height = 1080,
    AudioLoopStrategy = AudioLoopStrategy.Loop,
    PreserveAspectRatio = true,
    EnableHardwareAcceleration = true,
    UseMultiThreading = true,
    ThreadCount = 4,
    ApplyFades = true,
    FadeInMs = 500,
    FadeOutMs = 500,
    // CreatedAt is set automatically by the constructor.
};

string ffmpegArgs = settings.GetFFmpegCodecParams();
// ffmpegArgs might be: "-c:v libx264 -b:v 5000k -maxrate 5000k -bufsize 10000k -c:a aac -b:a 192k"
```

```csharp
using CoubDownloader.Conversion;
using CoubDownloader.Pipeline;

// Example of using ConversionSettings within a conversion job.
public async Task ConvertCoubAsync(string coubUrl, string outputPath)
{
    var downloader = new CoubDownloader();
    var videoInfo = await downloader.FetchInfoAsync(coubUrl);

    var settings = new ConversionSettings
    {
        Id = $"coub-{videoInfo.Id}",
        Format = VideoFormat.Webm,
        Quality = VideoQuality.Medium,
        VideoBitrate = 2500,
        AudioBitrate = 128,
        VideoCodec = "libvpx-vp9",
        AudioCodec = "libopus",
        FrameRate = videoInfo.FrameRate,
        Width = 1280,
        Height = 720,
        AudioLoopStrategy = AudioLoopStrategy.TrimToFit,
        PreserveAspectRatio = false,
        EnableHardwareAcceleration = false,
        UseMultiThreading = true,
        ThreadCount = Environment.ProcessorCount,
        ApplyFades = false,
        FadeInMs = 0,
        FadeOutMs = 0,
    };

    var converter = new FFmpegConverter(settings);
    await converter.ConvertAsync(videoInfo.StreamUrl, outputPath);
}
```

## Notes

- **Validation:** The type does not enforce constraints on numeric properties; invalid values (zero or negative bitrates, dimensions, or frame rates) will not throw until `GetFFmpegCodecParams` is called, at which point an `InvalidOperationException` may be raised.
- **Thread‑safety:** Instances are safe to read from multiple threads after construction, but mutating any property while another thread is reading it or invoking `GetFFmpegCodecParams` leads to undefined behavior. Treat the object as effectively immutable after initialization for concurrent use.
- **Interaction of properties:** When `PreserveAspectRatio` is true, the encoder will scale the source to fit within the specified `Width` and `Height`, adding padding if necessary. If false, the source is stretched to exactly match those dimensions.
- **Hardware acceleration:** Setting `EnableHardwareAcceleration` to true does not guarantee that a compatible encoder is available; FFmpeg will fall back to software encoding if the requested hardware codec is not present, without throwing an exception.
- **Thread count:** The `ThreadCount` value is only consulted when `UseMultiThreading` is true; otherwise FFmpeg defaults to a single thread regardless of this setting.
- **Fade effects:** `ApplyFades` must be true for `FadeInMs` and `FadeOutMs` to be considered; if false, the fade durations are ignored even if they are non‑zero.
- **CreatedAt:** This property reflects the instant the object was constructed and is not updated if the instance is later modified. It is useful for audit trails or caching invalidation policies.
