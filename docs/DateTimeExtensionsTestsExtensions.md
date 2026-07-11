# DateTimeExtensionsTestsExtensions

Extension methods for `DateTime` providing common date/time utilities such as friendly time formatting, range checks, date arithmetic, and Unix timestamp conversion.

## API

### `public static string ToFriendlyRelativeTime(this DateTime date)`

Converts a `DateTime` into a human-readable relative time string (e.g., "2 minutes ago", "in 3 days").

- **Parameters**
  - `date`: The `DateTime` value to format.
- **Return Value**
  - A localized string representing the relative time.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `date` is outside the representable range for relative time formatting.

---

### `public static bool IsWithinRange(this DateTime date, DateTime start, DateTime end)`

Determines whether the given `DateTime` falls within the specified inclusive range.

- **Parameters**
  - `date`: The `DateTime` to check.
  - `start`: The start of the range (inclusive).
  - `end`: The end of the range (inclusive).
- **Return Value**
  - `true` if `date` is between `start` and `end` (inclusive); otherwise, `false`.

---

### `public static DateTime NextDay(this DateTime date)`

Returns a `DateTime` representing the next calendar day at the same time.

- **Parameters**
  - `date`: The starting `DateTime`.
- **Return Value**
  - A new `DateTime` with the date incremented by one day, preserving the time of day.

---

### `public static long ToUnixTimestamp(this DateTime date)`

Converts a `DateTime` to a Unix timestamp (seconds since 1970-01-01 00:00:00 UTC).

- **Parameters**
  - `date`: The `DateTime` to convert. Assumed to be in UTC.
- **Return Value**
  - The Unix timestamp as a `long`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `date` is outside the valid Unix timestamp range (approximately 1970–2262).

## Usage
