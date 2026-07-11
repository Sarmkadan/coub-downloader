# ApplicationStartup

`ApplicationStartup` is the central configuration and lifecycle manager for the coub-downloader application. It exposes both direct property setters and a fluent builder interface (via the `With*` methods) to specify logging, download, configuration, and FFmpeg paths, as well as verbose logging. After configuration, `InitializeAsync` starts the application with the current settings, and `ShutdownAsync` gracefully stops it. The builder methods return a `StartupConfigurationBuilder` that can be used to chain calls and finalize the configuration with `Build`.

## API

### `public ApplicationStartup()`

Initializes a new instance of `ApplicationStartup` with default values. All configuration properties are initially `null` or `false`.

### `public async Task InitializeAsync()`

Starts the application using the current configuration values. This method should be called once after all desired settings have been applied.  
**Throws:**  
- `InvalidOperationException` if required paths (e.g., `DownloadDirectory`) are not set.  
- `FileNotFoundException` if the FFmpeg path points to a non‑existent file.  
- Any exception thrown by underlying I/O or process initialization.

### `public async Task ShutdownAsync()`

Gracefully stops the application and releases any held resources. Safe to call even if `InitializeAsync` was not invoked.  
**Throws:**  
- `ObjectDisposedException` if the instance has already been disposed.

### `public StartupConfigurationBuilder WithLoggingDirectory(string path)`

Sets the directory where log files will be written. Returns a `StartupConfigurationBuilder` that can be used for further chaining.  
**Parameters:**  
- `path` – Absolute or relative path to the logging directory.  
**Returns:** A `StartupConfigurationBuilder` instance reflecting the current configuration.  
**Throws:**  
- `ArgumentNullException` if `path` is `null`.  
- `ArgumentException` if `path` is empty or contains invalid characters.

### `public StartupConfigurationBuilder WithDownloadDirectory(string path)`

Sets the directory where downloaded content will be saved. Returns a `StartupConfigurationBuilder` for chaining.  
**Parameters:**  
- `path` – Absolute or relative path to the download directory.  
**Returns:** A `StartupConfigurationBuilder` instance.  
**Throws:**  
- `ArgumentNullException` if `path` is `null`.  
- `ArgumentException` if `path` is empty or invalid.

### `public StartupConfigurationBuilder WithConfigFile(string path)`

Sets the path to the application configuration file. Returns a `StartupConfigurationBuilder` for chaining.  
**Parameters:**  
- `path` – Absolute or relative path to the configuration file.  
**Returns:** A `StartupConfigurationBuilder` instance.  
**Throws:**  
- `ArgumentNullException` if `path` is `null`.  
- `ArgumentException` if `path` is empty or invalid.

### `public StartupConfigurationBuilder WithFFmpegPath(string path)`

Sets the path to the FFmpeg executable. Returns a `StartupConfigurationBuilder` for chaining.  
**Parameters:**  
- `path` – Absolute or relative path to the FFmpeg binary.  
**Returns:** A `StartupConfigurationBuilder` instance.  
**Throws:**  
- `ArgumentNullException` if `path` is `null`.  
- `ArgumentException` if `path` is empty or invalid.

### `public StartupConfigurationBuilder EnableVerboseLogging()`

Enables verbose (debug‑level) logging. Returns a `StartupConfigurationBuilder` for chaining.  
**Returns:** A `StartupConfigurationBuilder` instance.

### `public StartupConfigurationBuilder Build()`

Finalizes the configuration and returns the underlying `StartupConfigurationBuilder` that holds the current settings. After calling `Build`, the configuration properties on the `ApplicationStartup` instance are updated to match the builder’s values.  
**Returns:** A `StartupConfigurationBuilder` containing the finalized configuration.  
**Throws:**  
- `InvalidOperationException` if required settings are missing (e.g., no download directory specified).

### `public string LoggingDirectory { get; set; }`

Gets or sets the logging directory. Setting this property directly is equivalent to calling `WithLoggingDirectory`.  
**Throws:**  
- `ArgumentNullException` on set if value is `null`.  
- `ArgumentException` on set if value is empty or invalid.

### `public string DownloadDirectory { get; set; }`

Gets or sets the download directory. Setting this property directly is equivalent to calling `WithDownloadDirectory`.  
**Throws:**  
- `ArgumentNullException` on set if value is `null`.  
- `ArgumentException` on set if value is empty or invalid.

### `public string ConfigFilePath { get; set; }`

Gets or sets the configuration file path. Setting this property directly is equivalent to calling `WithConfigFile`.  
**Throws:**  
- `ArgumentNullException` on set if value is `null`.  
- `ArgumentException` on set if value is empty or invalid.

### `public string FFmpegPath { get; set; }`

Gets or sets the FFmpeg executable path. Setting this property directly is equivalent to calling `WithFFmpegPath`.  
**Throws:**  
- `ArgumentNullException` on set if value is `null`.  
- `ArgumentException` on set if value is empty or invalid.

### `public bool VerboseLogging { get; set; }`

Gets or sets whether verbose logging is enabled. Setting this property to `true` is equivalent to calling `EnableVerboseLogging`.

## Usage

### Example 1: Direct property assignment

```csharp
var startup = new ApplicationStartup
{
    DownloadDirectory = @"/data/downloads",
    LoggingDirectory  = @"/var/log/coub",
    ConfigFilePath    = @"/etc/coub/config.json",
    FFmpegPath        = @"/usr/bin/ffmpeg",
    VerboseLogging    = true
};

await startup.InitializeAsync();
// ... application runs ...
await startup.ShutdownAsync();
```

### Example 2: Fluent builder pattern

```csharp
var startup = new ApplicationStartup();

startup
    .WithDownloadDirectory(@"./downloads")
    .WithLoggingDirectory(@"./logs")
    .WithConfigFile(@"./config.json")
    .WithFFmpegPath(@"./tools/ffmpeg.exe")
    .EnableVerboseLogging()
    .Build();

await startup.InitializeAsync();
// ... application runs ...
await startup.ShutdownAsync();
```

## Notes

- **Thread safety:** `ApplicationStartup` is not thread‑safe. All configuration changes (property sets or builder calls) must be performed from a single thread before calling `InitializeAsync`. Concurrent access to properties or methods may result in undefined behaviour.
- **Required settings:** `DownloadDirectory` must be set before `InitializeAsync` is called; otherwise an `InvalidOperationException` is thrown. Other paths are optional, but missing logging or config paths may cause fallback to default behaviours (e.g., console‑only logging).
- **Builder vs. properties:** The `With*` methods and the corresponding properties are interchangeable. Mixing both approaches is allowed, but the last write wins. Calling `Build()` updates the `ApplicationStartup` properties to match the builder’s state.
- **Multiple calls:** `InitializeAsync` should be called only once. Calling it a second time without an intervening `ShutdownAsync` may throw an `InvalidOperationException`. `ShutdownAsync` is idempotent and can be called multiple times.
- **Path validation:** All path setters validate that the provided string is not null or empty. They do not verify that the path exists at configuration time; existence is checked during `InitializeAsync`.
- **Disposal:** The class does not implement `IDisposable`. Resource cleanup is handled by `ShutdownAsync`.
