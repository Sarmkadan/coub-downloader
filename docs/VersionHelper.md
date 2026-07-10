# VersionHelper

`VersionHelper` provides centralized version introspection, runtime environment details, and version-comparison utilities for the application. It exposes both static convenience methods for quick lookups and an instance-based `ApplicationInfo` object that aggregates application version, runtime version, operating system, processor count, process bitness, and build date. It also supports checking for available updates by comparing version strings.

## API

### Static Members

#### `GetApplicationVersion`
```csharp
public static string GetApplicationVersion { get; }
```
Returns the application version string (e.g., `"2.1.0"`). This property reads the version from the entry assembly’s informational or file version attribute.

#### `GetRuntimeVersion`
```csharp
public static string GetRuntimeVersion { get; }
```
Returns the .NET runtime version string (e.g., `"8.0.3"`). Derived from `Environment.Version`.

#### `GetOperatingSystem`
```csharp
public static string GetOperatingSystem { get; }
```
Returns a human-readable operating system description (e.g., `"Windows 10.0.19045"`). Combines OS name and version from `Environment.OSVersion` and `RuntimeInformation`.

#### `GetApplicationInfo`
```csharp
public static ApplicationInfo GetApplicationInfo { get; }
```
Returns an `ApplicationInfo` instance populated with current application version, runtime version, operating system, processor count, process bitness, and build date. The returned object is a snapshot; subsequent environment changes are not reflected.

#### `GetBuildDate`
```csharp
public static DateTime GetBuildDate { get; }
```
Returns the build date of the entry assembly as a `DateTime` in local time. The date is extracted from the `BuildDateAttribute` applied to the assembly. Throws `InvalidOperationException` if the attribute is missing or cannot be parsed.

#### `CompareVersions`
```csharp
public static int CompareVersions(string version1, string version2)
```
Compares two semantic version strings. Returns a negative integer if `version1` is less than `version2`, zero if equal, and a positive integer if greater. Both strings must be in a parseable `major.minor[.build[.revision]]` format. Throws `ArgumentException` if either string is null or not a valid version. Throws `FormatException` if parsing fails.

#### `IsGreaterThan`
```csharp
public static bool IsGreaterThan(string version1, string version2)
```
Returns `true` if `version1` is strictly greater than `version2` according to semantic versioning rules. Delegates to `CompareVersions`. Throws the same exceptions as `CompareVersions` for invalid inputs.

#### `IsUpdateAvailable`
```csharp
public static bool IsUpdateAvailable(string currentVersion, string remoteVersion)
```
Returns `true` if `remoteVersion` is greater than `currentVersion`. A convenience wrapper around `IsGreaterThan` with the argument order reversed for readability. Throws the same exceptions as `CompareVersions` for invalid inputs.

### Instance Members (`ApplicationInfo`)

#### `AppVersion`
```csharp
public string AppVersion { get; }
```
The application version string captured at the time the `ApplicationInfo` was created.

#### `RuntimeVersion`
```csharp
public string RuntimeVersion { get; }
```
The .NET runtime version string captured at creation time.

#### `OperatingSystem`
```csharp
public string OperatingSystem { get; }
```
The operating system description captured at creation time.

#### `ProcessorCount`
```csharp
public int ProcessorCount { get; }
```
The number of logical processors available to the process, obtained from `Environment.ProcessorCount`.

#### `Is64BitProcess`
```csharp
public bool Is64BitProcess { get; }
```
Indicates whether the current process is running as a 64-bit process. Derived from `Environment.Is64BitProcess`.

#### `BuildDate`
```csharp
public DateTime BuildDate { get; }
```
The build date of the entry assembly in local time, captured at creation time. May throw `InvalidOperationException` during construction if the `BuildDateAttribute` is absent or unparseable.

#### `ToString`
```csharp
public override string ToString()
```
Returns a multi-line string summarizing all properties of the `ApplicationInfo` instance, suitable for logging or diagnostic output.

### `BuildDateAttribute`

A custom assembly-level attribute that stores the build timestamp. Applied to the entry assembly during the build process. Its presence is required for `GetBuildDate` and `ApplicationInfo.BuildDate` to succeed.

### Update Checker Members

#### `Enable`
```csharp
public void Enable()
```
Enables the update-checking mechanism. Subsequent calls to `IsUpdateAvailable` (or related logic) will be allowed to proceed. If already enabled, calling this method has no effect.

#### `Disable`
```csharp
public void Disable()
```
Disables the update-checking mechanism. Calls to `IsUpdateAvailable` made while disabled will return `false` without performing a comparison. If already disabled, calling this method has no effect.

#### `IsEnabled`
```csharp
public bool IsEnabled { get; }
```
Returns `true` if the update-checking mechanism is currently enabled; otherwise `false`.

## Usage

### Example 1: Logging Full Environment Information

```csharp
var appInfo = VersionHelper.GetApplicationInfo;
Console.WriteLine(appInfo.ToString());

// Output:
// AppVersion: 2.3.1
// RuntimeVersion: 8.0.3
// OperatingSystem: Windows 10.0.22631
// ProcessorCount: 16
// Is64BitProcess: True
// BuildDate: 2025-03-12 14:30:00
```

### Example 2: Checking for Updates with Guard

```csharp
string current = VersionHelper.GetApplicationVersion;
string remote = "2.4.0";

if (VersionHelper.IsEnabled)
{
    if (VersionHelper.IsUpdateAvailable(current, remote))
    {
        Console.WriteLine($"Update available: {remote} (current: {current})");
    }
    else
    {
        Console.WriteLine("Already running the latest version.");
    }
}
else
{
    Console.WriteLine("Update checking is disabled.");
}
```

## Notes

- **Version parsing**: All version-comparison methods expect strings conforming to `major.minor[.build[.revision]]`. Pre-release labels and build metadata are not supported and will cause a `FormatException`.
- **Build date dependency**: `GetBuildDate` and `ApplicationInfo.BuildDate` rely on the presence of `BuildDateAttribute` in the entry assembly. If the build pipeline omits this attribute, these members throw `InvalidOperationException`. Ensure the attribute is emitted during compilation.
- **Snapshot semantics**: `ApplicationInfo` properties are captured at the moment of construction. Changes to the runtime environment (e.g., OS updates, process bitness changes via restart) are not reflected in an existing instance.
- **Thread safety**: All static properties and methods are thread-safe; they read immutable data or perform pure computations. The `Enable`/`Disable` methods on the update checker mutate shared state and are not thread-safe. External synchronization is required if multiple threads may toggle the enabled state concurrently.
- **Disabled update checking**: When `IsEnabled` returns `false`, `IsUpdateAvailable` short-circuits to `false` without evaluating version strings. This avoids unnecessary parsing and network calls in downstream consumers that check the enabled flag first.
