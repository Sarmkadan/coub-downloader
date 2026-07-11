#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CoubDownloader.Application.Startup;

/// <summary>
/// Provides validation helpers for <see cref="ApplicationStartup"/> and <see cref="StartupConfiguration"/> classes
/// </summary>
public static class ApplicationStartupValidation
{
    /// <summary>
    /// Validates a <see cref="StartupConfiguration"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The configuration to validate</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this StartupConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate LoggingDirectory
        if (string.IsNullOrWhiteSpace(value.LoggingDirectory))
        {
            errors.Add("Logging directory path cannot be null or empty.");
        }
        else if (!Path.IsPathRooted(value.LoggingDirectory) && value.LoggingDirectory.Contains(".."))
        {
            errors.Add("Logging directory path cannot contain relative path traversal (..).");
        }
        else if (value.LoggingDirectory.Length > 260)
        {
            errors.Add("Logging directory path cannot exceed 260 characters.");
        }

        // Validate DownloadDirectory
        if (string.IsNullOrWhiteSpace(value.DownloadDirectory))
        {
            errors.Add("Download directory path cannot be null or empty.");
        }
        else if (!Path.IsPathRooted(value.DownloadDirectory) && value.DownloadDirectory.Contains(".."))
        {
            errors.Add("Download directory path cannot contain relative path traversal (..).");
        }
        else if (value.DownloadDirectory.Length > 260)
        {
            errors.Add("Download directory path cannot exceed 260 characters.");
        }

        // Validate ConfigFilePath
        if (string.IsNullOrWhiteSpace(value.ConfigFilePath))
        {
            errors.Add("Configuration file path cannot be null or empty.");
        }
        else if (value.ConfigFilePath.Length > 260)
        {
            errors.Add("Configuration file path cannot exceed 260 characters.");
        }
        else if (!Path.HasExtension(value.ConfigFilePath))
        {
            errors.Add("Configuration file path must have a file extension.");
        }
        else if (!value.ConfigFilePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                 !value.ConfigFilePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
                 !value.ConfigFilePath.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Configuration file path should have a common configuration file extension (.json, .xml, .config).");
        }

        // Validate FFmpegPath
        if (string.IsNullOrWhiteSpace(value.FFmpegPath))
        {
            errors.Add("FFmpeg path cannot be null or empty.");
        }
        else if (value.FFmpegPath.Length > 260)
        {
            errors.Add("FFmpeg path cannot exceed 260 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="StartupConfiguration"/> instance is valid.
    /// </summary>
    /// <param name="value">The configuration to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this StartupConfiguration value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="StartupConfiguration"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The configuration to validate</param>
    /// <exception cref="ArgumentException">Thrown if the configuration is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static void EnsureValid(this StartupConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Startup configuration is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}
