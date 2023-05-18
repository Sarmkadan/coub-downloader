#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>DateTime extension methods</summary>
public static class DateTimeExtensions
{
    /// <summary>Get human-readable time difference</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is in the future.</exception>
    public static string GetRelativeTime(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dateTime, DateTime.UtcNow, nameof(dateTime));

        var timeSpan = DateTime.UtcNow - dateTime;

        return timeSpan.TotalSeconds < 60 ? "just now"
            : timeSpan.TotalMinutes <60 ? $"{(int)timeSpan.TotalMinutes}m ago"
            : timeSpan.TotalHours <24 ? $"{(int)timeSpan.TotalHours}h ago"
            : timeSpan.TotalDays <7 ? $"{(int)timeSpan.TotalDays}d ago"
            : $"{dateTime:MMM d, yyyy}";
    }

    /// <summary>Format duration as HH:MM:SS</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="duration"/> is negative.</exception>
    public static string FormatDuration(this TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration cannot be negative.");
        }
        return duration.ToString(@"hh\:mm\:ss");
    }

    /// <summary>Check if date is within range</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> is after <paramref name="end"/>.</exception>
    public static bool IsWithinRange(this DateTime dateTime, DateTime start, DateTime end)
    {
        if (start > end)
        {
            throw new ArgumentOutOfRangeException(nameof(start), "Start date cannot be after end date.");
        }
        return dateTime >= start && dateTime <= end;
    }

    /// <summary>Get start of day</summary>
    public static DateTime StartOfDay(this DateTime dateTime) => dateTime.Date;

    /// <summary>Get end of day</summary>
    public static DateTime EndOfDay(this DateTime dateTime) => dateTime.Date.AddDays(1).AddTicks(-1);

    /// <summary>Get start of week</summary>
    /// <param name="dateTime">The date to calculate the start of week for.</param>
    /// <param name="startDay">The day of week to consider as the first day of week. Defaults to Monday.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startDay"/> is not a valid DayOfWeek value.</exception>
    public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startDay = DayOfWeek.Monday)
    {
        if (startDay is < DayOfWeek.Sunday or > DayOfWeek.Saturday)
        {
            throw new ArgumentOutOfRangeException(nameof(startDay), "DayOfWeek must be between Sunday and Saturday.");
        }

        var diff = (7 + (dateTime.DayOfWeek - startDay)) % 7;
        return dateTime.AddDays(-diff).Date;
    }

    /// <summary>Get start of month</summary>
    public static DateTime StartOfMonth(this DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, 1);

    /// <summary>Get end of month</summary>
    public static DateTime EndOfMonth(this DateTime dateTime) => dateTime.StartOfMonth().AddMonths(1).AddDays(-1);

    /// <summary>Convert Unix timestamp to DateTime</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timestamp"/> represents a date before Unix epoch.</exception>
    public static DateTime FromUnixTimestamp(this long timestamp)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(timestamp, nameof(timestamp));

        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddSeconds(timestamp).ToLocalTime();
    }

    /// <summary>Convert DateTime to Unix timestamp</summary>
    /// <exception cref="OverflowException">Thrown when the resulting timestamp would overflow a long.</exception>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        var result = (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        if (result < 0)
        {
            throw new OverflowException("DateTime represents a date before Unix epoch (1970-01-01).");
        }
        return result;
    }

    /// <summary>Check if date is today</summary>
    public static bool IsToday(this DateTime dateTime) => dateTime.Date == DateTime.Today;

    /// <summary>Check if date is yesterday</summary>
    public static bool IsYesterday(this DateTime dateTime) => dateTime.Date == DateTime.Today.AddDays(-1);
}