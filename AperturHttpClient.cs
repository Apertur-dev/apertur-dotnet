using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Apertur.Sdk.Exceptions;

namespace Apertur.Sdk;

/// <summary>
/// Low-level HTTP wrapper around <see cref="HttpClient"/> used by all resource classes.
/// Handles authentication, JSON serialization and error mapping.
/// </summary>
public sealed class AperturHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string? _bearerToken;

    /// <summary>
    /// JSON serializer options configured for snake_case property names.
    /// </summary>
    internal static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    /// <summary>
    /// Initializes a new <see cref="AperturHttpClient"/>.
    /// </summary>
    /// <param name="baseUrl">The base URL for the Apertur API.</param>
    /// <param name="bearerToken">The bearer token used for authentication.</param>
    /// <param name="httpClient">An optional <see cref="HttpClient"/> to use.</param>
    public AperturHttpClient(string baseUrl, string? bearerToken, HttpClient? httpClient = null)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _bearerToken = bearerToken;
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <summary>
    /// Sends an HTTP request and deserializes the JSON response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path (appended to the base URL).</param>
    /// <param name="body">Optional request body to serialize as JSON.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="ct">Optional cancellation token forwarded to the underlying send.</param>
    /// <returns>The deserialized response body.</returns>
    public async Task<T> RequestAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        Dictionary<string, string>? headers = null,
        CancellationToken ct = default)
    {
        using var request = BuildJsonRequest(method, path, body, headers);
        using var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response).ConfigureAwait(false);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default!;
        }

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    /// <summary>
    /// Sends an HTTP request and returns the response body as a raw byte array.
    /// Useful for binary content such as QR codes or image downloads.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path (appended to the base URL).</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <returns>The raw response bytes.</returns>
    public async Task<byte[]> RequestRawAsync(
        HttpMethod method,
        string path,
        Dictionary<string, string>? headers = null)
    {
        using var request = new HttpRequestMessage(method, _baseUrl + path);
        ApplyAuth(request);
        ApplyHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response).ConfigureAwait(false);
        }

        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a multipart/form-data request and deserializes the JSON response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path (appended to the base URL).</param>
    /// <param name="content">The multipart form content.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <returns>The deserialized response body.</returns>
    public async Task<T> RequestMultipartAsync<T>(
        HttpMethod method,
        string path,
        MultipartFormDataContent content,
        Dictionary<string, string>? headers = null)
    {
        using var request = new HttpRequestMessage(method, _baseUrl + path) { Content = content };
        ApplyAuth(request);
        ApplyHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response).ConfigureAwait(false);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default!;
        }

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    /// <summary>
    /// Sends an HTTP request with a pre-serialized JSON string body and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path (appended to the base URL).</param>
    /// <param name="jsonBody">The pre-serialized JSON body.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <returns>The deserialized response body.</returns>
    public async Task<T> RequestJsonStringAsync<T>(
        HttpMethod method,
        string path,
        string jsonBody,
        Dictionary<string, string>? headers = null)
    {
        using var request = new HttpRequestMessage(method, _baseUrl + path)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        ApplyAuth(request);
        ApplyHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response).ConfigureAwait(false);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default!;
        }

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    private HttpRequestMessage BuildJsonRequest(
        HttpMethod method,
        string path,
        object? body,
        Dictionary<string, string>? headers)
    {
        var request = new HttpRequestMessage(method, _baseUrl + path);
        ApplyAuth(request);
        ApplyHeaders(request, headers);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private void ApplyAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
        }
    }

    private static void ApplyHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers is null) return;

        foreach (var (key, value) in headers)
        {
            // Content headers must be set on request.Content, but for simplicity
            // we set them via TryAddWithoutValidation which works for most cases.
            request.Headers.TryAddWithoutValidation(key, value);
        }
    }

    private static async Task HandleErrorAsync(HttpResponseMessage response)
    {
        string message;
        string? code = null;

        try
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            message = root.TryGetProperty("message", out var msgProp)
                ? msgProp.GetString() ?? $"HTTP {(int)response.StatusCode}"
                : $"HTTP {(int)response.StatusCode}";

            if (root.TryGetProperty("code", out var codeProp))
            {
                code = codeProp.GetString();
            }
        }
        catch
        {
            message = $"HTTP {(int)response.StatusCode}";
        }

        var status = (int)response.StatusCode;

        throw status switch
        {
            401 => new AuthenticationException(message),
            404 => new NotFoundException(message),
            429 => new RateLimitException(
                message,
                response.Headers.TryGetValues("Retry-After", out var values)
                    ? int.TryParse(values.FirstOrDefault(), out var ra) ? ra : null
                    : null),
            400 => new ValidationException(message),
            _ => new AperturException(status, message, code)
        };
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

#if NET8_0_OR_GREATER
        options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
#else
        options.PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance;
#endif

        return options;
    }
}

#if !NET8_0_OR_GREATER
/// <summary>
/// Custom snake_case naming policy for .NET 6 / .NET 7 where
/// <c>JsonNamingPolicy.SnakeCaseLower</c> is not available.
/// </summary>
internal sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public static readonly SnakeCaseNamingPolicy Instance = new();

    /// <inheritdoc />
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    sb.Append('_');
                }
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
#endif
