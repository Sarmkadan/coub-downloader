using System;
using System.IO;
using System.Threading.Tasks;

namespace CoubDownloader.Tests;

/// <summary>
/// Extension methods for <see cref="FileUtilitiesTests"/> that provide test utilities for file operations.
/// </summary>
public static class FileUtilitiesTestsExtensions
{
    /// <summary>
    /// Creates a temporary file with the specified content and returns its path.
    /// The file will be automatically deleted when disposed.
    /// </summary>
    /// <param name="_">The test instance.</param>
    /// <param name="content">The content to write to the file. If null, creates an empty file.</param>
    /// <returns>A tuple containing the file path and a disposable that cleans it up.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="_"/> is null.</exception>
    public static (string path, IDisposable cleanup) CreateTempFile(this FileUtilitiesTests _, string content = null)
    {
        ArgumentNullException.ThrowIfNull(_);

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
    /// <param name="_">The test instance.</param>
    /// <returns>A tuple containing the directory path and a disposable that cleans it up.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="_"/> is null.</exception>
    public static (string path, IDisposable cleanup) CreateTempDirectory(this FileUtilitiesTests _)
    {
        ArgumentNullException.ThrowIfNull(_);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expectedPath"/> or <paramref name="actualPath"/> is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when either file does not exist.</exception>
    public static void ShouldContainSameContentAs(this FileUtilitiesTests fileUtilitiesTests, string expectedPath, string actualPath)
    {
        ArgumentNullException.ThrowIfNull(fileUtilitiesTests);
        ArgumentNullException.ThrowIfNull(expectedPath);
        ArgumentNullException.ThrowIfNull(actualPath);

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
            throw new FileNotFoundException($"File content mismatch.\nExpected: {expectedContent}\nActual: {actualContent}");
        }
    }

    /// <summary>
    /// Creates a temporary file with random content of specified size.
    /// </summary>
    /// <param name="_">The test instance.</param>
    /// <param name="size">The size of random content to generate in bytes. Must be positive.</param>
    /// <returns>A tuple containing the file path and a disposable that cleans it up.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="_"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is not positive.</exception>
    public static (string path, IDisposable cleanup) CreateTempFileWithRandomContent(this FileUtilitiesTests _, int size = 1024)
    {
        ArgumentNullException.ThrowIfNull(_);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(size, 0);

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

        public TempFileCleanup(string path) => _path = path;

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
            catch (IOException)
            {
                // Best effort cleanup - file may be locked or in use
            }
            catch (UnauthorizedAccessException)
            {
                // Best effort cleanup - may not have permissions
            }

            _disposed = true;
        }
    }

    private sealed class TempDirectoryCleanup : IDisposable
    {
        private readonly string _path;
        private bool _disposed;

        public TempDirectoryCleanup(string path) => _path = path;

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                if (Directory.Exists(_path))
                {
                    Directory.Delete(_path, recursive: true);
                }
            }
            catch (IOException)
            {
                // Best effort cleanup - directory may contain locked files
            }
            catch (UnauthorizedAccessException)
            {
                // Best effort cleanup - may not have permissions
            }

            _disposed = true;
        }
    }
}