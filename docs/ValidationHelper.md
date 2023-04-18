# ValidationHelper

`ValidationHelper` provides a centralized set of static validation methods and a fluent builder API for validating input data in the coub-downloader project. It covers common data types such as URLs, file paths, network addresses, media parameters, and general string patterns, and supports aggregating multiple validation errors before optionally throwing.

## API

### Static Validation Methods

#### `IsValidEmail`
```csharp
public static bool IsValidEmail(string email)
```
Returns `true` if `email` matches a standard email address format; otherwise `false`. Does not throw.

#### `IsValidUrl`
```csharp
public static bool IsValidUrl(string url)
```
Returns `true` if `url` is a well-formed absolute or relative URL; otherwise `false`. Does not throw.

#### `IsValidIpAddress`
```csharp
public static bool IsValidIpAddress(string ip)
```
Returns `true` if `ip` represents a valid IPv4 or IPv6 address; otherwise `false`. Does not throw.

#### `IsValidFilePath`
```csharp
public static bool IsValidFilePath(string path)
```
Returns `true` if `path` is a syntactically valid file path for the current operating system; otherwise `false`. Does not throw.

#### `SanitizeFileName`
```csharp
public static string SanitizeFileName(string fileName)
```
Returns a sanitized version of `fileName` with characters that are invalid in file names replaced or removed. Does not throw.

#### `IsValidCoubUrl`
```csharp
public static bool IsValidCoubUrl(string url)
```
Returns `true` if `url` matches the expected pattern for a coub.com video URL; otherwise `false`. Does not throw.

#### `IsValidBitrate`
```csharp
public static bool IsValidBitrate(string bitrate)
```
Returns `true` if `bitrate` represents a valid bitrate value (e.g., numeric with optional "k" or "M" suffix); otherwise `false`. Does not throw.

#### `IsValidResolution`
```csharp
public static bool IsValidResolution(string resolution)
```
Returns `true` if `resolution` matches a standard resolution format (e.g., "1920x1080"); otherwise `false`. Does not throw.

#### `IsValidFrameRate`
```csharp
public static bool IsValidFrameRate(string frameRate)
```
Returns `true` if `frameRate` represents a valid frames-per-second value; otherwise `false`. Does not throw.

#### `IsValidDuration`
```csharp
public static bool IsValidDuration(string duration)
```
Returns `true` if `duration` represents a valid time duration string (e.g., "00:05:30" or "330"); otherwise `false`. Does not throw.

#### `IsSafeDirectoryPath`
```csharp
public static bool IsSafeDirectoryPath(string path)
```
Returns `true` if `path` points to a directory that is considered safe for file operations (e.g., within allowed boundaries, not a system directory); otherwise `false`. Does not throw.

#### `IsValidBatchSize`
```csharp
public static bool IsValidBatchSize(string size)
```
Returns `true` if `size` represents a valid positive integer suitable for batch processing; otherwise `false`. Does not throw.

#### `MatchesPattern`
```csharp
public static bool MatchesPattern(string input, string pattern)
```
Returns `true` if `input` matches the specified regular expression `pattern`; otherwise `false`. Does not throw.

### Fluent Validation Builder

#### `RequireNotEmpty`
```csharp
public ValidationBuilder RequireNotEmpty(string field, string value)
```
Adds a validation rule that `value` must not be null or empty. If the check fails, an error for `field` is recorded. Returns the `ValidationBuilder` instance for chaining. Does not throw immediately.

#### `RequirePattern`
```csharp
public ValidationBuilder RequirePattern(string field, string value, string pattern)
```
Adds a validation rule that `value` must match the regular expression `pattern`. If the check fails, an error for `field` is recorded. Returns the `ValidationBuilder` instance for chaining. Does not throw immediately.

#### `RequireRange`
```csharp
public ValidationBuilder RequireRange(string field, int value, int min, int max)
```
Adds a validation rule that `value` must fall within the inclusive range `[min, max]`. If the check fails, an error for `field` is recorded. Returns the `ValidationBuilder` instance for chaining. Does not throw immediately.

#### `AddError`
```csharp
public ValidationBuilder AddError(string field, string message)
```
Manually adds an error message for the specified `field`. Returns the `ValidationBuilder` instance for chaining. Does not throw.

#### `GetErrors`
```csharp
public List<(string field, string message)> GetErrors()
```
Returns a list of all accumulated errors as tuples of field name and message. Does not throw.

#### `ThrowIfInvalid`
```csharp
public void ThrowIfInvalid()
```
Throws an exception (typically a `ValidationException` or `AggregateException`) if any errors have been accumulated via the builder methods. If no errors exist, returns silently.

## Usage

### Example 1: Quick Single-Field Checks
```csharp
string userInput = Console.ReadLine();

if (!ValidationHelper.IsValidUrl(userInput))
{
    Console.WriteLine("The provided URL is not valid.");
    return;
}

if (!ValidationHelper.IsSafeDirectoryPath("/home/user/downloads"))
{
    Console.WriteLine("The target directory is not considered safe.");
    return;
}

string safeName = ValidationHelper.SanitizeFileName("video:*.mp4");
Console.WriteLine($"Sanitized file name: {safeName}");
```

### Example 2: Multi-Field Validation with the Builder
```csharp
var builder = new ValidationBuilder();

builder
    .RequireNotEmpty("url", inputUrl)
    .RequirePattern("url", inputUrl, @"^https?://coub\.com/.*$")
    .RequireRange("threads", threadCount, 1, 16)
    .RequireNotEmpty("output", outputPath);

if (!ValidationHelper.IsValidResolution("1280x720"))
{
    builder.AddError("resolution", "Resolution must be in WxH format.");
}

var errors = builder.GetErrors();
if (errors.Any())
{
    foreach (var (field, message) in errors)
    {
        Console.WriteLine($"Error in {field}: {message}");
    }
    builder.ThrowIfInvalid(); // throws after logging
}

// Proceed with download
```

## Notes

- All static `IsValid*` and `MatchesPattern` methods are pure functions with no side effects and are safe to call from multiple threads concurrently.
- `SanitizeFileName` is also thread-safe; it does not access shared state.
- The `ValidationBuilder` class is **not** thread-safe by design. Instances should be used within a single thread or protected by external synchronization if shared across threads.
- `ThrowIfInvalid` typically throws a custom exception type defined elsewhere in the project. Callers should catch this specific type if graceful error handling is required.
- `IsValidFilePath` checks syntactic validity only; it does not verify that the path actually exists on disk.
- `IsSafeDirectoryPath` may rely on configurable allow-lists or deny-lists. Its behavior can depend on application settings or platform conventions.
- `RequireRange` works with integer values. For floating-point or other numeric types, use `AddError` with a custom check.
- The builder accumulates errors in insertion order. `GetErrors` returns them in the same order they were added.
