#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Exceptions;

/// <summary>
/// Exception thrown when network-related operations fail (HTTP requests, DNS resolution, etc.).
/// </summary>
public class NetworkException : CoubDownloaderException
{
    public string? Url { get; set; }
    public int? HttpStatusCode { get; set; }
    public bool IsTimeout { get; set; }

    public NetworkException(string message) : base(message) { }

    public NetworkException(string message, string url) : base(message)
    {
        Url = url;
    }

    public NetworkException(string message, string url, int statusCode) : base(message)
    {
        Url = url;
        HttpStatusCode = statusCode;
    }

    public NetworkException(string message, Exception inner) : base(message, inner) { }

    public NetworkException(string message, string url, Exception inner) : base(message, inner)
    {
        Url = url;
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        var details = new List<string>();
        if (Url != null) details.Add($"URL: {Url}");
        if (HttpStatusCode.HasValue) details.Add($"HTTP Status: {HttpStatusCode}");
        if (IsTimeout) details.Add("Timeout: true");

        if (details.Count > 0)
        {
            return $"{baseString}\n{string.Join(" | ", details)}";
        }
        return baseString;
    }
}