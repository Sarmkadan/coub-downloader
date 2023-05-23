#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text;

namespace CoubDownloader.Domain.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="ConfigurationException"/> to enhance error handling and diagnostics.
/// </summary>
public static class ConfigurationExceptionExtensions
{
    /// <summary>
    /// Creates a detailed error message that includes the configuration key, if available.
    /// </summary>
    /// <param name="exception">The configuration exception to format.</param>
    /// <returns>A formatted error message string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    public static string GetDetailedMessage(this ConfigurationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine(exception.Message);

        if (!string.IsNullOrEmpty(exception.ConfigurationKey))
        {
            messageBuilder.AppendLine($"Configuration Key: {exception.ConfigurationKey}");
        }

        if (exception.InnerException is not null)
        {
            messageBuilder.AppendLine($"Inner Exception: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}");
        }

        return messageBuilder.ToString();
    }

    /// <summary>
    /// Determines whether the exception is related to a specific configuration key.
    /// </summary>
    /// <param name="exception">The configuration exception to check.</param>
    /// <param name="key">The configuration key to match against.</param>
    /// <returns>True if the exception's configuration key matches the specified key; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/> or empty</exception>
    public static bool IsForKey(this ConfigurationException exception, string key)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return string.Equals(exception.ConfigurationKey, key, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a new ConfigurationException with the same configuration key but a modified message.
    /// </summary>
    /// <param name="exception">The original configuration exception.</param>
    /// <param name="newMessage">The new error message.</param>
    /// <returns>A new ConfigurationException instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="newMessage"/> is <see langword="null"/> or empty</exception>
    public static ConfigurationException WithMessage(this ConfigurationException exception, string newMessage)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(newMessage);

        return exception.ConfigurationKey is null
            ? new ConfigurationException(newMessage)
            : new ConfigurationException(newMessage, exception.ConfigurationKey);
    }

    /// <summary>
    /// Creates a new ConfigurationException that includes the original exception as inner exception.
    /// </summary>
    /// <param name="exception">The original configuration exception.</param>
    /// <param name="additionalContext">Additional context to include in the message.</param>
    /// <returns>A new ConfigurationException instance with the original as inner exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="additionalContext"/> is <see langword="null"/> or empty</exception>
    public static ConfigurationException WithContext(this ConfigurationException exception, string additionalContext)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(additionalContext);

        var newMessage = $"{exception.Message} ({additionalContext}){Environment.NewLine}{exception.GetDetailedMessage()}";
        return exception.ConfigurationKey is null
            ? new ConfigurationException(newMessage, exception)
            : new ConfigurationException(newMessage, exception.ConfigurationKey, exception);
    }
}