// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>DateTime extension methods</summary>
public static class DateTimeExtensions
{
    /// <summary>Get human-readable time difference</summary>
    public static string GetRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        return timeSpan.TotalSeconds < 60 ? "just now"
            : timeSpan.TotalMinutes < 60 ? $"{(int)timeSpan.TotalMinutes}m ago"
            : timeSpan.TotalHours < 24 ? $"{(int)timeSpan.TotalHours}h ago"
            : timeSpan.TotalDays < 7 ? $"{(int)timeSpan.TotalDays}d ago"
            : $"{dateTime:MMM d, yyyy}";
    }

    /// <summary>Format duration as HH:MM:SS</summary>
    public static string FormatDuration(this TimeSpan duration)
    {
        return duration.ToString(@"hh\:mm\:ss");
    }

    /// <summary>Check if date is within range</summary>
    public static bool IsWithinRange(this DateTime dateTime, DateTime start, DateTime end)
    {
        return dateTime >= start && dateTime <= end;
    }

    /// <summary>Get start of day</summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>Get end of day</summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>Get start of week</summary>
    public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startDay = DayOfWeek.Monday)
    {
        var diff = (7 + (dateTime.DayOfWeek - startDay)) % 7;
        return dateTime.AddDays(-diff).Date;
    }

    /// <summary>Get start of month</summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>Get end of month</summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddDays(-1);
    }

    /// <summary>Convert Unix timestamp to DateTime</summary>
    public static DateTime FromUnixTimestamp(this long timestamp)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddSeconds(timestamp).ToLocalTime();
    }

    /// <summary>Convert DateTime to Unix timestamp</summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(dateTime.ToUniversalTime() - epoch).TotalSeconds;
    }

    /// <summary>Check if date is today</summary>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today;
    }

    /// <summary>Check if date is yesterday</summary>
    public static bool IsYesterday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today.AddDays(-1);
    }
}
