#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Exceptions;

/// <summary>
/// Exception thrown when there is an issue with application configuration.
/// </summary>
public class ConfigurationException : CoubDownloaderException
{
    public string? ConfigurationKey { get; set; }

    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">The message that describes the configuration error.</param>
    public ConfigurationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified error message and configuration key.
    /// </summary>
    /// <param name="message">The message that describes the configuration error.</param>
    /// <param name="configurationKey">The configuration key that caused the error.</param>
    public ConfigurationException(string message, string configurationKey) : base(message)
    {
        ConfigurationKey = configurationKey;
    }

    /// <summary>
    /// Initializes a new instance with the specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the configuration error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public ConfigurationException(string message, Exception inner) : base(message, inner) { }

    /// <summary>
    /// Initializes a new instance with the specified error message, configuration key and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the configuration error.</param>
    /// <param name="configurationKey">The configuration key that caused the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public ConfigurationException(string message, string configurationKey, Exception inner) : base(message, inner)
    {
        ConfigurationKey = configurationKey;
    }
}