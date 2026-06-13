# Apertur.Sdk

Official .NET SDK for the [Apertur](https://apertur.ca) API. Collect photos from any mobile device via QR codes -- no app required.

## Installation

```bash
dotnet add package Apertur.Sdk
```

## Quick Start

```csharp
using Apertur.Sdk;

var client = new AperturClient("aptr_xxxx");

// Create a session and upload an image
var session = await client.Sessions.CreateAsync();
var uuid = session.GetProperty("uuid").GetString()!;
var result = await client.Upload.ImageAsync(uuid, "./photo.jpg");
```

## Authentication

Two auth methods: API key (for server-to-server) and OAuth token (for third-party integrations).

```csharp
// API Key
var client = new AperturClient("aptr_your_key_here");

// OAuth Token
var client = new AperturClient(new AperturConfig { OAuthToken = "aptr_oauth_your_token" });

// Custom base URL (for self-hosted or staging)
var client = new AperturClient(new AperturConfig
{
    ApiKey = "aptr_xxx",
    BaseUrl = "https://api.aptr.ca"
});
```

The environment is auto-detected from the key prefix: `aptr_test_` keys target the sandbox (`https://sandbox.api.aptr.ca`), all other keys target production (`https://api.aptr.ca`).

See [Authentication documentation](https://docs.apertur.ca/authentication)

## Sessions

Create upload sessions, check status, and manage password-protected sessions.

```csharp
// Create a session
var session = await client.Sessions.CreateAsync(new Dictionary<string, object>
{
    ["tags"] = new[] { "event-photos" },
    ["expires_in_hours"] = 48,
    ["max_images"] = 50,
    ["password"] = "secret123",
    ["long_polling"] = true,
});

var uuid = session.GetProperty("uuid").GetString()!;

// Get session info
var info = await client.Sessions.GetAsync(uuid);

// Generate QR code
byte[] qr = await client.Sessions.QrAsync(uuid, new Dictionary<string, string>
{
    ["format"] = "png",
    ["size"] = "400"
});
File.WriteAllBytes("qr.png", qr);

// Verify password
var check = await client.Sessions.VerifyPasswordAsync(uuid, "secret123");

// Check delivery status — snapshot
var status = await client.Sessions.DeliveryStatusAsync(uuid);
string overall  = status.GetProperty("status").GetString()!; // pending|active|completed|expired
var files       = status.GetProperty("files");
string changed  = status.GetProperty("lastChanged").GetString()!;

// Long-poll for the next change (server holds the response up to 5 minutes).
// Allow at least 6 minutes on the client so the server releases first.
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(6));
var next = await client.Sessions.DeliveryStatusAsync(uuid, pollFrom: changed, ct: cts.Token);
```

See [Sessions documentation](https://docs.apertur.ca/upload-sessions)

## Uploading Images

Upload images from a file path, byte array, or stream. Supports optional client-side encryption.

```csharp
// Upload from file path
await client.Upload.ImageAsync(uuid, "./photo.jpg",
    mimeType: "image/jpeg", source: "gallery");

// Upload from byte array
byte[] bytes = File.ReadAllBytes("./photo.png");
await client.Upload.ImageAsync(uuid, bytes,
    filename: "vacation.png", mimeType: "image/png");

// Upload from stream
using var stream = File.OpenRead("./photo.jpg");
await client.Upload.ImageAsync(uuid, stream, "photo.jpg", "image/jpeg");

// Upload with client-side encryption
var serverKey = await client.Encryption.GetServerKeyAsync();
var publicKey = serverKey.GetProperty("public_key").GetString()!;
await client.Upload.ImageEncryptedAsync(uuid, bytes, publicKey);
```

See [Upload documentation](https://docs.apertur.ca/upload-sessions)

## Long Polling

Retrieve uploaded images via polling instead of webhooks. Requires session created with `long_polling: true`.

```csharp
// Manual poll cycle
var poll = await client.Polling.ListAsync(uuid);
foreach (var image in poll.GetProperty("images").EnumerateArray())
{
    var imageId = image.GetProperty("id").GetString()!;
    var data = await client.Polling.DownloadAsync(uuid, imageId);
    File.WriteAllBytes($"./downloads/{image.GetProperty("filename").GetString()}", data);
    await client.Polling.AckAsync(uuid, imageId);
}

// Automatic poll + process loop
using var cts = new CancellationTokenSource();

_ = client.Polling.PollAndProcessAsync(uuid, async (image, data) =>
{
    var filename = image.GetProperty("filename").GetString()!;
    Console.WriteLine($"Received: {filename}");
    await File.WriteAllBytesAsync($"./output/{filename}", data);
}, interval: 3000, cancellationToken: cts.Token);

// Stop polling
cts.Cancel();
```

See [Long Polling documentation](https://docs.apertur.ca/long-polling)

## Receiving Webhooks

Verify webhook signatures to ensure payloads are authentic.

```csharp
using Apertur.Sdk;

// Image delivery webhook
bool isValid = Signature.VerifyWebhook(body, signatureHeader, webhookSecret);

// Event webhook (HMAC SHA256)
bool isValid = Signature.VerifyEvent(body, timestampHeader, signatureHeader, secret);

// Event webhook (Svix)
bool isValid = Signature.VerifySvix(body, svixId, svixTimestamp, svixSignature, secret);
```

See [Webhook documentation](https://docs.apertur.ca/webhooks)

## Destinations

Manage delivery destinations (webhook, S3, Google Drive, etc.).

```csharp
var destinations = await client.Destinations.ListAsync(projectId);

var webhook = await client.Destinations.CreateAsync(projectId, new Dictionary<string, object>
{
    ["type"] = "webhook",
    ["name"] = "My Backend",
    ["config"] = new Dictionary<string, object>
    {
        ["url"] = "https://api.example.com/photos",
        ["format"] = "json_base64"
    }
});

var destId = webhook.GetProperty("id").GetString()!;
await client.Destinations.TestAsync(projectId, destId);
await client.Destinations.UpdateAsync(projectId, destId, new Dictionary<string, object>
{
    ["is_active"] = false
});
await client.Destinations.DeleteAsync(projectId, destId);
```

See [Destinations documentation](https://docs.apertur.ca/destinations)

## API Keys

Manage API keys and their default destinations.

```csharp
var keys = await client.Keys.ListAsync(projectId);

var created = await client.Keys.CreateAsync(projectId, new Dictionary<string, object>
{
    ["label"] = "Production",
    ["max_images"] = 100,
});
Console.WriteLine($"Save this key: {created.GetProperty("plain_text_key").GetString()}");

// Set default destinations for a key
var keyId = created.GetProperty("key").GetProperty("id").GetString()!;
await client.Keys.SetDestinationsAsync(keyId, new[] { destId1, destId2 }, longPollingEnabled: true);
```

See [API Keys documentation](https://docs.apertur.ca/api-keys)

## Event Webhooks

Subscribe to project events (uploads, deliveries, billing changes, etc.).

```csharp
var webhook = await client.Webhooks.CreateAsync(projectId, new Dictionary<string, object>
{
    ["url"] = "https://api.example.com/events",
    ["topics"] = new[] { "project.upload.*", "project.billing.plan_changed" }
});

var webhookId = webhook.GetProperty("id").GetString()!;

// List deliveries
var deliveries = await client.Webhooks.DeliveriesAsync(projectId, webhookId);

// Retry a failed delivery
var firstDeliveryId = deliveries.GetProperty("deliveries")[0].GetProperty("id").GetString()!;
await client.Webhooks.RetryDeliveryAsync(projectId, webhookId, firstDeliveryId);
```

See [Event Webhooks documentation](https://docs.apertur.ca/event-webhooks)

## Error Handling

All SDK errors extend `AperturException` with typed subclasses for common HTTP failure cases.

```csharp
using Apertur.Sdk.Exceptions;

try
{
    await client.Sessions.CreateAsync();
}
catch (RateLimitException ex)
{
    Console.WriteLine($"Rate limited. Retry after: {ex.RetryAfter}s");
}
catch (AuthenticationException)
{
    Console.WriteLine("Invalid API key");
}
catch (NotFoundException)
{
    Console.WriteLine("Resource not found");
}
catch (AperturException ex)
{
    Console.WriteLine($"API error {ex.StatusCode}: {ex.Message} (code: {ex.Code})");
}
```

## API Reference

For complete API documentation, visit [docs.apertur.ca](https://docs.apertur.ca).

## License

MIT
