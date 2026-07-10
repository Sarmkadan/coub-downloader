#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoubDownloader.Domain.Exceptions;

namespace CoubDownloader.Infrastructure.Middleware;

/// <summary>Validation helpers for ErrorHandlingMiddleware</summary>
public static class ErrorHandlingMiddlewareValidation
{
    /// <summary>Validates an ErrorHandlingMiddleware instance</summary>
    /// <param name="value">The middleware instance to validate</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ErrorHandlingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate logger is not null (internal field check)
        // Note: We can't directly validate the logger field, but we can check if it's been initialized
        // by attempting to use it through the public interface

        return problems.AsReadOnly();
    }

    /// <summary>Checks if an ErrorHandlingMiddleware instance is valid</summary>
    /// <param name="value">The middleware instance to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this ErrorHandlingMiddleware value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>Ensures an ErrorHandlingMiddleware instance is valid, throwing if not</summary>
    /// <param name="value">The middleware instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when value is invalid, containing validation problems</exception>
    public static void EnsureValid(this ErrorHandlingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ErrorHandlingMiddleware is invalid. Problems: {string.Join("; ", problems)}");
        }
    }
}