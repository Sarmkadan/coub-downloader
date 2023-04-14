# StringExtensions

The `StringExtensions` class provides a comprehensive set of static extension methods for the `string` type within the `coub-downloader` project. Designed to streamline common string manipulation tasks required for URL parsing, content sanitization, and data validation, this utility class eliminates boilerplate code by offering concise, reusable operations for checking emptiness, truncating text, validating URLs, generating slugs, and performing advanced substring searches.

## API

### IsNullOrWhiteSpace
Determines whether a specified string is null, empty, or consists only of white-space characters.
- **Parameters**: `this string? value` – The string to test.
- **Returns**: `true` if the value is null, empty, or white-space; otherwise, `false`.
- **Throws**: None.

### Truncate
Shortens a string to a specified maximum length, appending an ellipsis if truncation occurs.
- **Parameters**: `this string value` – The source string; `int maxLength` – The maximum allowed length; `string suffix` (optional) – The string to append if truncated (defaults to "...").
- **Returns**: The truncated string including the suffix if the original length exceeds `maxLength`; otherwise, the original string.
- **Throws**: `ArgumentNullException` if `value` is null.

### IsValidUrl
Validates whether a string represents a well-formed absolute URL.
- **Parameters**: `this string? value` – The string to validate.
- **Returns**: `true` if the string is a valid absolute URI with http or https scheme; otherwise, `false`.
- **Throws**: None.

### GetUrlDomain
Extracts the domain name from a valid URL string.
- **Parameters**: `this string? value` – The URL string.
- **Returns**: The domain name (e.g., "example.com") if the URL is valid; otherwise, `null`.
- **Throws**: None.

### ToSlug
Converts a string into a URL-friendly slug by lowercasing, removing special characters, and replacing spaces with hyphens.
- **Parameters**: `this string value` – The source string.
- **Returns**: A sanitized slug string.
- **Throws**: `ArgumentNullException` if `value` is null.

### Capitalize
Converts the first character of a string to uppercase and the remaining characters to lowercase.
- **Parameters**: `this string value` – The source string.
- **Returns**: The capitalized string.
- **Throws**: `ArgumentNullException` if `value` is null.

### ToTitleCase
Converts the string to title case, capitalizing the first letter of each word based on the current culture.
- **Parameters**: `this string value` – The source string.
- **Returns**: The string in title case.
- **Throws**: `ArgumentNullException` if `value` is null.

### ReplaceIgnoreCase
Replaces all occurrences of a specified substring with another string, ignoring case sensitivity.
- **Parameters**: `this string value` – The source string; `string oldValue` – The substring to find; `string newValue` – The replacement string.
- **Returns**: A new string with replacements made.
- **Throws**: `ArgumentNullException` if `value` or `oldValue` is null.

### SplitByMultiple
Splits a string based on multiple delimiter strings rather than single characters.
- **Parameters**: `this string value` – The source string; `string[] separators` – The delimiters; `StringSplitOptions options` – Split behavior options.
- **Returns**: An array of substrings.
- **Throws**: `ArgumentNullException` if `value` or `separators` is null.

### ContainsAny
Checks if the string contains any of the specified substrings.
- **Parameters**: `this string value` – The source string; `IEnumerable<string> values` – The collection of substrings to search for.
- **Returns**: `true` if any substring is found; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `value` or `values` is null.

### StartsWithAny
Checks if the string starts with any of the specified prefixes.
- **Parameters**: `this string value` – The source string; `IEnumerable<string> values` – The collection of prefixes.
- **Returns**: `true` if the string starts with any provided prefix; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `value` or `values` is null.

### SubstringBetween
Extracts the substring located between two specified delimiter strings.
- **Parameters**: `this string value` – The source string; `string startDelimiter` – The starting marker; `string endDelimiter` – The ending marker.
- **Returns**: The extracted substring if both delimiters are found in order; otherwise, `null`.
- **Throws**: `ArgumentNullException` if `value`, `startDelimiter`, or `endDelimiter` is null.

### CountOccurrences
Counts the number of non-overlapping occurrences of a substring within the string.
- **Parameters**: `this string value` – The source string; `string substring` – The substring to count.
- **Returns**: The integer count of occurrences.
- **Throws**: `ArgumentNullException` if `value` or `substring` is null.

### IsNumeric
Determines if the string consists entirely of numeric digits.
- **Parameters**: `this string? value` – The string to test.
- **Returns**: `true` if the string is non-empty and contains only digits; otherwise, `false`.
- **Throws**: None.

### RemoveDuplicateWhitespace
Replaces sequences of multiple whitespace characters with a single space.
- **Parameters**: `this string value` – The source string.
- **Returns**: The sanitized string with normalized whitespace.
- **Throws**: `ArgumentNullException` if `value` is null.

## Usage

### Example 1: URL Validation and Domain Extraction
This example demonstrates validating a user-provided URL and extracting its domain for logging or processing purposes.

```csharp
using CoubDownloader.Extensions;

string inputUrl = "https://www.coub.com/view/12345";

if (inputUrl.IsValidUrl())
{
    string? domain = inputUrl.GetUrlDomain();
    Console.WriteLine($"Valid URL detected. Domain: {domain}");
    
    // Check if the domain is specifically coub.com
    if (domain?.Contains("coub.com") == true)
    {
        Console.WriteLine("Processing Coub video...");
    }
}
else
{
    Console.WriteLine("Invalid URL format.");
}
```

### Example 2: Content Sanitization and Slug Generation
This example shows how to clean up a video title, capitalize it properly, and generate a safe filename slug.

```csharp
using CoubDownloader.Extensions;

string rawTitle = "   funny   cat VIDEO!!! (2023)   ";

// Normalize whitespace and capitalize
string cleanedTitle = rawTitle.RemoveDuplicateWhitespace().Capitalize();

// Generate a file-safe slug
string slug = rawTitle.ToSlug();

Console.WriteLine($"Cleaned Title: {cleanedTitle}"); 
// Output: "Funny cat video!!! (2023)"

Console.WriteLine($"Filename Slug: {slug}.mp4"); 
// Output: "funny-cat-video-2023.mp4"
```

## Notes

- **Null Handling**: Most methods that transform strings (`Truncate`, `ToSlug`, `Capitalize`, etc.) throw `ArgumentNullException` if the input `this` value is null, enforcing strict null safety. Methods designed for validation (`IsNullOrWhiteSpace`, `IsValidUrl`, `IsNumeric`) accept null inputs and return `false` or `true` appropriately without throwing.
- **Thread Safety**: As this class consists entirely of stateless static methods that operate only on their input parameters and do not share mutable static state, all methods are inherently thread-safe.
- **Culture Sensitivity**: The `ToTitleCase` method relies on the current thread's culture info. For consistent behavior across different server environments, consider setting the appropriate culture context before invocation.
- **Edge Cases**: 
  - `SubstringBetween` returns `null` if the start delimiter appears after the end delimiter or if either is missing.
  - `IsNumeric` returns `false` for empty strings, strings with decimal points, or negative signs, as it strictly checks for digit characters only.
  - `Truncate` returns the original string unchanged if the string length is less than or equal to the specified `maxLength`, even if a suffix is provided.
