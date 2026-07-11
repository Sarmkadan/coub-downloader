#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>
/// Provides extension methods for string operations.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class StringExtensions
{
    /// <summary>Check if string is null or whitespace</summary>
    /// <param name="value">The string to check.</param>
    /// <returns><see langword="true"/> if the string is null, empty, or consists only of white-space characters; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>Truncate string to maximum length</summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">Maximum length of the resulting string.</param>
    /// <param name="suffix">Suffix to append when truncating. Defaults to "...".</param>
    /// <returns>The truncated string, or the original string if it's shorter than <paramref name="maxLength"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is less than 0.</exception>
    /// <exception cref="ArgumentException"><paramref name="suffix"/> is null or empty, or <paramref name="maxLength"/> is less than the length of <paramref name="suffix"/>.</exception>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        ArgumentException.ThrowIfNullOrEmpty(suffix);

        if (value.Length <= maxLength)
            return value;

        if (maxLength < suffix.Length)
            throw new ArgumentException("Max length must be greater than or equal to suffix length", nameof(maxLength));

        return value[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>Check if string matches URL format</summary>
    /// <param name="value">The string to validate.</param>
    /// <returns><see langword="true"/> if the string is a valid absolute HTTP/HTTPS URL; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static bool IsValidUrl(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>Extract domain from URL</summary>
    /// <param name="url">The URL to parse.</param>
    /// <returns>The host/domain part of the URL, or <see langword="null"/> if parsing fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="url"/> is <see langword="null"/></exception>
    public static string? GetUrlDomain(this string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        try
        {
            var uri = new Uri(url);
            return uri.Host;
        }
        catch (UriFormatException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>Convert string to slug format</summary>
    /// <param name="value">The string to convert to slug format.</param>
    /// <returns>A URL-friendly slug string, or empty string if <paramref name="value"/> is null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToSlug(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Convert to lowercase
        var slug = value.ToLowerInvariant();

        // Remove accents - use invariant culture for consistent behavior
        slug = RemoveDiacritics(slug);

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove invalid characters (keep only alphanumeric, hyphens, and underscores)
        slug = Regex.Replace(slug, @"[^a-z0-9\-_]", "");

        // Replace multiple hyphens/underscores with single hyphen
        slug = Regex.Replace(slug, @"[-_]+", "-");

        // Remove leading/trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }

    /// <summary>Capitalize first character</summary>
    /// <param name="value">The string to capitalize.</param>
    /// <returns>The string with the first character capitalized, or the original string if it's null, empty, or has only one character.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string Capitalize(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrEmpty(value) || value.Length == 1)
            return value;

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    /// <summary>Convert to title case</summary>
    /// <param name="value">The string to convert to title case.</param>
    /// <returns>The string in title case format, or the original string if it's null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToTitleCase(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrEmpty(value))
            return value;

        var textInfo = CultureInfo.InvariantCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLowerInvariant());
    }

    /// <summary>Replace all occurrences case-insensitively</summary>
    /// <param name="value">The string to search and replace in.</param>
    /// <param name="oldValue">The value to replace.</param>
    /// <param name="newValue">The replacement value.</param>
    /// <returns>A new string with all occurrences of <paramref name="oldValue"/> replaced by <paramref name="newValue"/>, case-insensitive.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="oldValue"/> is <see langword="null"/></exception>
    public static string ReplaceIgnoreCase(this string value, string oldValue, string newValue)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(oldValue);

        return Regex.Replace(value, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
    }

    /// <summary>Split by multiple separators</summary>
    /// <param name="value">The string to split.</param>
    /// <param name="separators">The separator strings to use.</param>
    /// <returns>An array of substrings.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string[] SplitByMultiple(this string value, params string[] separators)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(separators);

        return value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>Check if string contains any of the values</summary>
    /// <param name="value">The string to search in.</param>
    /// <param name="values">The values to search for.</param>
    /// <returns><see langword="true"/> if the string contains any of the specified values; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="values"/> is <see langword="null"/></exception>
    public static bool ContainsAny(this string value, params string[] values)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(values);

        return values.Any(v => value.Contains(v));
    }

    /// <summary>Check if string starts with any of the values</summary>
    /// <param name="value">The string to check.</param>
    /// <param name="values">The values to check against.</param>
    /// <returns><see langword="true"/> if the string starts with any of the specified values; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="values"/> is <see langword="null"/></exception>
    public static bool StartsWithAny(this string value, params string[] values)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(values);

        return values.Any(v => value.StartsWith(v));
    }

    /// <summary>Get substring between two strings</summary>
    /// <param name="value">The string to search in.</param>
    /// <param name="start">The starting delimiter.</param>
    /// <param name="end">The ending delimiter.</param>
    /// <returns>The substring between the delimiters, or <see langword="null"/> if either delimiter is not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/>, <paramref name="start"/>, or <paramref name="end"/> is <see langword="null"/></exception>
    public static string? SubstringBetween(this string value, string start, string end)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        var startIndex = value.IndexOf(start);
        if (startIndex == -1)
            return null;

        startIndex += start.Length;
        var endIndex = value.IndexOf(end, startIndex);

        return endIndex == -1 ? null : value[startIndex..endIndex];
    }

    /// <summary>Count occurrences of substring</summary>
    /// <param name="value">The string to search in.</param>
    /// <param name="substring">The substring to count.</param>
    /// <returns>The number of occurrences of <paramref name="substring"/> in <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="substring"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="substring"/> is empty.</exception>
    public static int CountOccurrences(this string value, string substring)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(substring);

        if (substring.Length == 0)
            throw new ArgumentException("Substring cannot be empty", nameof(substring));

        return (value.Length - value.Replace(substring, "").Length) / substring.Length;
    }

    /// <summary>Check if string is numeric</summary>
    /// <param name="value">The string to check.</param>
    /// <returns><see langword="true"/> if the string contains only digits; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static bool IsNumeric(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.All(char.IsDigit);
    }

    /// <summary>Remove duplicate whitespace</summary>
    /// <param name="value">The string to process.</param>
    /// <returns>A string with duplicate whitespace replaced by single spaces and trimmed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string RemoveDuplicateWhitespace(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Regex.Replace(value, @"\s+", " ").Trim();
    }

    /// <summary>
    /// Removes diacritics (accents) from characters.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <returns>The text with diacritics removed.</returns>
    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}