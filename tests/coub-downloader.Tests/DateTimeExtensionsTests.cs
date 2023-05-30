#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Infrastructure.Utilities;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the DateTimeExtensions class.
/// </summary>
public class DateTimeExtensionsTests
{
    /// <summary>
    /// Tests the GetRelativeTime method with various time spans.
    /// </summary>
    /// <param name="secondsAgo">The number of seconds ago.</param>
    /// <param name="expected">The expected relative time string.</param>
    [Theory]
    [InlineData(10, "just now")]
    [InlineData(65, "1m ago")]
    [InlineData(3605, "1h ago")]
    [InlineData(86405, "1d ago")]
    public void GetRelativeTime_VariousTimeSpans_ReturnsExpectedString(int secondsAgo, string expected)
    {
        var dateTime = DateTime.UtcNow.AddSeconds(-secondsAgo);
        var result = dateTime.GetRelativeTime();
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests the FormatDuration method with a valid time span.
    /// </summary>
    [Fact]
    public void FormatDuration_ValidTimeSpan_ReturnsFormattedString()
    {
        var duration = new TimeSpan(1, 2, 3);
        var result = duration.FormatDuration();
        result.Should().Be("01:02:03");
    }

    /// <summary>
    /// Tests the IsWithinRange method with a date in the specified range.
    /// </summary>
    [Fact]
    public void IsWithinRange_DateInRange_ReturnsTrue()
    {
        var dateTime = new DateTime(2026, 6, 26, 12, 0, 0);
        var start = new DateTime(2026, 6, 26, 0, 0, 0);
        var end = new DateTime(2026, 6, 27, 0, 0, 0);
        dateTime.IsWithinRange(start, end).Should().BeTrue();
    }

    /// <summary>
    /// Tests the StartOfDay method with a date and time.
    /// </summary>
    [Fact]
    public void StartOfDay_ReturnsCorrectDatePart()
    {
        var dateTime = new DateTime(2026, 6, 26, 15, 30, 0);
        var expected = new DateTime(2026, 6, 26, 0, 0, 0);
        dateTime.StartOfDay().Should().Be(expected);
    }

    /// <summary>
    /// Tests the StartOfWeek method with a date and time, specifying Monday as the start of the week.
    /// </summary>
    [Fact]
    public void StartOfWeek_MondayStart_ReturnsCorrectDate()
    {
        var dateTime = new DateTime(2026, 6, 26); // Friday
        var expected = new DateTime(2026, 6, 22); // Previous Monday
        dateTime.StartOfWeek(DayOfWeek.Monday).Should().Be(expected);
    }

    /// <summary>
    /// Tests the ToUnixTimestamp method with a date and time, and then converts it back to a date and time using the FromUnixTimestamp method.
    /// </summary>
    [Fact]
    public void ToUnixTimestamp_FromUnixTimestamp_RoundtripsCorrectly()
    {
        var dateTime = new DateTime(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = dateTime.ToUnixTimestamp();
        var result = timestamp.FromUnixTimestamp().ToUniversalTime();

        // Unix timestamps are in seconds, so we expect precision matching
        result.Should().Be(dateTime);
    }
}
