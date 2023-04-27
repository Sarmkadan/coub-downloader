# CoubVideoExtensions

Provides a set of static utility methods for analyzing and processing Coub video metadata, calculating durations, estimating output sizes, and generating FFmpeg-compatible parameters. This class centralizes logic for format detection, audio specification, progress tracking, and quality assessment used throughout the download and conversion pipeline.

## API

### GetAspectRatio
```csharp
public static double GetAspectRatio(int width, int height)
```
Returns the width divided by height as a `double`. Used to determine display proportions for layout and format decisions. Does not throw.

### IsVerticalFormat
```csharp
public static bool IsVerticalFormat(int width, int height)
```
Returns `true` if `height` is greater than `width`, indicating a vertical (portrait) video. Does not throw.

### IsHdQuality
```csharp
public static bool IsHdQuality(int width, int height)
```
Returns `true` if the resolution meets or exceeds 1280×720 (HD). Does not throw.

### Is4kQuality
```csharp
public static bool Is4kQuality(int width, int height)
```
Returns `true` if the resolution meets or exceeds 3840×2160 (4K UHD). Does not throw.

### CalculateRequiredAudioDuration
```csharp
public static double CalculateRequiredAudioDuration(double videoDuration, double loopCount)
```
Computes the total audio duration needed to cover a video looped `loopCount` times. Returns `videoDuration * loopCount`. Does not throw.

### GetDurationCategory
```csharp
public static string GetDurationCategory(double durationSeconds)
```
Classifies a duration into a human-readable category string (e.g., "Short", "Medium", "Long") based on predefined thresholds. Returns `"Unknown"` for negative values. Does not throw.

### GetFormattedViewCount
```csharp
public static string GetFormattedViewCount(long viewCount)
```
Formats a view count with appropriate suffixes (K, M, B) and one decimal place when applicable. Returns the formatted string. Does not throw.

### GetAudioSpec
```csharp
public static string GetAudioSpec(int channels, int sampleRate, int bitrate)
```
Generates a concise audio specification string (e.g., "stereo 44100Hz 192kbps"). Returns `"unknown"` if parameters are zero or negative. Does not throw.

### CalculateLoopedDuration
```csharp
public static double CalculateLoopedDuration(double videoDuration, double loopCount)
```
Returns the total playback duration when a video of `videoDuration` is looped `loopCount` times. Equivalent to `videoDuration * loopCount`. Does not throw.

### IsLossless
```csharp
public static bool IsLossless(string codecName)
```
Returns `true` if the provided codec name corresponds to a lossless audio format (e.g., FLAC, ALAC, WAV). Comparison is case-insensitive. Does not throw.

### IsStereo
```csharp
public static bool IsStereo(int channels)
```
Returns `true` if `channels` equals 2. Does not throw.

### IsMono
```csharp
public static bool IsMono(int channels)
```
Returns `true` if `channels` equals 1. Does not throw.

### IsSurround
```csharp
public static bool IsSurround(int channels)
```
Returns `true` if `channels` is greater than 2, indicating multi-channel surround audio. Does not throw.

### GetFFmpegCodecParams
```csharp
public static string GetFFmpegCodecParams(string format, int bitrate, int channels, int sampleRate)
```
Builds and returns a string of FFmpeg codec parameters appropriate for the given output format, bitrate, channel count, and sample rate. Returns `string.Empty` if the format is unrecognized. Does not throw.

### GetTotalBitrate
```csharp
public static int GetTotalBitrate(int videoBitrate, int audioBitrate)
```
Returns the sum of `videoBitrate` and `audioBitrate`. Does not throw.

### ShouldUseHardwareAcceleration
```csharp
public static bool ShouldUseHardwareAcceleration(string codecName, int width, int height)
```
Determines whether hardware acceleration should be employed based on the codec and resolution. Returns `true` for high-resolution content with supported codecs. Does not throw.

### EstimateOutputSize
```csharp
public static long EstimateOutputSize(int totalBitrate, double durationSeconds)
```
Estimates the output file size in bytes given a total bitrate (in bits per second) and duration. Returns `0` for non-positive inputs. Does not throw.

### GetProgressPercent
```csharp
public static int GetProgressPercent(long bytesDownloaded, long totalBytes)
```
Calculates download progress as an integer percentage (0–100). Returns `0` if `totalBytes` is zero or negative; clamps to 100 if `bytesDownloaded` exceeds `totalBytes`. Does not throw.

### IsCompleted
```csharp
public static bool IsCompleted(long bytesDownloaded, long totalBytes)
```
Returns `true` if `bytesDownloaded` is greater than or equal to `totalBytes` and `totalBytes` is positive. Does not throw.

### GetEstimatedTimeRemaining
```csharp
public static TimeSpan? GetEstimatedTimeRemaining(long bytesDownloaded, long totalBytes, double elapsedSeconds)
```
Estimates the remaining download time based on current throughput. Returns `null` if `bytesDownloaded` is zero or negative, or if `elapsedSeconds` is zero. Does not throw.

## Usage

### Example 1: Analyzing a Coub Before Download
```csharp
int width = 1920;
int height = 1080;
double videoDuration = 10.5;
int audioChannels = 2;
int audioSampleRate = 44100;
int audioBitrate = 192000;

bool isHd = CoubVideoExtensions.IsHdQuality(width, height);
bool is4k = CoubVideoExtensions.Is4kQuality(width, height);
double aspect = CoubVideoExtensions.GetAspectRatio(width, height);
string audioSpec = CoubVideoExtensions.GetAudioSpec(audioChannels, audioSampleRate, audioBitrate);
bool isStereo = CoubVideoExtensions.IsStereo(audioChannels);

Console.WriteLine($"HD: {isHd}, 4K: {is4k}, Aspect: {aspect:F2}");
Console.WriteLine($"Audio: {audioSpec}, Stereo: {isStereo}");
```

### Example 2: Estimating Output and Tracking Progress
```csharp
int videoBitrate = 2500000;
int audioBitrate = 192000;
double loopCount = 3;
double videoDuration = 12.0;
long totalBytes = 50_000_000;
long downloaded = 35_000_000;
double elapsed = 15.0;

int totalBitrate = CoubVideoExtensions.GetTotalBitrate(videoBitrate, audioBitrate);
double loopedDuration = CoubVideoExtensions.CalculateLoopedDuration(videoDuration, loopCount);
long estimatedSize = CoubVideoExtensions.EstimateOutputSize(totalBitrate, loopedDuration);
int progress = CoubVideoExtensions.GetProgressPercent(downloaded, totalBytes);
TimeSpan? eta = CoubVideoExtensions.GetEstimatedTimeRemaining(downloaded, totalBytes, elapsed);

Console.WriteLine($"Estimated output: {estimatedSize} bytes");
Console.WriteLine($"Progress: {progress}%");
Console.WriteLine($"ETA: {(eta.HasValue ? eta.Value.ToString() : "calculating...")}");
```

## Notes

- All methods are static and stateless; they are safe to call concurrently from multiple threads without synchronization.
- Methods accepting numeric inputs do not throw exceptions for negative or zero values—they return sensible defaults (zero, `false`, `null`, or placeholder strings).
- `GetDurationCategory` relies on internal thresholds that may change between versions; do not hardcode expected category strings in persistent logic.
- `GetFFmpegCodecParams` returns `string.Empty` for unrecognized formats; callers should validate the result before passing it to FFmpeg command builders.
- `EstimateOutputSize` provides a rough approximation based solely on bitrate and duration; actual file size may vary due to container overhead and variable bitrate encoding.
- `GetEstimatedTimeRemaining` returns `null` when there is insufficient data to compute a meaningful estimate (no bytes downloaded yet or zero elapsed time). Callers must handle the nullable result.
- `IsLossless` performs case-insensitive matching against a fixed set of known lossless codecs; custom or obscure codec names may not be recognized.
