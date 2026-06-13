namespace Apertur.Sdk.Exceptions;

/// <summary>
/// Thrown when the API returns a 429 Too Many Requests response.
/// </summary>
public class RateLimitException : AperturException
{
    /// <summary>
    /// Number of seconds to wait before retrying, if provided by the API via the
    /// <c>Retry-After</c> header.
    /// </summary>
    public int? RetryAfter { get; }

    /// <summary>
    /// Initializes a new <see cref="RateLimitException"/>.
    /// </summary>
    /// <param name="message">A human-readable error message.</param>
    /// <param name="retryAfter">Optional retry-after duration in seconds.</param>
    public RateLimitException(string message, int? retryAfter = null)
        : base(429, message, "RATE_LIMIT")
    {
        RetryAfter = retryAfter;
    }
}
