namespace Apertur.Sdk.Exceptions;

/// <summary>
/// Thrown when the API returns a 401 Unauthorized response.
/// </summary>
public class AuthenticationException : AperturException
{
    /// <summary>
    /// Initializes a new <see cref="AuthenticationException"/>.
    /// </summary>
    /// <param name="message">A human-readable error message.</param>
    public AuthenticationException(string message = "Authentication failed")
        : base(401, message, "AUTHENTICATION_FAILED")
    {
    }
}
