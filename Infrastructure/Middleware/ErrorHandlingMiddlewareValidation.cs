#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Middleware;

/// <summary>Provides validation logic for <see cref="ErrorHandlingMiddleware"/> instances.</summary>
/// <remarks>
/// This static class contains extension methods for validating the state of ErrorHandlingMiddleware
/// to ensure it's properly initialized before use.
/// </remarks>
public static class ErrorHandlingMiddlewareValidation
{
    /// <summary>Validates an ErrorHandlingMiddleware instance.</summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ErrorHandlingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.ValidateInternal();
    }

    /// <summary>Determines whether the specified middleware instance is valid.</summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns><see langword="true"/> if the middleware instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ErrorHandlingMiddleware value)
        => value.Validate().Count == 0;

    /// <summary>Ensures that the specified middleware instance is valid, throwing an exception if it is not.</summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is invalid. The exception message contains the validation problems.</exception>
    public static void EnsureValid(this ErrorHandlingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.ValidateInternal();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ErrorHandlingMiddleware is invalid. Problems: {string.Join("; ", problems)}",
                nameof(value));
        }
    }
}