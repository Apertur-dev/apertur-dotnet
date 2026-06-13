namespace Apertur.Sdk.Exceptions;

/// <summary>
/// Thrown when the API returns a 400 Bad Request response due to invalid input.
/// </summary>
public class ValidationException : AperturException
{
    /// <summary>
    /// Initializes a new <see cref="ValidationException"/>.
    /// </summary>
    /// <param name="message">A human-readable error message.</param>
    public ValidationException(string message)
        : base(400, message, "VALIDATION_ERROR")
    {
    }
}
