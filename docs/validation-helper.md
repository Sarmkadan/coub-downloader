# ValidationHelper

Static input-validation and sanitization utilities for the coub-downloader project, plus `ValidationBuilder`, a small fluent helper that collects field errors and throws an `ArgumentException` on failure.

Source: `Infrastructure/Utilities/ValidationHelper.cs` (namespace `CoubDownloader.Infrastructure.Utilities`).

Unless noted otherwise, the `bool` validators return `false` for invalid input and never throw. Exceptions they may hit internally are caught.

---

## `ValidationHelper`

### Rules at a glance

| Method | Accepts | Rejects |
|---|---|---|
| `IsValidEmail` | Address that round-trips through `MailAddress` unchanged | Unparseable input, or input where the parsed address differs from the original (e.g. `Name <a@b.com>`) |
| `IsValidUrl` | Absolute URI with scheme `http` or `https` | Relative URIs, other schemes |
| `IsValidIpAddress` | Anything `IPAddress.TryParse` accepts (IPv4 and IPv6) | Non-IP text |
| `IsValidFilePath` | Any path `Path.GetFullPath` can resolve | Empty, null, or otherwise unresolvable paths |
| `IsValidCoubUrl` | `http`/`https` URL whose host is `coub.com` or a subdomain of it | Other hosts, other schemes, relative URIs |
| `IsValidBitrate` | `1` to `50000` inclusive | `0`, negatives, values above `50000` |
| `IsValidResolution` | Width and height each `1` to `8192` inclusive | Zero or negative values, either dimension above `8192` |
| `IsValidFrameRate` | `1` to `120` inclusive | Below `1`, above `120` |
| `IsValidDuration` | `> 0` and `<= 31,536,000` seconds (365 days) | Zero, negatives, longer than one year |
| `IsValidBatchSize` | `1` to `10000` inclusive | `0`, negatives, above `10000` |
| `IsSafeDirectoryPath` | `requestedPath` equal to `basePath`, or inside it | Paths that escape `basePath`, including sibling prefixes such as `/data/basement` vs `/data/base` |
| `MatchesPattern` | Value matching a regular expression (unanchored) | Non-matching values, invalid patterns |

### `public static bool IsValidEmail(string email)`

Parses `email` with `System.Net.Mail.MailAddress` and returns `true` only if the parsed `Address` is identical to the input string. This means display-name forms such as `Name <user@example.com>` are rejected even though `MailAddress` can parse them.

- **Parameters**: `email` – candidate address.
- **Returns**: `true` if the input is a bare, valid address; otherwise `false`.
- **Exceptions**: None. Parse failures (including `null`) return `false`.

### `public static bool IsValidUrl(string url)`

Returns `true` if `url` is an absolute URI whose scheme is `http` or `https`.

- **Parameters**: `url` – candidate URL.
- **Returns**: `true` for absolute `http`/`https` URIs; otherwise `false`.
- **Exceptions**: `ArgumentNullException` is thrown if `url` is `null`, because `Uri.TryCreate` does not accept null.

### `public static bool IsValidIpAddress(string ipAddress)`

Delegates to `System.Net.IPAddress.TryParse`. Accepts IPv4 and IPv6 literals. Because the check relies on the framework parser, it may also accept abbreviated IPv4 forms that the parser supports; the method does not enforce dotted-quad notation.

- **Parameters**: `ipAddress` – candidate address.
- **Returns**: `true` if the parser succeeds; otherwise `false`.
- **Exceptions**: `ArgumentNullException` is thrown if `ipAddress` is `null`.

### `public static bool IsValidFilePath(string path)`

Returns `true` if `Path.GetFullPath(path)` succeeds. This is a syntactic check only: it does not test whether the file or directory exists, and it does not check permissions.

- **Parameters**: `path` – candidate path.
- **Returns**: `true` if the path can be resolved to a full path; otherwise `false`.
- **Exceptions**: None. Any exception from `Path.GetFullPath` (including for `null` or an empty string) is caught and returns `false`.

### `public static string SanitizeFileName(string fileName)`

Removes every character returned by `Path.GetInvalidFileNameChars()` from `fileName`. Other characters are kept as-is; nothing is replaced with a placeholder. This method transforms its input rather than validating it. For underscore replacement, see `FileUtilities.GenerateSafeFileName`.

- **Parameters**: `fileName` – the name to sanitize.
- **Returns**: A new string with invalid characters removed. May be empty if every character was invalid.
- **Exceptions**: `ArgumentNullException` is thrown if `fileName` is `null` (not caught).

### `public static bool IsValidCoubUrl(string url)`

Returns `true` only when all of the following hold:

1. `url` parses as an absolute URI.
2. The scheme is `http` or `https`.
3. The host equals `coub.com` (case-insensitive) or ends with `.coub.com` (case-insensitive).

- **Parameters**: `url` – candidate Coub URL.
- **Returns**: `true` if all checks pass; otherwise `false`.
- **Exceptions**: `ArgumentNullException` is thrown if `url` is `null`.

### `public static bool IsValidBitrate(int bitrate)`

Returns `true` for `1 <= bitrate <= 50000`. The inline comment in the source reads "0-50 Mbps", which implies a unit of kbps; the method itself does not state a unit.

- **Parameters**: `bitrate` – candidate bitrate.
- **Returns**: `true` if in range; otherwise `false`.
- **Exceptions**: None.

### `public static bool IsValidResolution(int width, int height)`

Returns `true` if both `width` and `height` are in `1..8192` inclusive. Each dimension is checked independently; no aspect-ratio or total-pixel constraint applies.

- **Parameters**:
  - `width` – candidate width in pixels.
  - `height` – candidate height in pixels.
- **Returns**: `true` if both are in range; otherwise `false`.
- **Exceptions**: None.

### `public static bool IsValidFrameRate(int fps)`

Returns `true` for `1 <= fps <= 120`.

- **Parameters**: `fps` – candidate frame rate.
- **Returns**: `true` if in range; otherwise `false`.
- **Exceptions**: None.

### `public static bool IsValidDuration(double seconds)`

Returns `true` for `seconds > 0` and `seconds <= 86400 * 365` (31,536,000 seconds). The upper bound is one year.

- **Parameters**: `seconds` – candidate duration in seconds.
- **Returns**: `true` if in range; otherwise `false`.
- **Exceptions**: None. `NaN` fails the `> 0` comparison and returns `false`.

### `public static bool IsSafeDirectoryPath(string basePath, string requestedPath)`

Checks that `requestedPath` resolves to `basePath` itself or to a location inside it. Both arguments are normalized with `Path.GetFullPath` and trailing separators are trimmed before comparison.

- The comparison is ordinal and therefore case-sensitive on all platforms.
- A separator is required directly after the base path, so `/data/basement` is not treated as being inside `/data/base`.
- The check is purely lexical. It does not resolve symbolic links, so a symlink inside `basePath` that points elsewhere still passes.

- **Parameters**:
  - `basePath` – the directory that must contain the requested path.
  - `requestedPath` – the path to test, typically user-supplied.
- **Returns**: `true` if the requested path is the base or inside it; otherwise `false`.
- **Exceptions**: None. Any exception during normalization returns `false`.

### `public static bool IsValidBatchSize(int size)`

Returns `true` for `1 <= size <= 10000`.

- **Parameters**: `size` – candidate batch size.
- **Returns**: `true` if in range; otherwise `false`.
- **Exceptions**: None.

### `public static bool MatchesPattern(string value, string pattern)`

Returns `true` if `Regex.IsMatch(value, pattern)` succeeds. The match is unanchored, so the pattern may match any substring. Add `^` and `$` to require a full match.

- **Parameters**:
  - `value` – text to test.
  - `pattern` – regular expression.
- **Returns**: `true` if the pattern matches; `false` on no match, invalid pattern, or `null` input.
- **Exceptions**: None. All exceptions are caught.

---

## `ValidationBuilder`

`public class ValidationBuilder` collects validation errors through chainable calls. Each error is stored as a `(string field, string message)` tuple. Errors accumulate and are never cleared, so a builder should be used for one validation pass.

```csharp
var builder = new ValidationBuilder()
    .RequireNotEmpty(name, nameof(name))
    .RequireRange(batchSize, 1, 10000, nameof(batchSize))
    .RequirePattern(code, "^[A-Z]{3}$", nameof(code));

builder.ThrowIfInvalid();
```

### `public ValidationBuilder RequireNotEmpty(string? value, string fieldName)`

Adds the error `"{fieldName} cannot be empty"` when `value` is `null`, empty, or whitespace-only.

### `public ValidationBuilder RequirePattern(string value, string pattern, string fieldName)`

Adds the error `"{fieldName} has invalid format"` when `ValidationHelper.MatchesPattern(value, pattern)` returns `false`. This includes a `null` value or an invalid pattern, both of which make `MatchesPattern` return `false`.

### `public ValidationBuilder RequireRange(int value, int min, int max, string fieldName)`

Adds the error `"{fieldName} must be between {min} and {max}"` when `value` is outside `[min, max]`. Bounds are inclusive.

### `public ValidationBuilder AddError(string fieldName, string message)`

Adds an arbitrary error with the given field name and message. Performs no checks.

### `public List<(string field, string message)> GetErrors()`

Returns the internal error list directly, not a copy. Callers that modify the returned list also modify the builder's state.

### `public bool IsValid`

`true` when no errors have been recorded.

### `public void ThrowIfInvalid()`

If any errors exist, throws `ArgumentException` whose message is every error formatted as `"{field}: {message}"` and joined with `"; "`. The exception is created without a parameter name. Does nothing when the builder is valid.

---

## Related

- Tests: `tests/coub-downloader.Tests/ValidationHelperTests.cs`
- Other validation docs: `docs/ValidationHelper.md`, `docs/ValidationException.md`
- Safe file-name generation with underscore replacement: `docs/FileUtilities.md`
