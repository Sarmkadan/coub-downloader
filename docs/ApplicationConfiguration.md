# ApplicationConfiguration

Central configuration container for the Coub Downloader application, aggregating settings for downloads, media conversion, caching, API access, and logging. Designed to be serialized from and to JSON for persistence and user customization.

## API

### `Download`
Download-specific configuration settings.
Type: `DownloadSettings`
Default: instance with default values.
Used to control network behavior and retry logic during media acquisition.

### `Conversion`
Media conversion-specific configuration settings.
Type: `ConversionSettings`
Default: instance with default values.
Determines codec choices, quality levels, and conversion timeouts.

### `Cache`
Caching behavior and storage configuration.
Type: `CacheSettings`
Default: instance with default values.
Govern retention periods and directory paths for temporary and persistent cache.

### `Logging`
Logging level and output configuration.
Type: `LoggingSettings`
Default: instance with default values.
Controls verbosity and target destinations for diagnostic and operational logs.

### `Api`
Third-party API access configuration.
Type: `ApiSettings`
Default: instance with default values.
Holds credentials, rate limits, and endpoint URLs for external service integration.

### `OutputDirectory`
Absolute or relative filesystem path where downloaded and converted files are stored.
Type: `string`
Default: `null` (resolved to application working directory).
Must be writable; trailing slashes are normalized.

### `MaxConcurrentDownloads`
Maximum number of simultaneous download operations.
Type: `int`
Default: `4`
Must be positive; influences memory and network bandwidth usage.

### `TimeoutSeconds`
Global timeout for network-bound operations in seconds.
Type: `int`
Default: `30`
Must be non-negative; zero implies no timeout.

### `MaxRetries`
Maximum number of retry attempts for failed operations.
Type: `int`
Default: `3`
Must be non-negative; zero disables retries.

### `VerifyFileIntegrity`
Enables checksum validation of downloaded files.
Type: `bool`
Default: `true`
When `true`, throws `InvalidOperationException` if computed hash mismatches expected value.

### `MaxFileSizeBytes`
Upper bound on acceptable file size in bytes.
Type: `long`
Default: `5368709120` (5 GiB)
Zero or negative disables limit; positive values prevent oversized downloads.

### `FfmpegPath`
Filesystem path to the FFmpeg executable.
Type: `string`
Default: `null` (resolved via PATH environment variable).
Must point to a valid executable; otherwise conversion fails.

### `EnableHardwareAcceleration`
Enables hardware-accelerated video encoding when supported.
Type: `bool`
Default: `false`
When `true`, codec selection may implicitly switch to hardware-optimized variants.

### `MaxConcurrentConversions`
Maximum number of simultaneous media conversion operations.
Type: `int`
Default: `2`
Must be positive; influences CPU and GPU utilization.

### `VideoCodec`
Preferred video codec for conversion.
Type: `string`
Default: `"libx264"`
Must be a valid FFmpeg encoder name; empty string disables video encoding.

### `AudioCodec`
Preferred audio codec for conversion.
Type: `string`
Default: `"aac"`
Must be a valid FFmpeg encoder name; empty string disables audio encoding.

### `DefaultQuality`
Default quality preset for conversion.
Type: `int`
Default: `23`
Must be within codec-specific range (e.g., CRF 0–51 for libx264); out-of-range values are clamped.

### `Enabled`
Global toggle for the application’s core functionality.
Type: `bool`
Default: `true`
When `false`, suppresses download and conversion operations; does not affect API calls.

### `DefaultTtlSeconds`
Default time-to-live for cached items in seconds.
Type: `int`
Default: `86400` (24 hours)
Must be non-negative; zero implies permanent retention.
