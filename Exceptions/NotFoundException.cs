namespace Apertur.Sdk.Exceptions;

/// <summary>
/// Thrown when the API returns a 404 Not Found response.
/// </summary>
public class NotFoundException : AperturException
{
    /// <summary>
    /// Initializes a new <see cref="NotFoundException"/>.
    /// </summary>
    /// <param name="message">A human-readable error message.</param>
    public NotFoundException(string message = "Not found")
        : base(404, message, "NOT_FOUND")
    {
    }
}
