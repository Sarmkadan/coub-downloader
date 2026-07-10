using System;

namespace CoubDownloader.Tests
{
    public static class DateTimeExtensionsTestsExtensions
    {
        /// <summary>
        /// Converts a DateTime to a friendly relative time string (e.g., "2 minutes ago", "in 3 hours").
        /// </summary>
        public static string ToFriendlyRelativeTime(this DateTime dateTime)
        {
            var now = DateTime.Now;
            var timeSpan = now - dateTime;

            if (timeSpan.TotalSeconds < 60)
            {
                return "just now";
            }

            if (timeSpan.TotalMinutes < 60)
            {
                var minutes = (int)timeSpan.TotalMinutes;
                return $"{minutes} minute{(minutes == 1 ? "" : "s")} ago";
            }

            if (timeSpan.TotalHours < 24)
            {
                var hours = (int)timeSpan.TotalHours;
                return $"{hours} hour{(hours == 1 ? "" : "s")} ago";
            }

            if (timeSpan.TotalDays < 7)
            {
                var days = (int)timeSpan.TotalDays;
                return $"{days} day{(days == 1 ? "" : "s")} ago";
            }

            if (timeSpan.TotalDays < 30)
            {
                var weeks = (int)(timeSpan.TotalDays / 7);
                return $"{weeks} week{(weeks == 1 ? "" : "s")} ago";
            }

            if (timeSpan.TotalDays < 365)
            {
                var months = (int)(timeSpan.TotalDays / 30);
                return $"{months} month{(months == 1 ? "" : "s")} ago";
            }

            var years = (int)(timeSpan.TotalDays / 365);
            return $"{years} year{(years == 1 ? "" : "s")} ago";
        }

        /// <summary>
        /// Checks if a DateTime is within a specified range (inclusive).
        /// </summary>
        public static bool IsWithinRange(this DateTime dateTime, DateTime start, DateTime end)
        {
            return dateTime >= start && dateTime <= end;
        }

        /// <summary>
        /// Returns the DateTime representing the start of the next day (midnight of the following day).
        /// </summary>
        public static DateTime NextDay(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1);
        }

        /// <summary>
        /// Converts a DateTime to Unix timestamp (seconds since 1970-01-01).
        /// </summary>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (long)(dateTime - DateTime.UnixEpoch).TotalSeconds;
        }
    }
}