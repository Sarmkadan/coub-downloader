#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Infrastructure.Utilities;
using FluentAssertions;
using Xunit;

namespace CoubDownloader.Tests;

public class DateTimeExtensionsTests
{
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

    [Fact]
    public void FormatDuration_ValidTimeSpan_ReturnsFormattedString()
    {
        var duration = new TimeSpan(1, 2, 3);
        var result = duration.FormatDuration();
        result.Should().Be("01:02:03");
    }

    [Fact]
    public void IsWithinRange_DateInRange_ReturnsTrue()
    {
        var dateTime = new DateTime(2026, 6, 26, 12, 0, 0);
        var start = new DateTime(2026, 6, 26, 0, 0, 0);
        var end = new DateTime(2026, 6, 27, 0, 0, 0);
        dateTime.IsWithinRange(start, end).Should().BeTrue();
    }

    [Fact]
    public void StartOfDay_ReturnsCorrectDatePart()
    {
        var dateTime = new DateTime(2026, 6, 26, 15, 30, 0);
        var expected = new DateTime(2026, 6, 26, 0, 0, 0);
        dateTime.StartOfDay().Should().Be(expected);
    }

    [Fact]
    public void StartOfWeek_MondayStart_ReturnsCorrectDate()
    {
        var dateTime = new DateTime(2026, 6, 26); // Friday
        var expected = new DateTime(2026, 6, 22); // Previous Monday
        dateTime.StartOfWeek(DayOfWeek.Monday).Should().Be(expected);
    }

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
