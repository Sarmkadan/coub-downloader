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

## DateTimeExtensionsTests

The `DateTimeExtensionsTests` class provides a suite of xUnit tests that verify the behavior of extension methods for `DateTime` and `TimeSpan` operations, including relative time formatting, duration formatting, date range validation, and Unix timestamp conversion.

### Usage Example

```csharp
using System;
using CoubDownloader.Infrastructure.Utilities;

public class DateTimeDemo
{
    public void RunAll()
    {
        // GetRelativeTime - formats time spans relative to now
        var tenSecondsAgo = DateTime.UtcNow.AddSeconds(-10);
        Console.WriteLine(tenSecondsAgo.GetRelativeTime()); // "just now"

        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        Console.WriteLine(oneMinuteAgo.GetRelativeTime()); // "1m ago"

        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        Console.WriteLine(oneHourAgo.GetRelativeTime()); // "1h ago"

        // FormatDuration - formats TimeSpan as HH:MM:SS
        var duration = new TimeSpan(1, 2, 3);
        Console.WriteLine(duration.FormatDuration()); // "01:02:03"

        // IsWithinRange - checks if a date falls within a range
        var testDate = new DateTime(2026, 6, 26, 12, 0, 0);
        var rangeStart = new DateTime(2026, 6, 26, 0, 0, 0);
        var rangeEnd = new DateTime(2026, 6, 27, 0, 0, 0);
        Console.WriteLine(testDate.IsWithinRange(rangeStart, rangeEnd)); // true

        // StartOfDay - returns the date with time set to midnight
        var now = DateTime.Now;
        Console.WriteLine(now.StartOfDay()); // "2026-06-26 00:00:00" (date part only)

        // StartOfWeek - returns the date for the start of the week (Monday by default)
        var friday = new DateTime(2026, 6, 26); // Friday
        Console.WriteLine(friday.StartOfWeek(DayOfWeek.Monday)); // "2026-06-22" (previous Monday)

        // ToUnixTimestamp / FromUnixTimestamp - roundtrip conversion
        var utcDate = new DateTime(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = utcDate.ToUnixTimestamp();
        var backToDate = timestamp.FromUnixTimestamp().ToUniversalTime();
        Console.WriteLine(backToDate == utcDate); // true
    }
}
```

## IFileAdapter