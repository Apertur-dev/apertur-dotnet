using System.Text.Json;

namespace Apertur.Sdk.Resources;

/// <summary>
/// Retrieves encryption keys from the Apertur API for client-side image encryption.
/// </summary>
public class Encryption
{
    private readonly AperturHttpClient _http;

    /// <summary>
    /// Initializes a new <see cref="Encryption"/> resource.
    /// </summary>
    /// <param name="http">The HTTP client to use for requests.</param>
    public Encryption(AperturHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Retrieves the server's RSA public key for client-side encryption.
    /// Pass the returned key to <see cref="Upload.ImageEncryptedAsync"/> or
    /// <see cref="Crypto.EncryptImage"/> to encrypt images before upload.
    /// </summary>
    /// <returns>A JSON element containing the <c>public_key</c> field.</returns>
    public async Task<JsonElement> GetServerKeyAsync()
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            "/api/v1/encryption/server-key").ConfigureAwait(false);
    }
}
