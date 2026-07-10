using System;
using System.IO;
using System.Threading.Tasks;

namespace CoubDownloader.Tests;

public static class FileUtilitiesTestsExtensions
{
    /// <summary>
    /// Creates a temporary file with the specified content and returns its path.
    /// The file will be automatically deleted when disposed.
    /// </summary>
    /// <param name="content">The content to write to the file.</param>
    /// <returns>A tuple containing the file path and a disposable that cleans it up.</returns>
    public static (string path, IDisposable cleanup) CreateTempFile(this FileUtilitiesTests _, string content = null)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        if (content != null)
        {
            File.WriteAllText(path, content);
        }

        return (path, new TempFileCleanup(path));
    }

    /// <summary>
    /// Creates a temporary directory and returns its path.
    /// The directory will be automatically deleted when disposed.
    /// </summary>
    /// <returns>A tuple containing the directory path and a disposable that cleans it up.</returns>
    public static (string path, IDisposable cleanup) CreateTempDirectory(this FileUtilitiesTests _)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return (path, new TempDirectoryCleanup(path));
    }

    /// <summary>
    /// Asserts that two file paths point to the same file content.
    /// </summary>
    /// <param name="fileUtilitiesTests">The test instance.</param>
    /// <param name="expectedPath">The expected file path.</param>
    /// <param name="actualPath">The actual file path to verify.</param>
    public static void ShouldContainSameContentAs(this FileUtilitiesTests fileUtilitiesTests, string expectedPath, string actualPath)
    {
        if (!File.Exists(expectedPath))
        {
            throw new FileNotFoundException($"Expected file not found: {expectedPath}");
        }

        if (!File.Exists(actualPath))
        {
            throw new FileNotFoundException($"Actual file not found: {actualPath}");
        }

        var expectedContent = File.ReadAllText(expectedPath);
        var actualContent = File.ReadAllText(actualPath);

        if (expectedContent != actualContent)
        {
            throw new Exception($"File content mismatch.\nExpected: {expectedContent}\nActual: {actualContent}");
        }
    }

    /// <summary>
    /// Creates a temporary file with random content of specified size.
    /// </summary>
    /// <param name="size">The size of random content to generate in bytes.</param>
    /// <returns>A tuple containing the file path and a disposable that cleans it up.</returns>
    public static (string path, IDisposable cleanup) CreateTempFileWithRandomContent(this FileUtilitiesTests _, int size = 1024)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var random = new Random();
        var bytes = new byte[size];
        random.NextBytes(bytes);
        File.WriteAllBytes(path, bytes);
        return (path, new TempFileCleanup(path));
    }

    private sealed class TempFileCleanup : IDisposable
    {
        private readonly string _path;
        private bool _disposed;

        public TempFileCleanup(string path)
        {
            _path = path;
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }
            }
            catch
            {
                // Best effort cleanup
            }

            _disposed = true;
        }
    }

    private sealed class TempDirectoryCleanup : IDisposable
    {
        private readonly string _path;
        private bool _disposed;

        public TempDirectoryCleanup(string path)
        {
            _path = path;
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                if (Directory.Exists(_path))
                {
                    Directory.Delete(_path, true);
                }
            }
            catch
            {
                // Best effort cleanup
            }

            _disposed = true;
        }
    }
}