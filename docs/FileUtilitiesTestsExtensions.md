# FileUtilitiesTestsExtensions

A set of extension methods and helper utilities for creating and managing temporary files and directories in unit tests. These utilities simplify test setup by automatically cleaning up resources after use, ensuring a clean state for each test.

## API

### `CreateTempFile`

Creates a temporary file with the specified content and returns a tuple containing the file path and a cleanup disposable. The file is automatically deleted when the cleanup object is disposed.

- **Parameters**
  - `content` (string): The content to write to the temporary file.
- **Returns**
  - `(string path, IDisposable cleanup)`: A tuple where `path` is the absolute path to the created file and `cleanup` is an `IDisposable` that deletes the file when disposed.
- **Throws**
  - `ArgumentNullException`: If `content` is `null`.
  - `IOException`: If the file cannot be created or written to.

### `CreateTempFileWithRandomContent`

Creates a temporary file with random content of a specified size and returns a tuple containing the file path and a cleanup disposable. The file is automatically deleted when the cleanup object is disposed.

- **Parameters**
  - `sizeInBytes` (int): The size of the random content in bytes.
- **Returns**
  - `(string path, IDisposable cleanup)`: A tuple where `path` is the absolute path to the created file and `cleanup` is an `IDisposable` that deletes the file when disposed.
- **Throws**
  - `ArgumentOutOfRangeException`: If `sizeInBytes` is negative.
  - `IOException`: If the file cannot be created or written to.

### `CreateTempDirectory`

Creates a temporary directory and returns a tuple containing the directory path and a cleanup disposable. The directory and its contents are automatically deleted when the cleanup object is disposed.

- **Returns**
  - `(string path, IDisposable cleanup)`: A tuple where `path` is the absolute path to the created directory and `cleanup` is an `IDisposable` that deletes the directory when disposed.
- **Throws**
  - `IOException`: If the directory cannot be created.

### `ShouldContainSameContentAs`

Asserts that the content of a file matches the expected content. Throws an exception if the files differ.

- **Parameters**
  - `actualFilePath` (string): The path to the file whose content should be verified.
  - `expectedContent` (string): The expected content of the file.
- **Throws**
  - `ArgumentNullException`: If `actualFilePath` or `expectedContent` is `null`.
  - `FileNotFoundException`: If `actualFilePath` does not exist.
  - `Exception`: If the file content does not match the expected content.

### `TempFileCleanup`

A disposable object that deletes a temporary file when disposed. Implements `IDisposable`.

- **Methods**
  - `Dispose()`: Deletes the associated temporary file.

### `TempDirectoryCleanup`

A disposable object that deletes a temporary directory and its contents when disposed. Implements `IDisposable`.

- **Methods**
  - `Dispose()`: Deletes the associated temporary directory and its contents.

## Usage

### Example 1: Testing file content
