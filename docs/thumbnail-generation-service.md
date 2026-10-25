# ThumbnailGenerationService

`ThumbnailGenerationService` (namespace `CoubDownloader.Application.Services`) generates still-image thumbnails and contact sheets from video files. It does not run FFmpeg itself: every operation builds an argument list and passes it to an injected `IFFmpegWrapper`, and media duration is read through the same wrapper's `GetMediaInfoAsync`.

Source: `Application/Services/ThumbnailGenerationService.cs`

> There is no `IThumbnailGenerationService` interface and no DI registration for this class in the repository at the time of writing. Create it directly with an `IFFmpegWrapper`, or register it yourself.

## Constructor

### `public ThumbnailGenerationService(IFFmpegWrapper ffmpeg)`

Creates the service.

- **Parameters**
  - `ffmpeg`: The FFmpeg wrapper used to execute commands and probe media. Must not be `null`.
- **Throws**: `ArgumentNullException` if `ffmpeg` is `null`.

## Methods

All methods are `async`, return the `outputPath` argument on success, and accept an optional `CancellationToken`. See [Cancellation](#cancellation) for what the token actually does.

### `GenerateThumbnailAsync`

```csharp
public async Task<string> GenerateThumbnailAsync(
    string videoPath,
    string outputPath,
    TimeSpan timestamp,
    int? maxWidth = null,
    CancellationToken cancellationToken = default)
```

Extracts a single frame at `timestamp`. The output format is inferred by FFmpeg from the `outputPath` extension (for example `.jpg` or `.png`).

- **Parameters**
  - `videoPath`: Path to the input video. Must exist.
  - `outputPath`: Path of the image to write. An existing file is overwritten (`-y`).
  - `timestamp`: Position in the video to capture. Must not be negative.
  - `maxWidth`: Optional output width in pixels. When set, the frame is scaled to that width with the height computed as `-2` (keeps aspect ratio, keeps an even value). When `null`, the frame keeps its source size.
  - `cancellationToken`: Accepted but not forwarded (see [Cancellation](#cancellation)).
- **Returns**: `outputPath`.
- **Throws**
  - `ArgumentOutOfRangeException` if `timestamp` is negative. This check runs before the file check.
  - `FileNotFoundException` if `videoPath` does not exist.
  - `InvalidOperationException` if FFmpeg reports failure. The message is `Failed to generate thumbnail: ` followed by FFmpeg's error output.

The command is built as: `-ss <seconds, 3 decimals> -i <videoPath> -frames:v 1 [-vf scale=<maxWidth>:-2] -y <outputPath>`. Seeking happens before `-i` (fast input seek).

### `GenerateThumbnailAtPercentAsync`

```csharp
public async Task<string> GenerateThumbnailAtPercentAsync(
    string videoPath,
    string outputPath,
    double percent,
    int? maxWidth = null,
    CancellationToken cancellationToken = default)
```

Resolves a percentage of the video's duration to a timestamp and calls `GenerateThumbnailAsync`.

- **Parameters**
  - `videoPath`, `outputPath`, `maxWidth`: As for `GenerateThumbnailAsync`.
  - `percent`: Position as a percentage of duration, inclusive range `0`–`100`.
  - `cancellationToken`: Accepted but not forwarded.
- **Returns**: `outputPath`.
- **Throws**
  - `ArgumentOutOfRangeException` if `percent` is less than 0 or greater than 100.
  - `FileNotFoundException` if `videoPath` does not exist.
  - `InvalidOperationException` with message `Could not determine video duration` if `GetMediaInfoAsync` returns `null` or no duration.
  - Any exception thrown by `GenerateThumbnailAsync`.

Timestamp = `duration × percent / 100`. Note that `percent = 100` yields a timestamp equal to the full duration, which may be past the last decodable frame; FFmpeg may then fail and the call throws `InvalidOperationException`. Prefer a value slightly below 100 if you need a frame near the end.

### `GenerateContactSheetAsync`

```csharp
public async Task<string> GenerateContactSheetAsync(
    string videoPath,
    string outputPath,
    int columns = 4,
    int rows = 4,
    int tileWidth = 320,
    CancellationToken cancellationToken = default)
```

Intended to produce a `columns` × `rows` grid of frames sampled across the video. Each tile is scaled to `tileWidth` pixels wide.

- **Parameters**
  - `videoPath`: Path to the input video. Must exist.
  - `outputPath`: Path of the image to write (overwritten, `-y`).
  - `columns`, `rows`: Grid size. Both must be greater than zero. Default `4`×`4`.
  - `tileWidth`: Width of each tile in pixels. Must be greater than zero. Default `320`.
  - `cancellationToken`: Accepted but not used.
- **Returns**: `outputPath`.
- **Throws**
  - `ArgumentOutOfRangeException` if `columns` or `rows` is not positive, or if `tileWidth` is not positive.
  - `FileNotFoundException` if `videoPath` does not exist.
  - `InvalidOperationException` if the duration cannot be read, or if FFmpeg fails (`Failed to generate contact sheet: ...`).

> **Known issue: this method does not work as documented.** Found by reading the code and reproducing the filter graph with the local `ffmpeg` binary:
>
> 1. **Invalid filter graph.** The generated `-filter_complex` references input streams `[0:v]` through `[N-1:v]`, but only one input (`-i videoPath`) is supplied. The tile inputs are also emitted as `[0:v][1:v]…` followed by the `[vN]` labels, which does not form a valid graph. A two-tile equivalent (`[0:v]select=…[v0];[1:v]select=…[v1];[0:v][1:v][v0][v1]tile=layout=2x1[outv]`) fails in FFmpeg with `Error linking filters` / `Invalid argument`. Expect `InvalidOperationException` for any grid with more than one tile.
> 2. **Not evenly sampled.** Each tile selects frame number `n == i` (the first `N` frames of the video), not a frame at `i × duration / (N − 1)`. The `frameInterval` and `frameTime` values computed in the method are never used.
>
> Fixing either point changes behavior and is out of scope for this document. Until fixed, do not rely on this method.

## Cancellation

The `cancellationToken` parameter is part of every public signature, but the current implementation does not honor it:

- `GenerateThumbnailAsync` does not pass the token to `IFFmpegWrapper.ExecuteAsync` (which takes no token, only an optional `TimeSpan? timeout`).
- `GenerateThumbnailAtPercentAsync` does not forward the token to `GenerateThumbnailAsync` or to `GetMediaInfoAsync`.
- `GenerateContactSheetAsync` ignores it entirely.

A cancelled token therefore has no effect on the FFmpeg process. Do not assume these calls can be cancelled mid-run.

## Usage

### Basic setup

```csharp
using CoubDownloader.Application.Services;
using CoubDownloader.Infrastructure.Integration;

IFFmpegWrapper ffmpeg = new FFmpegWrapper();
var thumbnails = new ThumbnailGenerationService(ffmpeg);
```

### Frame at a fixed time, scaled to 640 px wide

```csharp
string thumb = await thumbnails.GenerateThumbnailAsync(
    videoPath: "/data/coubs/clip.mp4",
    outputPath: "/data/thumbs/clip-5s.jpg",
    timestamp: TimeSpan.FromSeconds(5),
    maxWidth: 640);
```

### Frame at the middle of the video

```csharp
string thumb = await thumbnails.GenerateThumbnailAtPercentAsync(
    videoPath: "/data/coubs/clip.mp4",
    outputPath: "/data/thumbs/clip-mid.png",
    percent: 50);
```

### Handling errors

```csharp
try
{
    await thumbnails.GenerateThumbnailAtPercentAsync(videoPath, outputPath, percent: 10);
}
catch (FileNotFoundException)
{
    // input video is missing
}
catch (InvalidOperationException ex)
{
    // duration unavailable, or FFmpeg failed; ex.Message contains FFmpeg's error text
}
```

## Notes

- Output files are overwritten without warning.
- The output directory must already exist; the service does not create it.
- The service performs no validation of `maxWidth` (zero or negative values are passed straight to FFmpeg).
- `FFmpegWrapper` and its `FFmpegResult` / `MediaInfo` types are documented in `FFmpegWrapper.md`.
