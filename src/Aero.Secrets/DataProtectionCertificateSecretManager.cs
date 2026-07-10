using System.Security.Cryptography.X509Certificates;
using Aero.Secrets.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Aero.Secrets;

/// <summary>
/// Represents a class for DataProtectionCertificateSecretManager.
/// </summary>
public sealed class DataProtectionCertificateSecretManager : SecretManagerBase
{
    private const string EncryptedPrefix = "enc:";
    private readonly IDataProtector _protector;

        /// <summary>
    /// Initializes a new instance of the <see cref="DataProtectionCertificateSecretManager"/> class.
    /// </summary>
public DataProtectionCertificateSecretManager(
        X509Certificate2 certificate,
        string applicationName = "AeroCMS-Vault",
        string? keyRingPath = null,
        string protectorPurpose = "Aero.Secrets.V1")
    {
        ArgumentNullException.ThrowIfNull(certificate);

        var services = new ServiceCollection();
        var builder = services.AddDataProtection()
            .SetApplicationName(applicationName)
            .ProtectKeysWithCertificate(certificate);

        if (!string.IsNullOrWhiteSpace(keyRingPath))
        {
            Directory.CreateDirectory(keyRingPath);
            builder.PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));
        }

        var provider = services.BuildServiceProvider();
        _protector = provider.GetDataProtector(protectorPurpose);
    }

        /// <summary>
    /// StoreCore method.
    /// </summary>
protected override StoredSecretReference StoreCore(string secret, string name, SecretProviderType providerType)
        => new(providerType, name, $"{EncryptedPrefix}{_protector.Protect(secret)}");

        /// <summary>
    /// ReadCore method.
    /// </summary>
protected override string ReadCore(StoredSecretReference secretReference)
    {
        if (secretReference.Value is null)
        {
            throw new InvalidOperationException("Stored secret reference does not contain a value.");
        }

        var value = secretReference.Value;

        if (value.StartsWith(EncryptedPrefix, StringComparison.Ordinal))
        {
            return _protector.Unprotect(value[EncryptedPrefix.Length..]);
        }

        try
        {
            return _protector.Unprotect(value);
        }
        catch
        {
            return value;
        }
    }
}
