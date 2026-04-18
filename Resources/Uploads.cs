using System.Text.Json;

namespace Apertur.Sdk.Resources;

/// <summary>
/// Queries uploaded images across all sessions.
/// </summary>
public class Uploads
{
    private readonly AperturHttpClient _http;

    /// <summary>
    /// Initializes a new <see cref="Uploads"/> resource.
    /// </summary>
    /// <param name="http">The HTTP client to use for requests.</param>
    public Uploads(AperturHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Lists uploaded images with pagination.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated list of uploads.</returns>
    public async Task<JsonElement> ListAsync(int? page = null, int? pageSize = null)
    {
        var query = QueryHelper.Build(
            ("page", page?.ToString()),
            ("pageSize", pageSize?.ToString()));

        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/uploads{query}").ConfigureAwait(false);
    }

    /// <summary>
    /// Lists recent uploads.
    /// </summary>
    /// <param name="limit">Maximum number of uploads to return.</param>
    /// <returns>An array of recent uploads.</returns>
    public async Task<JsonElement> RecentAsync(int? limit = null)
    {
        var query = QueryHelper.Build(("limit", limit?.ToString()));

        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/uploads/recent{query}").ConfigureAwait(false);
    }
}

/// <summary>
/// Internal helper for building URL query strings.
/// </summary>
internal static class QueryHelper
{
    /// <summary>
    /// Builds a URL query string from a set of key-value pairs, omitting null values.
    /// </summary>
    /// <param name="parameters">The parameters to include.</param>
    /// <returns>A query string starting with <c>?</c>, or an empty string if no parameters have values.</returns>
    public static string Build(params (string Key, string? Value)[] parameters)
    {
        var pairs = parameters
            .Where(p => p.Value is not null)
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}");

        var qs = string.Join("&", pairs);
        return string.IsNullOrEmpty(qs) ? string.Empty : $"?{qs}";
    }
}
