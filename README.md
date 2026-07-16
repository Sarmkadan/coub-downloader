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

The `FileUtilitiesTests` class provides a suite of xUnit tests that verify the behavior of the `FileUtilities` helper methods, such as safe file name generation, file size formatting, directory creation, unique file naming, file copying with progress, and recursive directory deletion.

### Usage Example

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using CoubDownloader.Tests;
using FluentAssertions;

public class FileUtilitiesTestsDemo
{
    public async Task RunAll()
    {
        var tests = new FileUtilitiesTests();

        // GenerateSafeFileName test
        tests.GenerateSafeFileName_ShouldReturnSafeName(
            input: "invalid/file\\name",
            extension: ".mp4",
            expected: "invalidfilename.mp4");

        // FormatFileSize test
        tests.FormatFileSize_ShouldReturnHumanReadableSize(
            bytes: 1048576,
            expected: "1.00 MB");

        // EnsureDirectory test
        tests.EnsureDirectory_ShouldCreateDirectoryIfDoesNotExist();

        // GetUniqueFileName tests
        tests.GetUniqueFileName_ShouldReturnOriginalIfFileDoesNotExist();
        tests.GetUniqueFileName_ShouldReturnNewNameIfFileExists();

        // CopyFileWithProgressAsync test
        await tests.CopyFileWithProgressAsync_ShouldCopyFileSuccessfully();

        // DeleteDirectoryRecursively test
        tests.DeleteDirectoryRecursively_ShouldDeleteDirectory();
    }
}
```
