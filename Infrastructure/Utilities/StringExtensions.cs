// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>String extension methods</summary>
public static class StringExtensions
{
    /// <summary>Check if string is null or whitespace</summary>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>Truncate string to maximum length</summary>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>Check if string matches URL format</summary>
    public static bool IsValidUrl(this string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>Extract domain from URL</summary>
    public static string? GetUrlDomain(this string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Convert string to slug format</summary>
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Convert to lowercase
        var slug = value.ToLowerInvariant();

        // Remove accents
        var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(slug);
        slug = Encoding.ASCII.GetString(bytes);

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove invalid characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Replace multiple hyphens with single hyphen
        slug = Regex.Replace(slug, @"-+", "-");

        // Remove leading/trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }

    /// <summary>Capitalize first character</summary>
    public static string Capitalize(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToUpper(value[0]) + value[1..];
    }

    /// <summary>Convert to title case</summary>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var textInfo = new CultureInfo("en-US").TextInfo;
        return textInfo.ToTitleCase(value.ToLowerInvariant());
    }

    /// <summary>Replace all occurrences case-insensitively</summary>
    public static string ReplaceIgnoreCase(this string value, string oldValue, string newValue)
    {
        return Regex.Replace(value, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
    }

    /// <summary>Split by multiple separators</summary>
    public static string[] SplitByMultiple(this string value, params string[] separators)
    {
        return value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>Check if string contains any of the values</summary>
    public static bool ContainsAny(this string value, params string[] values)
    {
        return values.Any(v => value.Contains(v));
    }

    /// <summary>Check if string starts with any of the values</summary>
    public static bool StartsWithAny(this string value, params string[] values)
    {
        return values.Any(v => value.StartsWith(v));
    }

    /// <summary>Get substring between two strings</summary>
    public static string? SubstringBetween(this string value, string start, string end)
    {
        var startIndex = value.IndexOf(start);
        if (startIndex == -1) return null;

        startIndex += start.Length;
        var endIndex = value.IndexOf(end, startIndex);

        return endIndex == -1 ? null : value[startIndex..endIndex];
    }

    /// <summary>Count occurrences of substring</summary>
    public static int CountOccurrences(this string value, string substring)
    {
        return (value.Length - value.Replace(substring, "").Length) / substring.Length;
    }

    /// <summary>Check if string is numeric</summary>
    public static bool IsNumeric(this string value)
    {
        return !string.IsNullOrEmpty(value) && value.All(char.IsDigit);
    }

    /// <summary>Remove duplicate whitespace</summary>
    public static string RemoveDuplicateWhitespace(this string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }
}
