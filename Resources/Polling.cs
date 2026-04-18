using System.Text.Json;

namespace Apertur.Sdk.Resources;

/// <summary>
/// Long-polling resource for retrieving uploaded images in real time.
/// Requires sessions created with <c>long_polling: true</c>.
/// </summary>
public class Polling
{
    private readonly AperturHttpClient _http;

    /// <summary>
    /// Initializes a new <see cref="Polling"/> resource.
    /// </summary>
    /// <param name="http">The HTTP client to use for requests.</param>
    public Polling(AperturHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Lists images available for polling in a session.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <returns>A JSON element containing an <c>images</c> array.</returns>
    public async Task<JsonElement> ListAsync(string uuid)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Get,
            $"/api/v1/upload-sessions/{uuid}/poll").ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads an image by its ID from a polling-enabled session.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="imageId">The image ID.</param>
    /// <returns>The raw image bytes.</returns>
    public async Task<byte[]> DownloadAsync(string uuid, string imageId)
    {
        return await _http.RequestRawAsync(
            HttpMethod.Get,
            $"/api/v1/upload-sessions/{uuid}/images/{imageId}").ConfigureAwait(false);
    }

    /// <summary>
    /// Acknowledges that an image has been received and processed, removing it from the polling queue.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="imageId">The image ID.</param>
    /// <returns>A JSON element containing a <c>status</c> field.</returns>
    public async Task<JsonElement> AckAsync(string uuid, string imageId)
    {
        return await _http.RequestAsync<JsonElement>(
            HttpMethod.Post,
            $"/api/v1/upload-sessions/{uuid}/images/{imageId}/ack").ConfigureAwait(false);
    }

    /// <summary>
    /// Continuously polls for new images and invokes the handler for each one.
    /// The handler receives the image metadata and its raw bytes. After the handler completes,
    /// the image is automatically acknowledged.
    /// </summary>
    /// <param name="uuid">The session UUID.</param>
    /// <param name="handler">
    /// An async callback invoked for each image. Receives the image metadata as a
    /// <see cref="JsonElement"/> and the raw image bytes.
    /// </param>
    /// <param name="interval">The polling interval in milliseconds (default: 3000).</param>
    /// <param name="cancellationToken">A cancellation token to stop the polling loop.</param>
    public async Task PollAndProcessAsync(
        string uuid,
        Func<JsonElement, byte[], Task> handler,
        int interval = 3000,
        CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await ListAsync(uuid).ConfigureAwait(false);

            if (result.TryGetProperty("images", out var images))
            {
                foreach (var image in images.EnumerateArray())
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    var imageId = image.GetProperty("id").GetString()!;
                    var data = await DownloadAsync(uuid, imageId).ConfigureAwait(false);
                    await handler(image, data).ConfigureAwait(false);
                    await AckAsync(uuid, imageId).ConfigureAwait(false);
                }
            }

            if (cancellationToken.IsCancellationRequested) return;

            try
            {
                await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
