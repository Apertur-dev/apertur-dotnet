using System.Text.Json;

namespace Apertur.Sdk.Resources;

/// <summary>
/// Retrieves account-level statistics.
/// </summary>
public class Stats
{
    private readonly AperturHttpClient _http;

    /// <summary>
    /// Initializes a new <see cref="Stats"/> resource.
    /// </summary>
    /// <param name="http">The HTTP client to use for requests.</param>
    public Stats(AperturHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Gets aggregated account statistics including session counts, upload metrics
    /// and top projects.
    /// </summary>
    /// <returns>A JSON element containing the stats fields.</returns>
    public async Task<JsonElement> GetAsync()
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            "/api/v1/stats").ConfigureAwait(false);
    }
}
