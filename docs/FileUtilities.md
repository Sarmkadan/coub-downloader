# FileUtilities

Utility class providing common file system operations for the coub-downloader project, including path normalization, disk space checks, recursive directory operations, and safe file name generation.

## API

### `public static string GenerateSafeFileName(string input)`

Converts an arbitrary string into a filesystem-safe filename by replacing invalid characters with underscores. Invalid characters are determined by the current platform's filesystem rules. Spaces are preserved.

- **Parameters**:
  - `input` – The string to sanitize.
- **Return value**: A new string with invalid characters replaced by underscores.
- **Exceptions**: Throws `ArgumentNullException` if `input` is null.

---

### `public static string FormatFileSize(long bytes)`

Formats a file size in bytes into a human-readable string with appropriate unit (B, KB, MB, GB).

- **Parameters**:
  - `bytes` – The size in bytes to format.
- **Return value**: A string like "1.23 MB" or "42 B".
- **Exceptions**: None.

---

### `public static string EnsureDirectory(string path)`

Ensures that the specified directory exists, creating it and any missing parent directories if necessary.

- **Parameters**:
  - `path` – The directory path to ensure.
- **Return value**: The normalized path to the directory.
- **Exceptions**:
  - Throws `ArgumentNullException` if `path` is null.
  - Throws `IOException` if directory creation fails.

---

### `public static long GetAvailableDiskSpace(string drivePath)`

Returns the number of free bytes available on the drive containing the specified path.

- **Parameters**:
  - `drivePath` – A path on the target drive.
- **Return value**: Free space in bytes, or -1 if unavailable.
- **Exceptions**: None.

---
### `public static bool HasSufficientDiskSpace(string path, long requiredBytes)`

Checks whether the drive containing the specified path has at least the requested free space.

- **Parameters**:
  - `path` – A path on the target drive.
  - `requiredBytes` – Minimum required free space in bytes.
- **Return value**: `true` if sufficient space is available; otherwise, `false`.
- **Exceptions**: None.

---
### `public static List<string> FindFiles(string directory, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)`

Recursively searches a directory for files matching the given pattern.

- **Parameters**:
  - `directory` – The root directory to search.
  - `searchPattern` – The file pattern (e.g., "*.mp4").
  - `searchOption` – Whether to search subdirectories.
- **Return value**: A list of absolute file paths.
- **Exceptions**:
  - Throws `ArgumentNullException` if `directory` or `searchPattern` is null.
  - Throws `DirectoryNotFoundException` if `directory` does not exist.

---
### `public static async Task CopyFileWithProgressAsync(string sourcePath, string destinationPath, IProgress<long> progress = null, CancellationToken cancellationToken = default)`

Copies a file asynchronously while reporting progress via an optional `IProgress<long>` callback.

- **Parameters**:
  - `sourcePath` – Path to the source file.
  - `destinationPath` – Path to the destination file.
  - `progress` – Optional progress reporter.
  - `cancellationToken` – Optional cancellation token.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**:
  - Throws `ArgumentNullException` if `sourcePath` or `destinationPath` is null.
  - Throws `FileNotFoundException` if `sourcePath` does not exist.
  - Throws `IOException` on file access errors.

---
### `public static bool DeleteDirectoryRecursively(string directoryPath)`

Deletes a directory and all its contents recursively.

- **Parameters**:
  - `directoryPath` – Path to the directory to delete.
- **Return value**: `true` if deletion succeeded; otherwise, `false`.
- **Exceptions**: None.

---
### `public static string GetUniqueFileName(string directory, string fileName)`

Generates a unique filename in the specified directory by appending a numeric suffix if necessary (e.g., "file(1).txt").

- **Parameters**:
  - `directory` – The target directory.
  - `fileName` – The desired filename.
- **Return value**: A unique absolute path.
- **Exceptions**:
  - Throws `ArgumentNullException` if `directory` or `fileName` is null.
  - Throws `DirectoryNotFoundException` if `directory` does not exist.

---
### `public static string NormalizePath(string path)`

Normalizes a filesystem path to use platform-specific directory separators and removes redundant separators or relative segments.

- **Parameters**:
  - `path` – The path to normalize.
- **Return value**: The normalized path.
- **Exceptions**: Throws `ArgumentNullException` if `path` is null.

---
### `public static string GetRelativePath(string relativeTo, string path)`

Computes the relative path from `relativeTo` to `path`.

- **Parameters**:
  - `relativeTo` – The base directory path.
  - `path` – The target path.
- **Return value**: A relative path string, or the original `path` if no relative form exists.
- **Exceptions**:
  - Throws `ArgumentNullException` if either parameter is null.

---
### `public static string CombinePaths(params string[] paths)`

Combines multiple path segments into a single path using platform-specific separators.

- **Parameters**:
  - `paths` – The path segments to combine.
- **Return value**: The combined path.
- **Exceptions**: Throws `ArgumentNullException` if `paths` is null or contains a null element.

## Usage

### Example 1: Safe Download Path Handling
