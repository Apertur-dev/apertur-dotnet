using System.Security.Cryptography;

namespace Apertur.Sdk;

/// <summary>
/// Result of encrypting image data with <see cref="Crypto.EncryptImage"/>.
/// All binary fields are base64-encoded strings.
/// </summary>
public class EncryptedPayload
{
    /// <summary>
    /// The AES-256 key encrypted (wrapped) with the RSA public key, base64-encoded.
    /// </summary>
    public string EncryptedKey { get; set; } = string.Empty;

    /// <summary>
    /// The AES-GCM initialization vector, base64-encoded.
    /// </summary>
    public string Iv { get; set; } = string.Empty;

    /// <summary>
    /// The AES-256-GCM encrypted image data with the authentication tag appended, base64-encoded.
    /// </summary>
    public string EncryptedData { get; set; } = string.Empty;

    /// <summary>
    /// The encryption algorithm identifier.
    /// </summary>
    public string Algorithm { get; set; } = "RSA-OAEP+AES-256-GCM";
}

/// <summary>
/// Provides client-side image encryption using AES-256-GCM with RSA-OAEP key wrapping.
/// </summary>
public static class Crypto
{
    /// <summary>
    /// Encrypts image data using a random AES-256-GCM key, then wraps that key with the
    /// provided RSA public key using RSA-OAEP (SHA-256).
    /// </summary>
    /// <param name="imageData">The raw image bytes to encrypt.</param>
    /// <param name="publicKeyPem">The RSA public key in PEM format.</param>
    /// <returns>An <see cref="EncryptedPayload"/> containing all base64-encoded fields.</returns>
    public static EncryptedPayload EncryptImage(byte[] imageData, string publicKeyPem)
    {
        // Generate random AES-256 key (32 bytes) and GCM IV (12 bytes)
        var aesKey = RandomNumberGenerator.GetBytes(32);
        var iv = RandomNumberGenerator.GetBytes(12);

        // Encrypt with AES-256-GCM
        var ciphertext = new byte[imageData.Length];
        var tag = new byte[16]; // 128-bit auth tag

        using (var aes = new AesGcm(aesKey))
        {
            aes.Encrypt(iv, imageData, ciphertext, tag);
        }

        // Combine ciphertext + auth tag (matching Node.js SDK behavior)
        var encryptedWithTag = new byte[ciphertext.Length + tag.Length];
        Buffer.BlockCopy(ciphertext, 0, encryptedWithTag, 0, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, encryptedWithTag, ciphertext.Length, tag.Length);

        // Wrap AES key with RSA-OAEP SHA-256
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        var wrappedKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);

        return new EncryptedPayload
        {
            EncryptedKey = Convert.ToBase64String(wrappedKey),
            Iv = Convert.ToBase64String(iv),
            EncryptedData = Convert.ToBase64String(encryptedWithTag),
            Algorithm = "RSA-OAEP+AES-256-GCM"
        };
    }
}
