using Aero.Core.Encryption;

namespace Aero.Core.Secrets;

/// <summary>
/// Represents a class for EncryptingSecretManager.
/// </summary>
public sealed class EncryptingSecretManager(
    ISecretManager manager,
    IEncryptor encryptor,
    ILogger<IEncryptingSecretManager> log) : IEncryptingSecretManager
{
        /// <summary>
    /// CreateFragments method.
    /// </summary>
public string[]? CreateFragments(string? secret, ushort numFragments = 3)
    {
        ArgumentException.ThrowIfNullOrEmpty(secret);
        var frags = manager.CreateFragments(Encoding.UTF8.GetBytes(secret), numFragments);
        var encrypted = frags.Select(encryptor.EncryptString)
            .ToArray();

        return encrypted;
    }

        /// <summary>
    /// CreateFragments method.
    /// </summary>
public string[]? CreateFragments(byte[]? secret, ushort nbFragments = 3)
    {
        log.LogInformation("encrypting fragments");

        var shards = manager.CreateFragments(secret, nbFragments);

        if (shards is null)
        {
            log.LogError("Failed to create fragments.");
            return [];
        }

        if (shards.Length == 0)
        {
            log.LogWarning("no secret fragments were created.");
            return [];
        }

        var encrypted = shards.Select(encryptor.EncryptString);

        return encrypted.ToArray();
    }

        /// <summary>
    /// ComputeFragments method.
    /// </summary>
public byte[] ComputeFragments(string[] fragments)
    {
        log.LogInformation("decrypting fragments");
        var decryptedFraments = fragments.Select(encryptor.DecryptString);
        var frags = manager.ComputeFragments(decryptedFraments.ToArray());

        return frags;
    }
}