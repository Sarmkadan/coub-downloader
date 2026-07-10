# DateTimeExtensions

Provides a set of static helper methods for common date and time operations, such as formatting relative times, computing range boundaries, and converting between `DateTime` and Unix timestamps.

## API

### GetRelativeTime
**Purpose:** Returns a human‑readable string that describes how far a given date/time is from the current moment (e.g., “5 minutes ago”, “in 2 days”).  
**Parameters:**  
- `dateTime` (`DateTime`) – The date/time to evaluate.  
**Return value:** A string representing the relative time.  
**Exceptions:** None.

### FormatDuration
**Purpose:** Formats a `TimeSpan` into a compact, readable string (e.g., “02:05:07” or “1.23 h”).  
**Parameters:**  
- `duration` (`TimeSpan`) – The time interval to format.  
**Return value:** A formatted string representation of the duration.  
**Exceptions:** None.

### IsWithinRange
**Purpose:** Determines whether a date/time falls within a specified inclusive range.  
**Parameters:**  
- `value` (`DateTime`) – The date/time to test.  
- `start` (`DateTime`) – The beginning of the range.  
- `end` (`DateTime`) – The end of the range.  
**Return value:** `true` if `value` is between `start` and `end` (inclusive); otherwise `false`.  
**Exceptions:** None.

### StartOfDay
**Purpose:** Returns a new `DateTime` representing the start of the day (00:00:00) for the supplied date/time.  
**Parameters:**  
- `dateTime` (`DateTime`) – The input date/time.  
**Return value:** A `DateTime` set to midnight of the same day.  
**Exceptions:** None.

### EndOfDay
**Purpose:** Returns a new `DateTime` representing the end of the day (23:59:59.9999999) for the supplied date/time.  
**Parameters:**  
- `dateTime` (`DateTime`) – The input date/time.  
**Return value:** A `DateTime` set to the last tick of the same day.  
**Exceptions:** None.

### StartOfWeek
**Purpose:** Returns a new `DateTime` representing the start of the week for the supplied date/time.  
**Parameters:**  
- `dateTime` (`DateTime`) – The input date/time.  
- `startOfWeek` (`DayOfWeek`, optional, default `DayOfWeek.Sunday`) – The day that is considered the first day of the week.  
**Return value:** A `DateTime` set to midnight of the first day of the week containing `dateTime`.  
**Exceptions:** None.

### StartOfMonth
**Purpose:** Returns a new `DateTime` representing the first moment of the month for the supplied date/time.  
**Parameters:**  
- `dateTime` (`DateTime`) – The input date/time.  
**Return value:** A `DateTime` set to midnight on the first day of the same month.  
**Exceptions:** None.

### EndOfMonth
**Purpose:** Returns a new `DateTime` representing the last tick of the month for the supplied date/time.  
**Parameters:**  
- `dateTime` (`DateTime`) – The input date/time.  
**Return value:** A `DateTime` set to the final tick of the last day of the same month.  
**Exceptions:** None.

### FromUnixTimestamp
**Purpose:** Converts a Unix timestamp (seconds since 1970‑01‑01 UTC) to a `DateTime` value.  
**Parameters:**  
- `unixTimestamp` (`long`) – The number of seconds elapsed since the Unix epoch.  
**Return value:** A `DateTime` representing the same instant in UTC.  
**Exceptions:** None.

### ToUnixTimestamp
**Purpose:** Converts a `DateTime` value to a Unix timestamp (seconds since 1970‑01‑01 UTC).  
**Parameters:**  
- `dateTime` (`DateTime`) – The date/time to convert. If the value has a Kind other than Utc, it is first converted to UTC.  
**Return value:** A `long` representing the number of whole seconds elapsed since the Unix epoch.  
**Exceptions:** None.

### IsToday
**Purpose:** Checks whether a given date/time falls on the current day (according to the system’s local date).  
**Parameters:**  
- `dateTime` (`DateTime`) – The date/time to test.  
**Return value:** `true` if the date component of `dateTime` matches today’s date; otherwise `false`.  
**Exceptions:** None.

### IsYesterday
**Purpose:** Checks whether a given date/time falls on the previous day (according to the system’s local date).  
**Parameters:**  
- `dateTime` (`DateTime`) – The date/time to test.  
**Return value:** `true` if the date component of `dateTime` matches yesterday’s date; otherwise `false`.  
**Exceptions:** None.

## Usage

```csharp
using System;
using coub_downloader.Extensions; // assuming the namespace

class Program
{
    static void Main()
    {
        DateTime now = DateTime.Now;

        // Get a friendly relative time for a timestamp in the past
        DateTime posted = now.AddMinutes(-7);
        string relative = DateTimeExtensions.GetRelativeTime(posted);
        Console.WriteLine(relative); // e.g., "7 minutes ago"

        // Determine if a log entry belongs to today
        DateTime logTime = new DateTime(2025, 9, 24, 14, 30, 0);
        bool today = DateTimeExtensions.IsToday(logTime);
        Console.WriteLine(today ? "Log is from today" : "Log is older");
    }
}
```

```csharp
using System;
using coub_downloader.Extensions;

class Example
{
    static void Main()
    {
        // Convert a Unix timestamp to DateTime and back
        long unix = 1737782400; // 2025-01-25 00:00:00 UTC
        DateTime dt = DateTimeExtensions.FromUnixTimestamp(unix);
        Console.WriteLine(dt.ToString("u")); // 2025-01-25 00:00:00Z

        long back = DateTimeExtensions.ToUnixTimestamp(dt);
        Console.WriteLine(back); // 1737782400

        // Find the start of the current week (Monday as first day)
        DateTime startOfWeek = DateTimeExtensions.StartOfWeek(DateTime.Now, DayOfWeek.Monday);
        Console.WriteLine($"Week starts at: {startOfWeek:yyyy-MM-dd HH:mm:ss}");
    }
}
```

## Notes

- All methods are **pure**: they do not modify the input `DateTime` values and have no side effects, making them safe to call from multiple threads without additional synchronization.
- `GetRelativeTime` uses the system’s local clock (`DateTime.Now`) internally; if a consistent reference point is required across threads, consider passing a fixed `DateTime` via an overload (not present in this API).
- `FromUnixTimestamp` and `ToUnixTimestamp` operate on **whole seconds**; fractional seconds are discarded when converting to a timestamp and are not represented when converting from a timestamp.
- `IsWithinRange` treats the bounds as inclusive; passing a `start` value greater than `end` will always return `false` (no exception is thrown).
- `StartOfWeek` respects the supplied `startOfWeek` parameter; if an invalid `DayOfWeek` is supplied (the enum is closed, so all values are valid), the method will compute the correct start date.
- The methods rely only on the .NET `DateTime` and `TimeSpan` types; they do not depend on external state, so they are thread‑safe by design.
