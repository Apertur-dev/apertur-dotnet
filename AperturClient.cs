using Apertur.Sdk.Resources;

namespace Apertur.Sdk;

/// <summary>
/// The main entry point for the Apertur .NET SDK. Provides access to all API resources
/// via typed sub-clients.
/// </summary>
/// <example>
/// <code>
/// // Simple constructor with API key
/// var client = new AperturClient("aptr_your_key_here");
///
/// // Full configuration
/// var client = new AperturClient(new AperturConfig
/// {
///     ApiKey = "aptr_test_your_key",
///     BaseUrl = "https://sandbox.api.aptr.ca"
/// });
/// </code>
/// </example>
public class AperturClient
{
    private const string DefaultBaseUrl = "https://api.aptr.ca";
    private const string SandboxBaseUrl = "https://sandbox.api.aptr.ca";

    /// <summary>
    /// The resolved environment (<see cref="AperturEnvironment.Live"/> or
    /// <see cref="AperturEnvironment.Test"/>), inferred from the API key prefix
    /// unless explicitly set in the configuration.
    /// </summary>
    public AperturEnvironment Environment { get; }

    /// <summary>
    /// Upload session management: create, update, list, QR codes, password verification.
    /// </summary>
    public Sessions Sessions { get; }

    /// <summary>
    /// Image uploading with optional client-side encryption.
    /// </summary>
    public Upload Upload { get; }

    /// <summary>
    /// Query uploaded images across all sessions.
    /// </summary>
    public Uploads Uploads { get; }

    /// <summary>
    /// Long-polling for real-time image retrieval.
    /// </summary>
    public Polling Polling { get; }

    /// <summary>
    /// Delivery destination management.
    /// </summary>
    public Destinations Destinations { get; }

    /// <summary>
    /// API key management.
    /// </summary>
    public Keys Keys { get; }

    /// <summary>
    /// Event webhook management.
    /// </summary>
    public Webhooks Webhooks { get; }

    /// <summary>
    /// Server encryption key retrieval.
    /// </summary>
    public Encryption Encryption { get; }

    /// <summary>
    /// Account-level statistics.
    /// </summary>
    public Stats Stats { get; }

    /// <summary>
    /// Creates a new Apertur client with an API key. The environment and base URL are
    /// auto-detected from the key prefix.
    /// </summary>
    /// <param name="apiKey">An Apertur API key (prefixed <c>aptr_</c> or <c>aptr_test_</c>).</param>
    public AperturClient(string apiKey) : this(new AperturConfig { ApiKey = apiKey })
    {
    }

    /// <summary>
    /// Creates a new Apertur client with full configuration options.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    /// <exception cref="ArgumentException">Thrown when neither <c>ApiKey</c> nor <c>OAuthToken</c> is provided.</exception>
    public AperturClient(AperturConfig config)
    {
        if (string.IsNullOrEmpty(config.ApiKey) && string.IsNullOrEmpty(config.OAuthToken))
        {
            throw new ArgumentException("Either ApiKey or OAuthToken must be provided.");
        }

        // Resolve environment from key prefix or explicit config
        var token = config.ApiKey ?? config.OAuthToken ?? string.Empty;
        var detectedEnv = token.StartsWith("aptr_test_", StringComparison.Ordinal)
            ? AperturEnvironment.Test
            : AperturEnvironment.Live;
        Environment = config.Environment ?? detectedEnv;

        // Auto-select sandbox URL for test keys unless BaseUrl is explicitly set
        var baseUrl = config.BaseUrl
            ?? (Environment == AperturEnvironment.Test ? SandboxBaseUrl : DefaultBaseUrl);

        var http = new AperturHttpClient(baseUrl, token, config.HttpClient);

        Sessions = new Sessions(http);
        Upload = new Upload(http);
        Uploads = new Uploads(http);
        Polling = new Polling(http);
        Destinations = new Destinations(http);
        Keys = new Keys(http);
        Webhooks = new Webhooks(http);
        Encryption = new Encryption(http);
        Stats = new Stats(http);
    }
}
