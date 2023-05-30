using Xunit;
using FluentAssertions;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using CoubDownloader.Infrastructure.Utilities;

namespace CoubDownloader.Tests;

/// <summary>
/// Tests for the FileUtilities class.
/// </summary>
public class FileUtilitiesTests
{
    /// <summary>
    /// Tests the GenerateSafeFileName method.
    /// </summary>
    /// <param name="input">The input string to generate a safe file name from.</param>
    /// <param name="extension">The file extension.</param>
    /// <param name="expected">The expected safe file name.</param>
    [Theory]
    [InlineData("valid_file_name", ".mp4", "valid_file_name.mp4")]
    [InlineData("invalid/file\\name", ".webm", "invalidfilename.webm")]
    [InlineData("file?name*", ".mp4", "filename.mp4")]
    public void GenerateSafeFileName_ShouldReturnSafeName(string input, string extension, string expected)
    {
        var result = FileUtilities.GenerateSafeFileName(input, extension);
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests the FormatFileSize method.
    /// </summary>
    /// <param name="bytes">The file size in bytes.</param>
    /// <param name="expected">The expected human-readable file size.</param>
    [Theory]
    [InlineData(100, "100.00 B")]
    [InlineData(2048, "2.00 KB")]
    [InlineData(1048576, "1.00 MB")]
    [InlineData(1073741824, "1.00 GB")]
    public void FormatFileSize_ShouldReturnHumanReadableSize(long bytes, string expected)
    {
        var result = FileUtilities.FormatFileSize(bytes);
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests the EnsureDirectory method.
    /// </summary>
    /// <param name="path">The directory path to ensure exists.</param>
    /// <returns>The ensured directory path.</returns>
    [Fact]
    public void EnsureDirectory_ShouldCreateDirectoryIfDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var result = FileUtilities.EnsureDirectory(path);
            Directory.Exists(result).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(path))
                Directory.Delete(path);
        }
    }

    /// <summary>
    /// Tests the GetUniqueFileName method when the file does not exist.
    /// </summary>
    /// <param name="path">The file path to get a unique name for.</param>
    /// <returns>The unique file name.</returns>
    [Fact]
    public void GetUniqueFileName_ShouldReturnOriginalIfFileDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), "test_file.txt");
        if (File.Exists(path)) File.Delete(path);

        var result = FileUtilities.GetUniqueFileName(path);
        result.Should().Be(path);
    }

    /// <summary>
    /// Tests the GetUniqueFileName method when the file exists.
    /// </summary>
    /// <param name="path">The file path to get a unique name for.</param>
    /// <returns>The unique file name.</returns>
    [Fact]
    public void GetUniqueFileName_ShouldReturnNewNameIfFileExists()
    {
        var path = Path.Combine(Path.GetTempPath(), "test_exists.txt");
        File.WriteAllText(path, "content");
        
        try
        {
            var result = FileUtilities.GetUniqueFileName(path);
            result.Should().NotBe(path);
            result.Should().EndWith("_1.txt");
            File.Exists(result).Should().BeFalse();
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Tests the CopyFileWithProgressAsync method.
    /// </summary>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="destPath">The destination file path.</param>
    [Fact]
    public async Task CopyFileWithProgressAsync_ShouldCopyFileSuccessfully()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), "source.txt");
        var destPath = Path.Combine(Path.GetTempPath(), "dest.txt");
        File.WriteAllText(sourcePath, "hello world");
        
        try
        {
            await FileUtilities.CopyFileWithProgressAsync(sourcePath, destPath);
            File.Exists(destPath).Should().BeTrue();
            File.ReadAllText(destPath).Should().Be("hello world");
        }
        finally
        {
            File.Delete(sourcePath);
            File.Delete(destPath);
        }
    }

    /// <summary>
    /// Tests the DeleteDirectoryRecursively method.
    /// </summary>
    /// <param name="path">The directory path to delete recursively.</param>
    /// <returns>True if the directory was deleted successfully, false otherwise.</returns>
    [Fact]
    public void DeleteDirectoryRecursively_ShouldDeleteDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, "test.txt"), "content");
        
        var result = FileUtilities.DeleteDirectoryRecursively(path);
        
        result.Should().BeTrue();
        Directory.Exists(path).Should().BeFalse();
    }
}
