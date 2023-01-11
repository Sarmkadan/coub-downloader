// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>Input validation and sanitization utilities</summary>
public static class ValidationHelper
{
    /// <summary>Validate email format</summary>
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Validate URL format</summary>
    public static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>Validate IP address</summary>
    public static bool IsValidIpAddress(string ipAddress)
    {
        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }

    /// <summary>Validate file path</summary>
    public static bool IsValidFilePath(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Sanitize filename</summary>
    public static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
    }

    /// <summary>Validate video URL (Coub format)</summary>
    public static bool IsValidCoubUrl(string url)
    {
        if (!IsValidUrl(url)) return false;
        return url.Contains("coub.com") && (url.Contains("/view/") || url.Contains("coub.com/"));
    }

    /// <summary>Validate bitrate value</summary>
    public static bool IsValidBitrate(int bitrate)
    {
        return bitrate > 0 && bitrate <= 50000; // 0-50 Mbps
    }

    /// <summary>Validate resolution</summary>
    public static bool IsValidResolution(int width, int height)
    {
        return width > 0 && height > 0 && width <= 8192 && height <= 8192;
    }

    /// <summary>Validate frame rate</summary>
    public static bool IsValidFrameRate(int fps)
    {
        return fps >= 1 && fps <= 120;
    }

    /// <summary>Validate duration in seconds</summary>
    public static bool IsValidDuration(double seconds)
    {
        return seconds > 0 && seconds <= 86400 * 365; // Up to 1 year
    }

    /// <summary>Check if directory path is safe (no path traversal)</summary>
    public static bool IsSafeDirectoryPath(string basePath, string requestedPath)
    {
        try
        {
            var fullBasePath = Path.GetFullPath(basePath);
            var fullRequestedPath = Path.GetFullPath(requestedPath);

            return fullRequestedPath.StartsWith(fullBasePath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Validate batch size</summary>
    public static bool IsValidBatchSize(int size)
    {
        return size > 0 && size <= 10000;
    }

    /// <summary>Check if string matches pattern</summary>
    public static bool MatchesPattern(string value, string pattern)
    {
        try
        {
            return Regex.IsMatch(value, pattern);
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>Builder for fluent validation</summary>
public class ValidationBuilder
{
    private readonly List<(string field, string message)> _errors = [];

    /// <summary>Validate that value is not null or empty</summary>
    public ValidationBuilder RequireNotEmpty(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            _errors.Add((fieldName, $"{fieldName} cannot be empty"));

        return this;
    }

    /// <summary>Validate that value matches pattern</summary>
    public ValidationBuilder RequirePattern(string value, string pattern, string fieldName)
    {
        if (!ValidationHelper.MatchesPattern(value, pattern))
            _errors.Add((fieldName, $"{fieldName} has invalid format"));

        return this;
    }

    /// <summary>Validate that value is in range</summary>
    public ValidationBuilder RequireRange(int value, int min, int max, string fieldName)
    {
        if (value < min || value > max)
            _errors.Add((fieldName, $"{fieldName} must be between {min} and {max}"));

        return this;
    }

    /// <summary>Add custom validation error</summary>
    public ValidationBuilder AddError(string fieldName, string message)
    {
        _errors.Add((fieldName, message));
        return this;
    }

    /// <summary>Get all validation errors</summary>
    public List<(string field, string message)> GetErrors() => _errors;

    /// <summary>Check if validation passed</summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>Throw if validation failed</summary>
    public void ThrowIfInvalid()
    {
        if (!IsValid)
        {
            var messages = string.Join("; ", _errors.Select(e => $"{e.field}: {e.message}"));
            throw new ArgumentException(messages);
        }
    }
}
