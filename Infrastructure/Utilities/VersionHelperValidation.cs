#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace CoubDownloader.Infrastructure.Utilities;

/// <summary>Validation helpers for ApplicationInfo</summary>
public static class VersionHelperValidation
{
    /// <summary>Validates an ApplicationInfo instance</summary>
    /// <param name="value">The ApplicationInfo instance to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this ApplicationInfo? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate AppVersion
        if (string.IsNullOrWhiteSpace(value.AppVersion))
        {
            problems.Add("AppVersion cannot be null or whitespace");
        }
        else if (!IsValidVersionFormat(value.AppVersion))
        {
            problems.Add($"AppVersion '{value.AppVersion}' has invalid version format");
        }

        // Validate RuntimeVersion
        if (string.IsNullOrWhiteSpace(value.RuntimeVersion))
        {
            problems.Add("RuntimeVersion cannot be null or whitespace");
        }
        else if (!IsValidVersionFormat(value.RuntimeVersion))
        {
            problems.Add($"RuntimeVersion '{value.RuntimeVersion}' has invalid version format");
        }

        // Validate OperatingSystem
        if (string.IsNullOrWhiteSpace(value.OperatingSystem))
        {
            problems.Add("OperatingSystem cannot be null or whitespace");
        }

        // Validate ProcessorCount
        if (value.ProcessorCount <= 0)
        {
            problems.Add("ProcessorCount must be greater than 0");
        }

        // Validate BuildDate
        if (value.BuildDate == default)
        {
            problems.Add("BuildDate cannot be default (Unix epoch)");
        }
        else if (value.BuildDate > DateTime.UtcNow.AddDays(1))
        {
            problems.Add("BuildDate cannot be in the future");
        }
        else if (value.BuildDate < new DateTime(2000, 1, 1))
        {
            problems.Add("BuildDate appears to be before year 2000");
        }

        return problems.AsReadOnly();
    }

    /// <summary>Checks if an ApplicationInfo instance is valid</summary>
    /// <param name="value">The ApplicationInfo instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this ApplicationInfo? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>Ensures an ApplicationInfo instance is valid, throwing if not</summary>
    /// <param name="value">The ApplicationInfo instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid, containing all validation problems</exception>
    public static void EnsureValid(this ApplicationInfo? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ApplicationInfo validation failed:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>Validates if a version string has a valid format</summary>
    /// <param name="version">The version string to validate</param>
    /// <returns>True if valid version format, false otherwise</returns>
    private static bool IsValidVersionFormat(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        try
        {
            _ = System.Version.Parse(version);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
