#nullable enable

using System;
using System.Collections.Generic;
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
        ValidatePath(value.LoggingDirectory, nameof(value.LoggingDirectory), errors, allowRelative: true);

        // Validate DownloadDirectory
        ValidatePath(value.DownloadDirectory, nameof(value.DownloadDirectory), errors, allowRelative: true);

        // Validate ConfigFilePath
        ValidatePath(value.ConfigFilePath, nameof(value.ConfigFilePath), errors, allowRelative: false);

        if (!string.IsNullOrWhiteSpace(value.ConfigFilePath))
        {
            ValidateConfigFileExtension(value.ConfigFilePath, errors);
        }

        // Validate FFmpegPath
        ValidatePath(value.FFmpegPath, nameof(value.FFmpegPath), errors, allowRelative: false);

        return errors.AsReadOnly();
    }

    private static void ValidatePath(string? path, string propertyName, List<string> errors, bool allowRelative)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            errors.Add($"{propertyName} cannot be null or empty.");
            return;
        }

        if (path.Length > 260)
        {
            errors.Add($"{propertyName} cannot exceed 260 characters.");
        }

        if (!allowRelative && !Path.IsPathRooted(path))
        {
            errors.Add($"{propertyName} must be an absolute path.");
        }

        if (path.Contains(".."))
        {
            errors.Add($"{propertyName} cannot contain relative path traversal (..).");
        }
    }

    private static void ValidateConfigFileExtension(string configFilePath, List<string> errors)
    {
        if (!Path.HasExtension(configFilePath))
        {
            errors.Add("Configuration file path must have a file extension.");
            return;
        }

        var extension = Path.GetExtension(configFilePath);
        if (!string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extension, ".config", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Configuration file path should have a common configuration file extension (.json, .xml, .config).");
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="StartupConfiguration"/> instance is valid.
    /// </summary>
    /// <param name="value">The configuration to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this StartupConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);
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
