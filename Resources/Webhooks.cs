using System.Text.Json;

namespace Apertur.Sdk.Resources;

/// <summary>
/// Manages event webhooks for a project. Subscribe to project events
/// (uploads, deliveries, billing changes, etc.) and manage delivery history.
/// </summary>
public class Webhooks
{
    private readonly AperturHttpClient _http;

    /// <summary>
    /// Initializes a new <see cref="Webhooks"/> resource.
    /// </summary>
    /// <param name="http">The HTTP client to use for requests.</param>
    public Webhooks(AperturHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Lists all event webhooks for a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>An array of webhook configurations.</returns>
    public async Task<JsonElement> ListAsync(string projectId)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/projects/{projectId}/webhooks").ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new event webhook in a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="config">The webhook configuration including URL and topics.</param>
    /// <returns>The created webhook.</returns>
    public async Task<JsonElement> CreateAsync(string projectId, Dictionary<string, object> config)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Post,
            $"/api/v1/projects/{projectId}/webhooks",
            config).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an existing event webhook.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="webhookId">The webhook ID.</param>
    /// <param name="config">The fields to update.</param>
    /// <returns>The updated webhook.</returns>
    public async Task<JsonElement> UpdateAsync(string projectId, string webhookId, Dictionary<string, object> config)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Patch,
            $"/api/v1/projects/{projectId}/webhooks/{webhookId}",
            config).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes an event webhook.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="webhookId">The webhook ID.</param>
    public async Task DeleteAsync(string projectId, string webhookId)
    {
        await _http.RequestAsync<JsonElement>(
            HttpMethod.Delete,
            $"/api/v1/projects/{projectId}/webhooks/{webhookId}").ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a test event to a webhook to verify it is correctly configured.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="webhookId">The webhook ID.</param>
    /// <returns>A JSON element containing a message.</returns>
    public async Task<JsonElement> TestAsync(string projectId, string webhookId)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Post,
            $"/api/v1/projects/{projectId}/webhooks/{webhookId}/test").ConfigureAwait(false);
    }

    /// <summary>
    /// Lists delivery attempts for a webhook.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="webhookId">The webhook ID.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="limit">The number of items per page.</param>
    /// <returns>A paginated list of webhook deliveries.</returns>
    public async Task<JsonElement> DeliveriesAsync(
        string projectId,
        string webhookId,
        int? page = null,
        int? limit = null)
    {
        var query = QueryHelper.Build(
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/projects/{projectId}/webhooks/{webhookId}/deliveries{query}").ConfigureAwait(false);
    }

    /// <summary>
    /// Retries a failed webhook delivery.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="webhookId">The webhook ID.</param>
    /// <param name="deliveryId">The delivery ID to retry.</param>
    /// <returns>A JSON element containing a message.</returns>
    public async Task<JsonElement> RetryDeliveryAsync(string projectId, string webhookId, string deliveryId)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Post,
            $"/api/v1/projects/{projectId}/webhooks/{webhookId}/deliveries/{deliveryId}/retry").ConfigureAwait(false);
    }
}
