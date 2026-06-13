using System.Text.Json;

namespace Apertur.Sdk.Resources;

/// <summary>
/// Manages upload sessions. Create sessions, retrieve session info, generate QR codes,
/// verify passwords and check delivery status.
/// </summary>
public class Sessions
{
    private readonly AperturHttpClient _http;

    /// <summary>
    /// Initializes a new <see cref="Sessions"/> resource.
    /// </summary>
    /// <param name="http">The HTTP client to use for requests.</param>
    public Sessions(AperturHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Creates a new upload session.
    /// </summary>
    /// <param name="options">Optional session creation parameters.</param>
    /// <returns>The created session details including the upload URL and QR code URL.</returns>
    public async Task<JsonElement> CreateAsync(Dictionary<string, object>? options = null)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Post,
            "/api/v1/upload-sessions",
            options).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves session information by UUID.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <returns>The session details.</returns>
    public async Task<JsonElement> GetAsync(string uuid)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/upload/{uuid}/session").ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an existing upload session.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="options">The fields to update.</param>
    /// <returns>The updated session details.</returns>
    public async Task<JsonElement> UpdateAsync(string uuid, Dictionary<string, object> options)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Patch,
            $"/api/v1/upload-sessions/{uuid}",
            options).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists sessions with pagination.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated list of sessions.</returns>
    public async Task<JsonElement> ListAsync(int? page = null, int? pageSize = null)
    {
        var query = QueryHelper.Build(
            ("page", page?.ToString()),
            ("pageSize", pageSize?.ToString()));

        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/sessions{query}").ConfigureAwait(false);
    }

    /// <summary>
    /// Lists recent sessions.
    /// </summary>
    /// <param name="limit">Maximum number of sessions to return.</param>
    /// <returns>An array of recent sessions.</returns>
    public async Task<JsonElement> RecentAsync(int? limit = null)
    {
        var query = QueryHelper.Build(("limit", limit?.ToString()));

        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/sessions/recent{query}").ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the QR code image for a session.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="options">Optional QR code rendering options (format, size, style, fg, bg, borderSize, borderColor).</param>
    /// <returns>The QR code image as raw bytes.</returns>
    public async Task<byte[]> QrAsync(string uuid, Dictionary<string, string>? options = null)
    {
        var pairs = options?.Select(kvp => (kvp.Key, (string?)kvp.Value)).ToArray()
            ?? Array.Empty<(string, string?)>();
        var query = QueryHelper.Build(pairs);

        return await _http.RequestRawAsync(
            HttpMethod.Get,
            $"/api/v1/upload-sessions/{uuid}/qr{query}").ConfigureAwait(false);
    }

    /// <summary>
    /// Verifies a password for a password-protected session.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="password">The password to verify.</param>
    /// <returns>A JSON element containing a <c>valid</c> boolean.</returns>
    public async Task<JsonElement> VerifyPasswordAsync(string uuid, string password)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Post,
            $"/api/v1/upload/{uuid}/verify-password",
            new Dictionary<string, object> { ["password"] = password }).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the delivery status for all files in a session.
    /// </summary>
    /// <remarks>
    /// The response is a JSON object with the shape
    /// <c>{ status, files: [...], lastChanged }</c> where <c>status</c> is one of
    /// <c>pending | active | completed | expired</c> and <c>lastChanged</c> is an ISO 8601
    /// timestamp.
    ///
    /// <para>
    /// When <paramref name="pollFrom"/> is provided, the server long-polls for up to 5 minutes
    /// and returns as soon as <c>lastChanged</c> advances past the supplied timestamp.
    /// Callers that rely on this long-poll mode should ensure the underlying
    /// <see cref="HttpClient"/> has a timeout of at least 6 minutes (e.g.
    /// <c>HttpClient.Timeout = TimeSpan.FromMinutes(6)</c>) or pass a
    /// <see cref="CancellationToken"/> whose deadline is at least 6 minutes out, so the
    /// server releases the response first under the happy path.
    /// </para>
    /// </remarks>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="pollFrom">Optional ISO 8601 timestamp; when set, the server long-polls until something changes.</param>
    /// <param name="ct">Optional cancellation token propagated to the HTTP send.</param>
    /// <returns>A JSON element matching <c>{ status, files, lastChanged }</c>.</returns>
    public async Task<JsonElement> DeliveryStatusAsync(
        string uuid,
        string? pollFrom = null,
        CancellationToken ct = default)
    {
        var query = QueryHelper.Build(("pollFrom", pollFrom));
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/upload-sessions/{uuid}/delivery-status{query}",
            ct: ct).ConfigureAwait(false);
    }
}
