using Xunit;
using FluentAssertions;
using System.IO;
using CoubDownloader.Infrastructure.Utilities;
using System;

namespace CoubDownloader.Tests;

public class PathUtilitiesTests
{
    [Theory]
    [InlineData("folder\\file.txt", "folder/file.txt")]
    [InlineData("folder/file.txt", "folder/file.txt")]
    public void NormalizePath_ShouldReturnCorrectSeparators(string input, string expected)
    {
        // Adjust expected to be OS dependent
        var expectedNormalized = expected.Replace('/', Path.DirectorySeparatorChar);
        var result = PathUtilities.NormalizePath(input);
        
        result.Should().Be(expectedNormalized);
    }

    [Fact]
    public void GetRelativePath_ShouldReturnCorrectRelativePath()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "base");
        var fullPath = Path.Combine(baseDir, "subdir", "file.txt");
        
        var result = PathUtilities.GetRelativePath(fullPath, baseDir);
        
        result.Should().Be(Path.Combine("subdir", "file.txt").Replace('\\', '/'));
    }

    [Fact]
    public void CombinePaths_ShouldCombinePathsCorrectly()
    {
        var result = PathUtilities.CombinePaths("folder", "subfolder", "file.txt");
        result.Should().Be(Path.Combine("folder", "subfolder", "file.txt"));
    }
}
