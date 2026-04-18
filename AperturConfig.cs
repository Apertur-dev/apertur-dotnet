namespace Apertur.Sdk;

/// <summary>
/// Configuration for the Apertur API client.
/// </summary>
public class AperturConfig
{
    /// <summary>
    /// API key for authentication (prefixed <c>aptr_</c> or <c>aptr_test_</c>).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// OAuth token for third-party authentication.
    /// </summary>
    public string? OAuthToken { get; set; }

    /// <summary>
    /// Override the base URL. When omitted the URL is inferred from the API key prefix:
    /// <c>aptr_test_</c> keys target <c>https://sandbox.api.aptr.ca</c>, all others target
    /// <c>https://api.aptr.ca</c>.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Explicitly set the environment. When omitted the environment is auto-detected
    /// from the API key prefix.
    /// </summary>
    public AperturEnvironment? Environment { get; set; }

    /// <summary>
    /// Optional <see cref="HttpClient"/> instance. When provided the client will use it
    /// instead of creating its own. Useful for dependency injection and testing.
    /// </summary>
    public HttpClient? HttpClient { get; set; }
}

/// <summary>
/// The target environment for the Apertur API.
/// </summary>
public enum AperturEnvironment
{
    /// <summary>Production environment.</summary>
    Live,

    /// <summary>Sandbox / test environment.</summary>
    Test
}
