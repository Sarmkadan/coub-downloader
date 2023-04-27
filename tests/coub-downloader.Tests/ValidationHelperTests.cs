#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using CoubDownloader.Infrastructure.Utilities;
using FluentAssertions;
using Xunit;

namespace CoubDownloader.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ValidationHelper"/> class.
/// Tests various validation methods including email validation, URL validation,
/// bitrate validation, resolution validation, frame rate validation, and file name sanitization.
/// Also includes tests for the <see cref="ValidationBuilder"/> class.
/// </summary>
public class ValidationHelperTests
{
    // --- IsValidEmail ---

    /// <summary>
    /// Tests the <see cref="ValidationHelper.IsValidEmail"/> method with various email inputs.
    /// Validates that the method correctly identifies valid and invalid email addresses.
    /// </summary>
    /// <param name="email">The email address to test.</param>
    /// <param name="expected">The expected result (true for valid, false for invalid).</param>
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

    /// <summary>
    /// Tests the <see cref="ValidationHelper.IsValidUrl"/> method with various URL inputs.
    /// Validates that the method correctly identifies valid and invalid URLs across different schemes.
    /// </summary>
    /// <param name="url">The URL to test.</param>
    /// <param name="expected">The expected result (true for valid, false for invalid).</param>
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

    /// <summary>
    /// Tests the <see cref="ValidationHelper.IsValidCoubUrl"/> method with a valid Coub URL.
    /// Validates that the method correctly identifies a valid Coub domain URL.
    /// </summary>
    [Fact]
    public void IsValidCoubUrl_WithViewPath_ReturnsTrue()
    {
        var result = ValidationHelper.IsValidCoubUrl("https://coub.com/view/abc123");
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests the <see cref="ValidationHelper.IsValidCoubUrl"/> method with a non-Coub domain.
    /// Validates that the method correctly rejects URLs from non-Coub domains.
    /// </summary>
    [Fact]
    public void IsValidCoubUrl_NonCoubDomain_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidCoubUrl("https://youtube.com/watch?v=abc123");
        result.Should().BeFalse();
    }

    // --- IsValidBitrate ---

    /// <summary>
    /// Tests the <see cref="ValidationHelper.IsValidBitrate"/> method with boundary values.
    /// Validates that the method correctly identifies valid bitrate values within the allowed range.
    /// </summary>
    /// <param name="bitrate">The bitrate value to test.</param>
    /// <param name="expected">The expected result (true for valid, false for invalid).</param>
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

    /// <summary>
    /// Tests the <see cref="ValidationHelper.IsValidResolution"/> method with standard HD resolution.
    /// Validates that the method correctly identifies valid HD resolution values.
    /// </summary>
    [Fact]
    public void IsValidResolution_StandardHD_ReturnsTrue()
    {
        var result = ValidationHelper.IsValidResolution(1920, 1080);
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests the <see cref="ValidationHelper.IsValidResolution"/> method with zero width.
    /// Validates that the method correctly rejects resolution with zero width.
    /// </summary>
    [Fact]
    public void IsValidResolution_ZeroWidth_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidResolution(0, 1080);
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests the <see cref="ValidationHelper.IsValidResolution"/> method with dimensions exceeding maximum allowed values.
    /// Validates that the method correctly rejects resolution values that exceed maximum dimensions.
    /// </summary>
    [Fact]
    public void IsValidResolution_ExceedsMaxDimension_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidResolution(9000, 1080);
        result.Should().BeFalse();
    }

    // --- IsValidFrameRate ---

    /// <summary>
    /// Tests the <see cref="ValidationHelper.IsValidFrameRate"/> method with boundary values.
    /// Validates that the method correctly identifies valid frame rate values within the allowed range.
    /// </summary>
    /// <param name="fps">The frame rate value to test.</param>
    /// <param name="expected">The expected result (true for valid, false for invalid).</param>
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

    /// <summary>
    /// Tests the <see cref="ValidationHelper.SanitizeFileName"/> method with a filename containing invalid characters.
    /// Validates that the method correctly removes invalid path separators from the filename.
    /// </summary>
    [Fact]
    public void SanitizeFileName_ContainsInvalidChars_RemovesThem()
    {
        // Path separator '/' is invalid in a filename on all platforms
        var result = ValidationHelper.SanitizeFileName("my/file/name.mp4");
        result.Should().NotContain("/");
        result.Should().Contain("myfilename.mp4");
    }

    /// <summary>
    /// Tests the <see cref="ValidationHelper.SanitizeFileName"/> method with a clean filename.
    /// Validates that the method returns the filename unchanged when no sanitization is needed.
    /// </summary>
    [Fact]
    public void SanitizeFileName_AlreadyClean_ReturnsUnchanged()
    {
        var result = ValidationHelper.SanitizeFileName("clean_filename.mp4");
        result.Should().Be("clean_filename.mp4");
    }

    // --- ValidationBuilder ---

    /// <summary>
    /// Tests the <see cref="ValidationBuilder"/> class when all validation rules pass.
    /// Validates that the builder correctly reports valid state and no errors.
    /// </summary>
    [Fact]
    public void ValidationBuilder_AllRulesPassed_IsValidTrue()
    {
        var builder = new ValidationBuilder()
            .RequireNotEmpty("value", "Field")
            .RequireRange(5, 1, 10, "Count");

        builder.IsValid.Should().BeTrue();
        builder.GetErrors().Should().BeEmpty();
    }

    /// <summary>
    /// Tests the <see cref="ValidationBuilder"/> class when a required field is empty.
    /// Validates that the builder correctly collects error for empty required fields.
    /// </summary>
    [Fact]
    public void ValidationBuilder_EmptyRequiredField_CollectsError()
    {
        var builder = new ValidationBuilder()
            .RequireNotEmpty("", "Username");

        builder.IsValid.Should().BeFalse();
        builder.GetErrors().Should().ContainSingle(e => e.field == "Username");
    }

    /// <summary>
    /// Tests the <see cref="ValidationBuilder"/> class when a value is out of range.
    /// Validates that the builder correctly collects error for values outside the specified range.
    /// </summary>
    [Fact]
    public void ValidationBuilder_OutOfRangeValue_CollectsError()
    {
        var builder = new ValidationBuilder()
            .RequireRange(150, 1, 100, "Percentage");

        builder.IsValid.Should().BeFalse();
        builder.GetErrors().Should().ContainSingle(e => e.field == "Percentage");
    }

    /// <summary>
    /// Tests the <see cref="ValidationBuilder.ThrowIfInvalid"/> property.
    /// Validates that the property throws an ArgumentException when validation fails.
    /// </summary>
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