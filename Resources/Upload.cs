using System.Net.Http.Headers;
using System.Text.Json;

namespace Apertur.Sdk.Resources;

/// <summary>
/// Uploads images to a session, with optional client-side encryption.
/// </summary>
public class Upload
{
    private readonly AperturHttpClient _http;

    /// <summary>
    /// Initializes a new <see cref="Upload"/> resource.
    /// </summary>
    /// <param name="http">The HTTP client to use for requests.</param>
    public Upload(AperturHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Uploads an image from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="stream">The image data stream.</param>
    /// <param name="filename">The filename for the uploaded image.</param>
    /// <param name="mimeType">The MIME type of the image (default: <c>image/jpeg</c>).</param>
    /// <param name="source">An optional source identifier.</param>
    /// <param name="password">An optional session password.</param>
    /// <returns>The upload result containing the image ID and delivery information.</returns>
    public async Task<JsonElement> ImageAsync(
        string uuid,
        Stream stream,
        string filename = "image.jpg",
        string mimeType = "image/jpeg",
        string? source = null,
        string? password = null)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        content.Add(streamContent, "file", filename);

        if (source is not null)
        {
            content.Add(new StringContent(source), "source");
        }

        Dictionary<string, string>? headers = null;
        if (password is not null)
        {
            headers = new Dictionary<string, string> { ["x-session-password"] = password };
        }

        return await _http.RequestMultipartAsync<JsonElement>(
            HttpMethod.Post,
            $"/api/v1/upload/{uuid}/images",
            content,
            headers).ConfigureAwait(false);
    }

    /// <summary>
    /// Uploads an image from a file path.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="filePath">The path to the image file.</param>
    /// <param name="filename">The filename for the uploaded image. Defaults to the file name from <paramref name="filePath"/>.</param>
    /// <param name="mimeType">The MIME type of the image (default: <c>image/jpeg</c>).</param>
    /// <param name="source">An optional source identifier.</param>
    /// <param name="password">An optional session password.</param>
    /// <returns>The upload result containing the image ID and delivery information.</returns>
    public async Task<JsonElement> ImageAsync(
        string uuid,
        string filePath,
        string? filename = null,
        string mimeType = "image/jpeg",
        string? source = null,
        string? password = null)
    {
        using var stream = File.OpenRead(filePath);
        return await ImageAsync(
            uuid,
            stream,
            filename ?? Path.GetFileName(filePath),
            mimeType,
            source,
            password).ConfigureAwait(false);
    }

    /// <summary>
    /// Uploads an image from a byte array.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="bytes">The raw image bytes.</param>
    /// <param name="filename">The filename for the uploaded image (default: <c>image.jpg</c>).</param>
    /// <param name="mimeType">The MIME type of the image (default: <c>image/jpeg</c>).</param>
    /// <param name="source">An optional source identifier.</param>
    /// <param name="password">An optional session password.</param>
    /// <returns>The upload result containing the image ID and delivery information.</returns>
    public async Task<JsonElement> ImageAsync(
        string uuid,
        byte[] bytes,
        string filename = "image.jpg",
        string mimeType = "image/jpeg",
        string? source = null,
        string? password = null)
    {
        using var stream = new MemoryStream(bytes);
        return await ImageAsync(uuid, stream, filename, mimeType, source, password)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Uploads an image with client-side encryption. The image is encrypted using AES-256-GCM
    /// and the symmetric key is wrapped with the provided RSA public key.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="imageData">The raw image bytes.</param>
    /// <param name="publicKeyPem">The RSA public key in PEM format (obtain via <c>Encryption.GetServerKeyAsync</c>).</param>
    /// <param name="filename">The filename for the uploaded image (default: <c>image.jpg</c>).</param>
    /// <param name="mimeType">The MIME type of the image (default: <c>image/jpeg</c>).</param>
    /// <param name="source">An optional source identifier (default: <c>sdk</c>).</param>
    /// <param name="password">An optional session password.</param>
    /// <returns>The upload result containing the image ID and delivery information.</returns>
    public async Task<JsonElement> ImageEncryptedAsync(
        string uuid,
        byte[] imageData,
        string publicKeyPem,
        string filename = "image.jpg",
        string mimeType = "image/jpeg",
        string? source = null,
        string? password = null)
    {
        var encrypted = Crypto.EncryptImage(imageData, publicKeyPem);

        var payload = new Dictionary<string, object>
        {
            ["encrypted_key"] = encrypted.EncryptedKey,
            ["iv"] = encrypted.Iv,
            ["encrypted_data"] = encrypted.EncryptedData,
            ["algorithm"] = encrypted.Algorithm,
            ["filename"] = filename,
            ["mime_type"] = mimeType,
            ["source"] = source ?? "sdk"
        };

        var jsonBody = JsonSerializer.Serialize(payload, AperturHttpClient.JsonOptions);

        var headers = new Dictionary<string, string>
        {
            ["X-Aptr-Encrypted"] = "default"
        };

        if (password is not null)
        {
            headers["x-session-password"] = password;
        }

        return await _http.RequestJsonStringAsync<JsonElement>(
            HttpMethod.Post,
            $"/api/v1/upload/{uuid}/images",
            jsonBody,
            headers).ConfigureAwait(false);
    }
}
