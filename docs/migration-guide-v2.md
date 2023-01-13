# Migration Guide: v1.x to v2.0

This guide provides a comprehensive overview of the changes introduced in Coub Downloader v2.0 and instructions for migrating from v1.x versions.

## Overview

Coub Downloader v2.0 introduces a major new feature: **Video Editing Capabilities**. This release adds a complete video editing suite that allows users to trim, merge, apply effects, and preview videos non-destructively before final rendering.

## Breaking Changes

Good news! **There are no breaking changes** in v2.0. All existing APIs remain fully backward compatible. The v2.0 release is purely additive, introducing new video editing functionality without modifying or removing any existing features.

## New Features Overview

### Video Editing Suite

v2.0 adds a comprehensive video editing system with the following capabilities:

1. **Trimming**: Cut videos to specific time segments with keyframe-aligned or flexible trimming
2. **Merging**: Concatenate multiple video clips with various transition options
3. **Effects**: Apply visual and audio effects in customizable chains
4. **Preview Generation**: Create low-latency previews of edits before final rendering
5. **Session-Based Editing**: Non-destructive editing workflow with operation queuing

### Key Components Added

- `IVideoEditorService` - Main interface for video editing operations
- `VideoEditorService` - Implementation of the video editing service
- `VideoEditSession` - Represents an editing session with queued operations
- `VideoEditResult` - Describes the result of video editing operations
- `VideoEffect` and `VideoEditorEnums` - Effect definitions and configuration options
- `VideoEffectsProcessor` - Core processing engine for applying effects
- Preview system with configurable quality and region options

## Step-by-Step Migration from v1.x to v2.0

Since there are no breaking changes, migration is straightforward:

### 1. Update Package Reference

If using NuGet:
```bash
dotnet add package CoubDownloader --version 2.0.0
```

If using Docker:
```bash
# Pull the latest v2.0 image
docker pull sarmkadan/coub-downloader:latest
```

### 2. Verify Your Existing Code Still Works

All existing v1.x code will continue to work without modification:
```csharp
// This v1.x code works unchanged in v2.0
var services = new ServiceCollection();
services.AddCoubDownloaderServices();
var serviceProvider = services.BuildServiceProvider();

var downloadService = serviceProvider.GetRequiredService<ICoubDownloadService>();
var result = await downloadService.DownloadAsync("https://coub.com/view/2a3b4c5d");
```

### 3. Explore New Video Editing Features (Optional)

To take advantage of the new v2.0 video editing capabilities:

#### Basic Video Trimming
```csharp
var editorService = serviceProvider.GetRequiredService<IVideoEditorService>();

// Create an editing session
var session = await editorService.CreateSessionAsync("input.mp4");

// Trim the video (this creates an operation in the session)
var trimmedResult = await editorService.TrimVideoAsync(
    "input.mp4",
    "trimmed.mp4",
    TimeSpan.FromSeconds(10),   // Start at 10 seconds
    TimeSpan.FromSeconds(30),   // End at 30 seconds
    progress: new Progress<int>(percent => Console.WriteLine($"Trimming: {percent}%"))
);

// Apply the session to get the final result
var finalResult = await editorService.ApplySessionAsync(
    session,
    "final-output.mp4",
    new ConversionSettings { Quality = VideoQuality.High }
);
```

#### Merging Multiple Videos
```csharp
var editorService = serviceProvider.GetRequiredService<IVideoEditorService>();

var mergeOperation = new MergeOperation
{
    Strategy = MergeStrategy.Concatenate,
    Transition = new VideoTransition
    {
        Type = TransitionType.Fade,
        Duration = TimeSpan.FromSeconds(1)
    }
};

var mergeResult = await editorService.MergeVideosAsync(
    new List<string> { "clip1.mp4", "clip2.mp4", "clip3.mp4" },
    "merged-output.mp4",
    mergeOperation,
    progress: new Progress<int>(percent => Console.WriteLine($"Merging: {percent}%"))
);
```

#### Applying Effects
```csharp
var editorService = serviceProvider.GetRequiredService<IVideoEditorService>();

var effects = new List<VideoEffect>
{
    new VideoEffect { Type = EffectType.Brightness, Parameters = { ["level"] = 0.2 } },
    new VideoEffect { Type = EffectType.Contrast, Parameters = { ["level"] = 1.1 } },
    new VideoEffect { Type = EffectType.Saturation, Parameters = { ["level"] = 1.2 } }
};

var effectsResult = await editorService.ApplyEffectsAsync(
    "input.mp4",
    "effects-output.mp4",
    effects,
    progress: new Progress<int>(percent => Console.WriteLine($"Applying effects: {percent}%"))
);
```

#### Generating Previews
```csharp
var editorService = serviceProvider.GetRequiredService<IVideoEditorService>();

// Create session and add some operations
var session = await editorService.CreateSessionAsync("input.mp4");
await editorService.TrimVideoAsync(session, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15));

// Generate a preview to see what the edit will look like
var previewOptions = new PreviewOptions
{
    Quality = PreviewQuality.Medium,
    Duration = TimeSpan.FromSeconds(5),
    Region = new PreviewRegion { X = 0, Y = 0, Width = 320, Height = 240 }
};

var previewResult = await editorService.GeneratePreviewAsync(
    session,
    "preview.mp4",
    previewOptions
);
```

## Configuration Changes

v2.0 introduces no new required configuration options. All existing configuration from v1.x continues to work unchanged.

However, if you wish to customize the video editing behavior, you can optionally configure:

### New Optional Configuration Sections

```json
{
  "VideoEditor": {
    "TempFilePath": "/tmp/video-editor",
    "MaxConcurrentOperations": 4,
    "EnableHardwareAcceleration": true,
    "DefaultPreviewQuality": "Medium",
    "CachePreviews": true
  }
}
```

Or via environment variables:
```bash
COUB_VIDEOEDITOR_TEMP_PATH=/tmp/video-editor
COUB_VIDEOEDITOR_MAX_CONCURRENT_OPERATIONS=4
COUB_VIDEOEDITOR_ENABLE_HW_ACCELERATION=true
```

## Code Examples: Old vs New API

Since v2.0 is additive, there are no "old vs new" API changes for existing functionality. However, here are examples showing how to use the new v2.0 features alongside existing v1.x code:

### Before (v1.x only)
```csharp
// Download and convert a video
var downloadService = serviceProvider.GetRequiredService<ICoubDownloadService>();
var downloadResult = await downloadService.DownloadAsync(
    "https://coub.com/view/2a3b4c5d",
    new ConversionSettings { Format = VideoFormat.MP4, Quality = VideoQuality.High }
);

// The downloaded video is now ready to use
File.Copy(downloadResult.OutputPath, "final-video.mp4");
```

### After (v1.x + v2.0 features)
```csharp
// Download and convert a video (same as v1.x)
var downloadService = serviceProvider.GetRequiredService<ICoubDownloadService>();
var downloadResult = await downloadService.DownloadAsync(
    "https://coub.com/view/2a3b4c5d",
    new ConversionSettings { Format = VideoFormat.MP4, Quality = VideoQuality.High }
);

// NEW: Apply video editing to the downloaded file
var editorService = serviceProvider.GetRequiredService<IVideoEditorService();

// Trim to interesting segment (10-30 seconds)
var trimmedResult = await editorService.TrimVideoAsync(
    downloadResult.OutputPath,
    "trimmed-video.mp4",
    TimeSpan.FromSeconds(10),
    TimeSpan.FromSeconds(30)
);

// Add some visual effects
var effects = new List<VideoEffect>
{
    new VideoEffect { Type = EffectType.Brightness, Parameters = { ["level"] = 0.15 } },
    new VideoEffect { Type = EffectType.Vignette, Parameters = { ["intensity"] = 0.3 } }
};

var finalResult = await editorService.ApplyEffectsAsync(
    trimmedResult.OutputPath,
    "final-video.mp4",
    effects
);

// final-video.mp4 now contains the trimmed, enhanced video
```

## Docker Usage

v2.0 works with the same Docker images as v1.x, but now includes the video editing capabilities:

```bash
# Run with Docker (now includes video editing!)
docker run -it \
  -v /path/to/downloads:/downloads \
  -v /path/to/config:/app/config \
  sarmkadan/coub-downloader:latest \
  edit \
    --input /downloads/raw-video.mp4 \
    --output /downloads/edited-video.mp4 \
    --trim 00:00:10 00:00:30 \
    --effect brightness:0.2 contrast:1.1
```

## Testing Your Migration

After updating to v2.0, verify that:

1. All existing v1.x functionality works unchanged
2. New video editing features are accessible via `IVideoEditorService`
3. Docker containers run correctly with both download and edit commands
4. Any custom extensions or integrations still function properly

## Troubleshooting

### Issue: "Could not find type IVideoEditorService"
**Solution**: Ensure you've updated to CoubDownloader version 2.0.0 or later.

### Issue: Docker container fails on video editing operations
**Solution**: Make sure FFmpeg is properly installed in the container (it should be by default in v2.0 images).

### Issue: Performance degradation when using video editing features
**Solution**: Consider enabling hardware acceleration:
```bash
export COUB_VIDEOEDITOR_ENABLE_HW_ACCELERATION=true
export COUB_VIDEOEDITOR_GPU_CODEC=h264_nvenc  # For NVIDIA
```

## Support

For assistance with migration to v2.0:
- Check the [FAQ](../faq.md)
- Review the [API Reference](../api-reference.md)
- Visit [GitHub Discussions](https://github.com/sarmkadan/coub-downloader/discussions)
- Open an issue on [GitHub Issues](https://github.com/sarmkadan/coub-downloader/issues)

---

*Last updated: 2026-05-18*