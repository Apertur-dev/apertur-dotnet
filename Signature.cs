using System.Security.Cryptography;
using System.Text;

namespace Apertur.Sdk;

/// <summary>
/// Static methods for verifying Apertur webhook signatures.
/// </summary>
public static class Signature
{
    /// <summary>
    /// Verifies an image delivery webhook signature.
    /// The expected header is <c>X-Apertur-Signature: sha256=&lt;hex&gt;</c>.
    /// Calculation: <c>HMAC-SHA256(body, secret)</c>.
    /// </summary>
    /// <param name="body">The raw request body.</param>
    /// <param name="signature">The signature from the <c>X-Apertur-Signature</c> header (with or without <c>sha256=</c> prefix).</param>
    /// <param name="secret">The webhook signing secret.</param>
    /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
    public static bool VerifyWebhook(string body, string signature, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var bodyBytes = Encoding.UTF8.GetBytes(body);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(bodyBytes);
        var expectedHex = Convert.ToHexString(hash).ToLowerInvariant();

        var sig = signature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? signature.Substring(7)
            : signature;

        return TimingSafeEqual(
            Encoding.UTF8.GetBytes(expectedHex),
            Encoding.UTF8.GetBytes(sig.ToLowerInvariant()));
    }

    /// <summary>
    /// Verifies an event webhook signature using the HMAC-SHA256 method.
    /// Headers: <c>X-Apertur-Signature: sha256=&lt;hex&gt;</c>, <c>X-Apertur-Timestamp: &lt;unix seconds&gt;</c>.
    /// Calculation: <c>HMAC-SHA256("{timestamp}.{body}", secret)</c>.
    /// </summary>
    /// <param name="body">The raw request body.</param>
    /// <param name="timestamp">The timestamp from the <c>X-Apertur-Timestamp</c> header.</param>
    /// <param name="signature">The signature from the <c>X-Apertur-Signature</c> header.</param>
    /// <param name="secret">The webhook signing secret.</param>
    /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
    public static bool VerifyEvent(string body, string timestamp, string signature, string secret)
    {
        var signatureBase = $"{timestamp}.{body}";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(signatureBase);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        var expectedHex = Convert.ToHexString(hash).ToLowerInvariant();

        var sig = signature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? signature.Substring(7)
            : signature;

        return TimingSafeEqual(
            Encoding.UTF8.GetBytes(expectedHex),
            Encoding.UTF8.GetBytes(sig.ToLowerInvariant()));
    }

    /// <summary>
    /// Verifies an event webhook signature using the Svix method.
    /// Headers: <c>svix-id</c>, <c>svix-timestamp</c>, <c>svix-signature: v1,&lt;base64&gt;</c>.
    /// Calculation: <c>HMAC-SHA256("{svixId}.{timestamp}.{body}", hexDecode(secret))</c>.
    /// </summary>
    /// <param name="body">The raw request body.</param>
    /// <param name="svixId">The value of the <c>svix-id</c> header.</param>
    /// <param name="timestamp">The value of the <c>svix-timestamp</c> header.</param>
    /// <param name="signature">The value of the <c>svix-signature</c> header (with or without <c>v1,</c> prefix).</param>
    /// <param name="secret">The webhook signing secret (hex-encoded).</param>
    /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
    public static bool VerifySvix(string body, string svixId, string timestamp, string signature, string secret)
    {
        var signatureBase = $"{svixId}.{timestamp}.{body}";
        var keyBytes = Convert.FromHexString(secret);
        var dataBytes = Encoding.UTF8.GetBytes(signatureBase);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        var expectedBase64 = Convert.ToBase64String(hash);

        var sig = signature.StartsWith("v1,", StringComparison.Ordinal)
            ? signature.Substring(3)
            : signature;

        var expectedBytes = Convert.FromBase64String(expectedBase64);
        byte[] sigBytes;
        try
        {
            sigBytes = Convert.FromBase64String(sig);
        }
        catch (FormatException)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedBytes, sigBytes);
    }

    private static bool TimingSafeEqual(byte[] a, byte[] b)
    {
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
