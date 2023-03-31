#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Domain.Exceptions;

/// <summary>
/// Exception thrown when validation fails for input parameters or data.
/// </summary>
public class ValidationException : CoubDownloaderException
{
    public string? ParameterName { get; set; }
    public object? InvalidValue { get; set; }

    public ValidationException(string message) : base(message) { }

    public ValidationException(string message, string parameterName, object? invalidValue) : base(message)
    {
        ParameterName = parameterName;
        InvalidValue = invalidValue;
    }

    public ValidationException(string message, Exception inner) : base(message, inner) { }

    public override string ToString()
    {
        var baseString = base.ToString();
        if (ParameterName != null || InvalidValue != null)
        {
            return $"{baseString}\nParameter: {ParameterName}\nInvalid Value: {InvalidValue}";
        }
        return baseString;
    }
}