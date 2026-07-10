# FileUtilitiesTests

Unit tests for the `FileUtilities` static class, which provides helper methods for common file-system operations such as generating safe file names, formatting file sizes, ensuring directories exist, generating unique file names, copying files with progress reporting, and recursively deleting directories.

## API

### `GenerateSafeFileName_ShouldReturnSafeName`
Ensures that a given file name is safe for use on the file system by removing or replacing invalid characters and trimming whitespace.

- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No documented exceptions.

### `FormatFileSize_ShouldReturnHumanReadableSize`
Converts a raw file size in bytes into a human-readable string with an appropriate unit (e.g., KB, MB, GB).

- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No documented exceptions.

### `EnsureDirectory_ShouldCreateDirectoryIfDoesNotExist`
Ensures that the specified directory exists; creates it if it does not.

- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No documented exceptions.

### `GetUniqueFileName_ShouldReturnOriginalIfFileDoesNotExist`
Returns the original file name if no file with that name exists in the specified directory.

- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No documented exceptions.

### `GetUniqueFileName_ShouldReturnNewNameIfFileExists`
Returns a new, unique file name by appending a numeric suffix if a file with the specified name already exists in the directory.

- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No documented exceptions.

### `CopyFileWithProgressAsync_ShouldCopyFileSuccessfully`
Asynchronously copies a file from a source path to a destination path while reporting progress via an `IProgress<long>` callback.

- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: No documented exceptions.

### `DeleteDirectoryRecursively_ShouldDeleteDirectory`
Recursively deletes a directory and all its contents.

- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No documented exceptions.

## Usage
