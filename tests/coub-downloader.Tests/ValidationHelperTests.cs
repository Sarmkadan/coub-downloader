// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Infrastructure.Utilities;
using FluentAssertions;
using Xunit;

namespace CoubDownloader.Tests;

public class ValidationHelperTests
{
    // --- IsValidEmail ---

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("name.surname+tag@sub.domain.org", true)]
    [InlineData("notanemail", false)]
    [InlineData("missing@", false)]
    [InlineData("", false)]
    public void IsValidEmail_VariousInputs_ReturnsExpectedResult(string email, bool expected)
    {
        var result = ValidationHelper.IsValidEmail(email);
        result.Should().Be(expected);
    }

    // --- IsValidUrl ---

    [Theory]
    [InlineData("https://coub.com/view/abc123", true)]
    [InlineData("http://example.com", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("not-a-url", false)]
    [InlineData("", false)]
    public void IsValidUrl_VariousSchemes_ReturnsExpectedResult(string url, bool expected)
    {
        var result = ValidationHelper.IsValidUrl(url);
        result.Should().Be(expected);
    }

    // --- IsValidCoubUrl ---

    [Fact]
    public void IsValidCoubUrl_WithViewPath_ReturnsTrue()
    {
        var result = ValidationHelper.IsValidCoubUrl("https://coub.com/view/abc123");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidCoubUrl_NonCoubDomain_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidCoubUrl("https://youtube.com/watch?v=abc123");
        result.Should().BeFalse();
    }

    // --- IsValidBitrate ---

    [Theory]
    [InlineData(1, true)]
    [InlineData(5000, true)]
    [InlineData(50000, true)]
    [InlineData(0, false)]
    [InlineData(50001, false)]
    [InlineData(-1, false)]
    public void IsValidBitrate_BoundaryValues_ReturnsExpectedResult(int bitrate, bool expected)
    {
        var result = ValidationHelper.IsValidBitrate(bitrate);
        result.Should().Be(expected);
    }

    // --- IsValidResolution ---

    [Fact]
    public void IsValidResolution_StandardHD_ReturnsTrue()
    {
        var result = ValidationHelper.IsValidResolution(1920, 1080);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidResolution_ZeroWidth_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidResolution(0, 1080);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidResolution_ExceedsMaxDimension_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidResolution(9000, 1080);
        result.Should().BeFalse();
    }

    // --- IsValidFrameRate ---

    [Theory]
    [InlineData(1, true)]
    [InlineData(30, true)]
    [InlineData(120, true)]
    [InlineData(0, false)]
    [InlineData(121, false)]
    public void IsValidFrameRate_BoundaryValues_ReturnsExpectedResult(int fps, bool expected)
    {
        var result = ValidationHelper.IsValidFrameRate(fps);
        result.Should().Be(expected);
    }

    // --- SanitizeFileName ---

    [Fact]
    public void SanitizeFileName_ContainsInvalidChars_RemovesThem()
    {
        // Path separator '/' is invalid in a filename on all platforms
        var result = ValidationHelper.SanitizeFileName("my/file/name.mp4");
        result.Should().NotContain("/");
        result.Should().Contain("myfilename.mp4");
    }

    [Fact]
    public void SanitizeFileName_AlreadyClean_ReturnsUnchanged()
    {
        var result = ValidationHelper.SanitizeFileName("clean_filename.mp4");
        result.Should().Be("clean_filename.mp4");
    }

    // --- ValidationBuilder ---

    [Fact]
    public void ValidationBuilder_AllRulesPassed_IsValidTrue()
    {
        var builder = new ValidationBuilder()
            .RequireNotEmpty("value", "Field")
            .RequireRange(5, 1, 10, "Count");

        builder.IsValid.Should().BeTrue();
        builder.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void ValidationBuilder_EmptyRequiredField_CollectsError()
    {
        var builder = new ValidationBuilder()
            .RequireNotEmpty("", "Username");

        builder.IsValid.Should().BeFalse();
        builder.GetErrors().Should().ContainSingle(e => e.field == "Username");
    }

    [Fact]
    public void ValidationBuilder_OutOfRangeValue_CollectsError()
    {
        var builder = new ValidationBuilder()
            .RequireRange(150, 1, 100, "Percentage");

        builder.IsValid.Should().BeFalse();
        builder.GetErrors().Should().ContainSingle(e => e.field == "Percentage");
    }

    [Fact]
    public void ValidationBuilder_ThrowIfInvalid_ThrowsArgumentException()
    {
        var builder = new ValidationBuilder()
            .RequireNotEmpty(null, "Title")
            .RequireRange(-1, 0, 100, "Progress");

        var act = builder.ThrowIfInvalid;
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Title*");
    }
}
