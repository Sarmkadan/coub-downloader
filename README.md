## IFileAdapter

The `IFileAdapter` interface provides a minimal file-system abstraction for testing purposes. It allows you to mock file operations such as writing to a file and deleting a file.

### Usage Example

```csharp
using CoubDownloader.Tests;

public class MyClass
{
    private readonly IFileAdapter _fileAdapter;

    public MyClass(IFileAdapter fileAdapter)
    {
        _fileAdapter = fileAdapter;
    }

    public void WriteToFile(string path, string contents)
    {
        _fileAdapter.WriteAllText(path, contents);
    }

    public void DeleteFile(string path)
    {
        _fileAdapter.Delete(path);
    }
}
``` 

## FileUtilitiesTests

The `FileUtilitiesTests` class contains unit tests for the `FileUtilities` static class, verifying file system operations such as generating safe file names, formatting file sizes, ensuring directory creation, and handling file operations with progress tracking.

### Usage Example

```csharp
using CoubDownloader.Tests;
using Xunit;

public class FileUtilitiesExample
{
    [Fact]
    public void ExampleUsage()
    {
        // Generate a safe file name from an invalid path
        string safeName = FileUtilities.GenerateSafeFileName("invalid/file\\name", ".mp4");
        Assert.Equal("invalidfilename.mp4", safeName);

        // Format file size in human-readable format
        string formattedSize = FileUtilities.FormatFileSize(1048576);
        Assert.Equal("1.00 MB", formattedSize);

        // Ensure a directory exists
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string ensuredPath = FileUtilities.EnsureDirectory(tempDir);
        Assert.True(Directory.Exists(ensuredPath));
        Directory.Delete(ensuredPath, true);

        // Get unique file name when file doesn't exist
        string uniquePath = FileUtilities.GetUniqueFileName("/tmp/test.txt");
        Assert.Equal("/tmp/test.txt", uniquePath);

        // Get unique file name when file exists
        string existingPath = Path.Combine(Path.GetTempPath(), "existing.txt");
        File.WriteAllText(existingPath, "content");
        string newPath = FileUtilities.GetUniqueFileName(existingPath);
        Assert.NotEqual(existingPath, newPath);
        Assert.EndsWith("_1.txt", newPath);
        File.Delete(existingPath);
        if (File.Exists(newPath)) File.Delete(newPath);
    }
}
```
