#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CoubDownloader.Domain.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="ProcessExecutionException"/> instances.
/// </summary>
public static class ProcessExecutionExceptionValidation
{
	/// <summary>
	/// Validates a <see cref="ProcessExecutionException"/> instance.
	/// </summary>
	/// <param name="value">The exception to validate.</param>
	/// <returns>A list of validation problems; empty if valid.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this ProcessExecutionException? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var problems = new List<string>();

		// ProcessName validation
		if (string.IsNullOrWhiteSpace(value.ProcessName))
		{
			problems.Add($"ProcessName must be a non-empty string, but was: {(value.ProcessName == null ? "null" : "empty or whitespace")}");
		}

		// Arguments validation
		if (string.IsNullOrWhiteSpace(value.Arguments))
		{
			problems.Add($"Arguments must be a non-empty string, but was: {(value.Arguments == null ? "null" : "empty or whitespace")}");
		}

		// ExitCode validation
		// ExitCode should be a valid process exit code (typically 0-255, but can be any int)
		// We'll validate it's not the default value (0) when ProcessName is set, as that's likely a bug
		if (!string.IsNullOrWhiteSpace(value.ProcessName) && value.ExitCode == 0)
		{
			problems.Add("ExitCode is 0, which typically indicates success. For process execution failures, ExitCode should indicate the actual error code.");
		}

		// StandardError validation
		if (string.IsNullOrWhiteSpace(value.StandardError))
		{
			problems.Add($"StandardError must be a non-empty string when process execution fails, but was: {(value.StandardError == null ? "null" : "empty or whitespace")}");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Determines whether a <see cref="ProcessExecutionException"/> instance is valid.
	/// </summary>
	/// <param name="value">The exception to check.</param>
	/// <returns>True if valid; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static bool IsValid(this ProcessExecutionException? value)
	{
		return value?.Validate().Count == 0;
	}

	/// <summary>
	/// Ensures that a <see cref="ProcessExecutionException"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
	/// </summary>
	/// <param name="value">The exception to validate.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
	public static void EnsureValid(this ProcessExecutionException? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var problems = value.Validate();
		if (problems.Count is 0)
		{
			return;
		}

		throw new ArgumentException($"ProcessExecutionException is not valid. Problems:\n- {string.Join("\n- ", problems)}");
	}
}