// README.md
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

The `FileUtilitiesTests` class provides a suite of xUnit tests that verify the behavior of the `FileUtilities` helper methods, including safe file name generation, file size formatting, directory creation, unique file naming, file copying with progress, and recursive directory deletion.

### Usage Example

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using CoubDownloader.Tests;

public class FileUtilitiesDemo
{
    public void RunAll()
    {
        // GenerateSafeFileName - converts invalid file names to safe versions
        var safeName = FileUtilities.GenerateSafeFileName(
            input: "video?file*name",
            extension: ".mp4");
        Console.WriteLine(safeName); // "videofilename.mp4"

        // FormatFileSize - converts bytes to human-readable format
        var sizeText = FileUtilities.FormatFileSize(1572864);
        Console.WriteLine(sizeText); // "1.50 MB"

        // EnsureDirectory - creates directory if it doesn't exist
        var directoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            "CoubDownloader");
        var ensuredPath = FileUtilities.EnsureDirectory(directoryPath);
        Console.WriteLine(Directory.Exists(ensuredPath)); // true

        // GetUniqueFileName - returns unique filename for existing files
        var basePath = Path.Combine(Path.GetTempPath(), "download.mp4");
        var uniquePath = FileUtilities.GetUniqueFileName(basePath);
        Console.WriteLine(Path.GetFileName(uniquePath)); // "download.mp4" or "download_1.mp4"

        // CopyFileWithProgressAsync - copies file with progress reporting
        var source = Path.Combine(Path.GetTempPath(), "source.txt");
        var destination = Path.Combine(Path.GetTempPath(), "destination.txt");
        File.WriteAllText(source, "test content");
        
        await FileUtilities.CopyFileWithProgressAsync(source, destination);
        Console.WriteLine(File.Exists(destination)); // true

        // DeleteDirectoryRecursively - deletes directory and all contents
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "file.txt"), "content");
        
        var deleted = FileUtilities.DeleteDirectoryRecursively(tempDir);
        Console.WriteLine(deleted); // true
    }
}
```