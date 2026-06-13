namespace Apertur.Sdk.Exceptions;

/// <summary>
/// Base exception for all Apertur API errors.
/// </summary>
public class AperturException : Exception
{
    /// <summary>
    /// HTTP status code returned by the API.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Machine-readable error code returned by the API, if any.
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// Initializes a new <see cref="AperturException"/>.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="message">A human-readable error message.</param>
    /// <param name="code">An optional machine-readable error code.</param>
    public AperturException(int statusCode, string message, string? code = null)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }

    /// <summary>
    /// Initializes a new <see cref="AperturException"/> with an inner exception.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="message">A human-readable error message.</param>
    /// <param name="code">An optional machine-readable error code.</param>
    /// <param name="innerException">The inner exception.</param>
    public AperturException(int statusCode, string message, string? code, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Code = code;
    }
}
