# DateTimeExtensionsTests

Unit test class for `DateTimeExtensions` providing verification of relative time formatting, duration formatting, date range checks, and Unix timestamp conversions.

## API

### `GetRelativeTime_VariousTimeSpans_ReturnsExpectedString`
Verifies that the `GetRelativeTime` extension method produces the correct human-readable relative time string for a variety of time spans (e.g., seconds, minutes, hours, days, weeks, months, years). The test covers both past and future scenarios and validates the expected localized output format.

### `FormatDuration_ValidTimeSpan_ReturnsFormattedString`
Ensures that the `FormatDuration` extension method correctly formats a `TimeSpan` into a human-readable string (e.g., "2h 30m", "5d 12h"). The test validates correct pluralization, ordering, and omission of zero components.

### `IsWithinRange_DateInRange_ReturnsTrue`
Confirms that the `IsWithinRange` extension method accurately determines whether a given `DateTime` falls within a specified inclusive range defined by `start` and `end` parameters. The test includes edge cases at range boundaries.

### `StartOfDay_ReturnsCorrectDatePart`
Validates that the `StartOfDay` extension method strips the time component from a `DateTime`, returning a new `DateTime` with the time set to midnight (00:00:00) while preserving the original date and kind.

### `StartOfWeek_MondayStart_ReturnsCorrectDate`
Checks that the `StartOfWeek` extension method returns the correct `DateTime` representing the beginning of the week (Monday by default), adjusting the input date backward if necessary. The test ensures correct behavior across week boundaries and month/year transitions.

### `ToUnixTimestamp_FromUnixTimestamp_RoundtripsCorrectly`
Ensures that the `ToUnixTimestamp` and `FromUnixTimestamp` extension methods form a correct round-trip conversion between `DateTime` and Unix timestamp (seconds since 1970-01-01 UTC). The test validates that the original `DateTime` is preserved after conversion to and from the timestamp.

## Usage
