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

    public ConfigurationException(string message) : base(message) { }

    public ConfigurationException(string message, string configurationKey) : base(message)
    {
        ConfigurationKey = configurationKey;
    }

    public ConfigurationException(string message, Exception inner) : base(message, inner) { }

    public ConfigurationException(string message, string configurationKey, Exception inner) : base(message, inner)
    {
        ConfigurationKey = configurationKey;
    }
}