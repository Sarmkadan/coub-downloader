// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>File system and path utilities</summary>
public static class FileUtilities
{
    /// <summary>Generate safe filename from text</summary>
    public static string GenerateSafeFileName(string input, string extension = ".mp4")
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(input
            .Where(c => !invalidChars.Contains(c))
            .Take(200)
            .ToArray());

        return $"{safeName.Trim()}{extension}";
    }

    /// <summary>Get file size in human-readable format</summary>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;

        for (int i = 0; i < sizes.Length; i++)
        {
            if (len < 1024)
                return $"{len:F2} {sizes[i]}";
            len /= 1024;
        }

        return $"{len:F2} PB";
    }

    /// <summary>Ensure directory exists, creating if necessary</summary>
    public static string EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>Get available disk space for a path</summary>
    public static long GetAvailableDiskSpace(string path)
    {
        var drive = new DriveInfo(Path.GetPathRoot(path) ?? "C:\\");
        return drive.AvailableFreeSpace;
    }

    /// <summary>Check if disk space is sufficient</summary>
    public static bool HasSufficientDiskSpace(string path, long requiredBytes)
    {
        return GetAvailableDiskSpace(path) > requiredBytes;
    }

    /// <summary>Find files matching pattern recursively</summary>
    public static List<string> FindFiles(string directory, string pattern = "*.*")
    {
        var results = new List<string>();

        try
        {
            foreach (var file in Directory.GetFiles(directory, pattern))
                results.Add(file);

            foreach (var subdir in Directory.GetDirectories(directory))
                results.AddRange(FindFiles(subdir, pattern));
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we can't access
        }

        return results;
    }

    /// <summary>Copy file with progress reporting</summary>
    public static async Task CopyFileWithProgressAsync(string source, string destination,
        IProgress<int>? progress = null)
    {
        const int bufferSize = 1024 * 64; // 64KB buffer
        var fileInfo = new FileInfo(source);
        long totalBytes = fileInfo.Length;
        long bytesCopied = 0;

        using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);
        using var destStream = new FileStream(destination, FileMode.Create, FileAccess.Write);

        var buffer = new byte[bufferSize];
        int bytesRead;

        while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await destStream.WriteAsync(buffer, 0, bytesRead);
            bytesCopied += bytesRead;

            var percentage = (int)((bytesCopied * 100) / totalBytes);
            progress?.Report(percentage);
        }
    }

    /// <summary>Delete directory and all contents recursively</summary>
    public static bool DeleteDirectoryRecursively(string path, int maxRetries = 3)
    {
        if (!Directory.Exists(path))
            return true;

        try
        {
            Directory.Delete(path, recursive: true);
            return true;
        }
        catch (IOException) when (maxRetries > 0)
        {
            Thread.Sleep(100);
            return DeleteDirectoryRecursively(path, maxRetries - 1);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Get unique filename if file already exists</summary>
    public static string GetUniqueFileName(string path)
    {
        if (!File.Exists(path))
            return path;

        var directory = Path.GetDirectoryName(path) ?? "";
        var filename = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);

        int counter = 1;
        while (true)
        {
            var newPath = Path.Combine(directory, $"{filename}_{counter}{extension}");
            if (!File.Exists(newPath))
                return newPath;
            counter++;
        }
    }
}

/// <summary>Path utilities for cross-platform compatibility</summary>
public static class PathUtilities
{
    /// <summary>Normalize path separators for current OS</summary>
    public static string NormalizePath(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar)
                   .Replace('/', Path.DirectorySeparatorChar);
    }

    /// <summary>Get relative path from base directory</summary>
    public static string GetRelativePath(string fullPath, string baseDirectory)
    {
        var uri = new Uri(fullPath);
        var baseUri = new Uri(baseDirectory.EndsWith(Path.DirectorySeparatorChar.ToString())
            ? baseDirectory
            : baseDirectory + Path.DirectorySeparatorChar);

        return Uri.UnescapeDataString(baseUri.MakeRelativeUri(uri).ToString());
    }

    /// <summary>Combine paths safely</summary>
    public static string CombinePaths(params string[] paths)
    {
        return Path.Combine(paths.Where(p => !string.IsNullOrEmpty(p)).ToArray());
    }
}
