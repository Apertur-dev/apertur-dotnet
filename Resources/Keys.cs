using System.Text.Json;

namespace Apertur.Sdk.Resources;

/// <summary>
/// Manages API keys and their default destinations for a project.
/// </summary>
public class Keys
{
    private readonly AperturHttpClient _http;

    /// <summary>
    /// Initializes a new <see cref="Keys"/> resource.
    /// </summary>
    /// <param name="http">The HTTP client to use for requests.</param>
    public Keys(AperturHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Lists all API keys for a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>An array of API keys.</returns>
    public async Task<JsonElement> ListAsync(string projectId)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/projects/{projectId}/keys").ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new API key in a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="options">The key creation parameters (label, maxImages, etc.).</param>
    /// <returns>The created key metadata and the plain-text key (shown only once).</returns>
    public async Task<JsonElement> CreateAsync(string projectId, Dictionary<string, object> options)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Post,
            $"/api/v1/projects/{projectId}/keys",
            options).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an existing API key.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="keyId">The key ID.</param>
    /// <param name="options">The fields to update.</param>
    /// <returns>The updated key.</returns>
    public async Task<JsonElement> UpdateAsync(string projectId, string keyId, Dictionary<string, object> options)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Patch,
            $"/api/v1/projects/{projectId}/keys/{keyId}",
            options).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes an API key.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="keyId">The key ID.</param>
    public async Task DeleteAsync(string projectId, string keyId)
    {
        await _http.RequestAsync<JsonElement>(
            HttpMethod.Delete,
            $"/api/v1/projects/{projectId}/keys/{keyId}").ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the default destinations and long-polling setting for an API key.
    /// </summary>
    /// <param name="keyId">The key ID.</param>
    /// <param name="destinationIds">The destination IDs to assign.</param>
    /// <param name="longPollingEnabled">Whether to enable long-polling for sessions created with this key.</param>
    /// <returns>The updated key destinations configuration.</returns>
    public async Task<JsonElement> SetDestinationsAsync(
        string keyId,
        string[] destinationIds,
        bool? longPollingEnabled = null)
    {
        var body = new Dictionary<string, object>
        {
            ["destination_ids"] = destinationIds
        };

        if (longPollingEnabled.HasValue)
        {
            body["long_polling_enabled"] = longPollingEnabled.Value;
        }

        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Put,
            $"/api/v1/keys/{keyId}/destinations",
            body).ConfigureAwait(false);
    }
}
