using System.Text.Json;

namespace Apertur.Sdk.Resources;

/// <summary>
/// Manages delivery destinations (webhook, S3, Google Drive, etc.) for a project.
/// </summary>
public class Destinations
{
    private readonly AperturHttpClient _http;

    /// <summary>
    /// Initializes a new <see cref="Destinations"/> resource.
    /// </summary>
    /// <param name="http">The HTTP client to use for requests.</param>
    public Destinations(AperturHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Lists all destinations for a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>An array of destinations.</returns>
    public async Task<JsonElement> ListAsync(string projectId)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/projects/{projectId}/destinations").ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new destination in a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="config">The destination configuration including type, name and config.</param>
    /// <returns>The created destination.</returns>
    public async Task<JsonElement> CreateAsync(string projectId, Dictionary<string, object> config)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Post,
            $"/api/v1/projects/{projectId}/destinations",
            config).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an existing destination.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="destinationId">The destination ID.</param>
    /// <param name="config">The fields to update.</param>
    /// <returns>The updated destination.</returns>
    public async Task<JsonElement> UpdateAsync(string projectId, string destinationId, Dictionary<string, object> config)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Patch,
            $"/api/v1/projects/{projectId}/destinations/{destinationId}",
            config).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a destination.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="destinationId">The destination ID.</param>
    public async Task DeleteAsync(string projectId, string destinationId)
    {
        await _http.RequestAsync<JsonElement>(
            HttpMethod.Delete,
            $"/api/v1/projects/{projectId}/destinations/{destinationId}").ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a test payload to a destination to verify it is correctly configured.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="destinationId">The destination ID.</param>
    /// <returns>The test result containing success status and optional error details.</returns>
    public async Task<JsonElement> TestAsync(string projectId, string destinationId)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Post,
            $"/api/v1/projects/{projectId}/destinations/{destinationId}/test").ConfigureAwait(false);
    }
}
