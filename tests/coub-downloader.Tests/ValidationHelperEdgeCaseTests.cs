#nullable enable
using FluentAssertions;
using CoubDownloader.Infrastructure.Utilities;
using Xunit;

namespace CoubDownloader.Tests;

public sealed class ValidationHelperEdgeCaseTests
{
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("", false)]
    [InlineData("not-an-email", false)]
    public void IsValidEmail_VariousInputs(string email, bool expected) =>
        ValidationHelper.IsValidEmail(email).Should().Be(expected);

    [Theory]
    [InlineData("https://coub.com/view/abc123", true)]
    [InlineData("https://coub.com/", true)]
    [InlineData("https://example.com", false)]
    [InlineData("not-a-url", false)]
    [InlineData("", false)]
    public void IsValidCoubUrl_VariousInputs(string url, bool expected) =>
        ValidationHelper.IsValidCoubUrl(url).Should().Be(expected);

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://localhost", true)]
    [InlineData("ftp://files.com", false)]
    [InlineData("not-a-url", false)]
    public void IsValidUrl_VariousInputs(string url, bool expected) =>
        ValidationHelper.IsValidUrl(url).Should().Be(expected);

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(50000, true)]
    [InlineData(50001, false)]
    [InlineData(-1, false)]
    public void IsValidBitrate_BoundaryValues(int bitrate, bool expected) =>
        ValidationHelper.IsValidBitrate(bitrate).Should().Be(expected);

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("::1", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    public void IsValidIpAddress_VariousInputs(string ip, bool expected) =>
        ValidationHelper.IsValidIpAddress(ip).Should().Be(expected);

    [Fact]
    public void SanitizeFileName_RemovesInvalidChars()
    {
        var result = ValidationHelper.SanitizeFileName("file<name>.mp4");
        result.Should().NotContain("<");
        result.Should().NotContain(">");
        result.Should().Contain("filename");
    }

    [Fact]
    public void SanitizeFileName_ValidName_ReturnsUnchanged()
    {
        var result = ValidationHelper.SanitizeFileName("valid-file_name.mp4");
        result.Should().Be("valid-file_name.mp4");
    }

    [Fact]
    public void IsValidFilePath_ValidPath_ReturnsTrue()
    {
        ValidationHelper.IsValidFilePath("/tmp/test.txt").Should().BeTrue();
    }
}
