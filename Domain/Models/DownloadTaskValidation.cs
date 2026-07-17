#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace CoubDownloader.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="DownloadTask"/> instances.
/// </summary>
public static class DownloadTaskValidation
{
    /// <summary>
    /// Validates a <see cref="DownloadTask"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The download task to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
	public static IReadOnlyList<string> Validate(this DownloadTask value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = new List<string>();

        // Validate required string properties
		ValidateRequiredString(value.Id, nameof(value.Id), errors);
		ValidateRequiredString(value.VideoId, nameof(value.VideoId), errors);
		ValidateRequiredString(value.Url, nameof(value.Url), errors);
		ValidateRequiredString(value.OutputPath, nameof(value.OutputPath), errors);

        // Validate ProgressPercent range (0-100)
		if (value.ProgressPercent < 0 || value.ProgressPercent > 100)
		{
			errors.Add($"{nameof(DownloadTask.ProgressPercent)} must be between 0 and 100, but was {value.ProgressPercent}.");
		}

        // Validate FileSizeBytes (non-negative)
		if (value.FileSizeBytes < 0)
		{
			errors.Add($"{nameof(DownloadTask.FileSizeBytes)} must be non-negative, but was {value.FileSizeBytes}.");
		}

        // Validate RetryCount range (0-10 based on attribute in DownloadTask)
		if (value.RetryCount < 0 || value.RetryCount > 10)
		{
			errors.Add($"{nameof(DownloadTask.RetryCount)} must be between 0 and 10, but was {value.RetryCount}.");
		}

        // Validate MaxRetries range (1-10 based on attribute in DownloadTask)
		if (value.MaxRetries < 1 || value.MaxRetries > 10)
		{
			errors.Add($"{nameof(DownloadTask.MaxRetries)} must be between 1 and 10, but was {value.MaxRetries}.");
		}

        // Validate CreatedAt (must be in the past)
		if (value.CreatedAt > DateTime.UtcNow)
		{
			errors.Add($"{nameof(DownloadTask.CreatedAt)} must be in the past, but was {value.CreatedAt:O}.");
		}

        // Validate UpdatedAt (must be >= CreatedAt)
		if (value.UpdatedAt < value.CreatedAt)
		{
			errors.Add($"{nameof(DownloadTask.UpdatedAt)} must be greater than or equal to {nameof(DownloadTask.CreatedAt)}, but was {value.UpdatedAt:O}.");
		}

        // Validate StartedAt/CompletedAt consistency
		if (value.StartedAt.HasValue && value.CompletedAt.HasValue && value.CompletedAt < value.StartedAt)
		{
			errors.Add($"{nameof(DownloadTask.CompletedAt)} must be greater than or equal to {nameof(DownloadTask.StartedAt)}, but was {value.CompletedAt:O}.");
		}

        // Validate that CompletedAt is not in the future
		if (value.CompletedAt.HasValue && value.CompletedAt > DateTime.UtcNow)
		{
			errors.Add($"{nameof(DownloadTask.CompletedAt)} must not be in the future, but was {value.CompletedAt:O}.");
		}

        // Validate that StartedAt is not in the future
		if (value.StartedAt.HasValue && value.StartedAt > DateTime.UtcNow)
		{
			errors.Add($"{nameof(DownloadTask.StartedAt)} must not be in the future, but was {value.StartedAt:O}.");
		}

		return errors.AsReadOnly();
	}

    /// <summary>
    /// Checks if a <see cref="DownloadTask"/> instance is valid.
    /// </summary>
    /// <param name="value">The download task to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
	public static bool IsValid(this DownloadTask value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return value.Validate().Count == 0;
	}

    /// <summary>
    /// Ensures that a <see cref="DownloadTask"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The download task to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid, containing all validation errors</exception>
	public static void EnsureValid(this DownloadTask value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = value.Validate();
		if (errors.Count == 0)
		{
			return;
		}

		throw new ArgumentException(
			$"DownloadTask is invalid. Validation failed with {errors.Count} error(s):{Environment.NewLine}- ".Replace("\n-", "\n- ") +
			string.Join(Environment.NewLine + "- ", errors),
			nameof(value));
	}

	private static void ValidateRequiredString(string? value, string propertyName, List<string> errors)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			errors.Add($"{propertyName} is required and cannot be null, empty, or whitespace.");
		}
	}
}
