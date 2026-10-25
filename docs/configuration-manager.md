# ConfigurationManager

`Infrastructure/Configuration/ConfigurationManager.cs` contains two types:

- `ApplicationConfiguration` and its section classes (`DownloadSettings`, `ConversionSettings`, `CacheSettings`, `LoggingSettings`, `ApiSettings`), a plain settings tree with in-code defaults.
- `ConfigurationManager`, which loads that tree from a JSON file, falls back to defaults, and saves it back.

This document describes which sources `ConfigurationManager` reads and in what order they take effect. The behavior described here was checked against the code and by running the class on sample inputs.

## Configuration sources

`ConfigurationManager` reads exactly two sources:

| Source | Used when | Notes |
|---|---|---|
| Built-in defaults (property initializers, e.g. `MaxRetries = 3`) | Always, as the base layer | Also used for any section or property missing from the file. |
| JSON file at `configPath` | File exists and parses | Default path `appsettings.json`, resolved relative to the process working directory, not the executable directory. |

It does **not** read:

- Environment variables (no `COUB_*` or `Environment.GetEnvironmentVariable` lookup). The `COUB_*` keys in `.env.example` and the "environment variable overrides" entry in `CHANGELOG.md` do not apply to this class.
- Command-line arguments, user secrets, or other providers.
- Changes to the file while the process runs. `reloadOnChange` is not used. `Load()` re-reads the file on each call, but nothing calls it automatically.

## Precedence

Effective value of a setting, highest priority first:

1. **Values in the JSON file**, when the file exists and deserializes successfully.
2. **Built-in defaults** from the property initializers in the settings classes.

The merge is per property, not per file. `System.Text.Json` deserializes into the default-initialized object, so a property missing from the file keeps its default.

Two failure modes fall back to defaults for the whole file:

- **Missing file.** `Load()` creates the file with the default configuration (`SaveDefault()` → `Save()`) and returns the defaults. This writes to disk as a side effect.
- **Unreadable or unparseable file.** Any exception from `File.ReadAllText` or `JsonSerializer.Deserialize` is caught by a bare `catch`. `Load()` returns the defaults and does not write to the file. No error, log entry, or warning is produced.

A `null` result from deserialization (e.g. the file contains only `null`) also yields the defaults.

### Matching rules

- Keys match the C# property names exactly, case-sensitive (`"MaxRetries"` matches; `"maxRetries"` and `"download"` do not). Default `JsonSerializerOptions` are used, so `PropertyNameCaseInsensitive` is off.
- Unknown keys are ignored.
- A value that cannot be converted to the property type (for example, a string where an `int` is expected) throws during deserialization. The `catch` then discards the **entire file**, so every setting falls back to its default, not only the offending one.

## Runtime behavior

- `Load()` reads the file and replaces the cached instance. It is protected by a lock.
- `GetConfiguration()` returns the cached instance and calls `Load()` only when nothing is cached.
- `Save(config)` writes the given object to the file with indented JSON, creating the directory if needed, and updates the cache. Writing does not add a source; it changes what the next `Load()` reads. Save errors are rethrown as `InvalidOperationException`.

## Relation to the rest of the application

`ConfigurationManager` is not on the main startup path:

- `Program.cs` builds an `IConfiguration` from `appsettings.json` with `Microsoft.Extensions.Configuration` (`AddJsonFile`, `optional: true`, `reloadOnChange: true`) and calls `AddCoubDownloaderServices`. That path binds the `Download`, `Conversion`, `Audio`, `Api`, and `Logging` sections to options classes. It has no environment-variable or command-line provider either.
- `ConfigurationManager` is registered only in `DependencyInjectionExtended.AddPhase2Services`, which is reached through `AddCoubDownloaderWithEditor`. `Program.cs` does not call that method.

So in the current `Program.cs` flow, `ApplicationConfiguration` is not used. If that changes, the precedence above applies.

## Known issue: the shipped `appsettings.json` does not match this schema

Checked by deserializing the repository's `appsettings.json` with `ApplicationConfiguration`:

- `Conversion.DefaultQuality` is `"High"` in the file, but `ConversionSettings.DefaultQuality` is an `int`. Deserialization fails.
- `Logging.LogLevel` is an object in the file, but `LoggingSettings.LogLevel` is a `string`. Deserialization fails.
- Because either failure discards the whole file, `ConfigurationManager.Load()` returns all defaults for the repository's `appsettings.json`. Its other keys do not match this schema either: `CoubDownloader`, `Audio`, and `Download` are not in the class, and `FFmpegPath` does not match `FfmpegPath` because matching is case-sensitive.

The file and the class are out of sync. Fixing that is outside this document; it would mean changing either the JSON or the class, and this document does not change either.

## Examples

Missing file: the file is created with defaults and the defaults are returned.

Partial file:

```json
{ "Download": { "MaxRetries": 7 } }
```

Result: `Download.MaxRetries` is `7`. All other settings keep their defaults (`MaxConcurrentDownloads` is `3`, `Conversion.FfmpegPath` is `"ffmpeg"`, and so on).

Invalid file (for example, `{ not json`): all settings are defaults. No error is reported.

Environment variable set to `9` while the file says `"MaxRetries": 7`: result is `7`. The environment variable is not read.
