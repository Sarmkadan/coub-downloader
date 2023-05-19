using System;

namespace CoubDownloader.Tests
{
    /// <summary>
    /// Extension methods for DateTime used in tests to provide consistent test data and assertions.
    /// </summary>
    public static class DateTimeExtensionsTestsExtensions
    {
        /// <summary>
        /// Checks if a DateTime is within a specified range (inclusive).
        /// </summary>
        /// <param name="dateTime">The DateTime to check.</param>
        /// <param name="start">The start of the range (inclusive).</param>
        /// <param name="end">The end of the range (inclusive).</param>
        /// <returns>True if the DateTime is within the range; otherwise, false.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> is after <paramref name="end"/>.</exception>
        public static bool IsWithinRange(this DateTime dateTime, DateTime start, DateTime end)
        {
            if (start > end)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "Start date cannot be after end date.");
            }

            return dateTime >= start && dateTime <= end;
        }

        /// <summary>
        /// Converts a DateTime to Unix timestamp (seconds since 1970-01-01 UTC).
        /// </summary>
        /// <param name="dateTime">The DateTime to convert.</param>
        /// <returns>The Unix timestamp in seconds.</returns>
        /// <exception cref="OverflowException">Thrown when the DateTime represents a date before Unix epoch (1970-01-01).</exception>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            var result = (long)(dateTime.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
            if (result < 0)
            {
                throw new OverflowException("DateTime represents a date before Unix epoch (1970-01-01).");
            }

            return result;
        }
    }
}