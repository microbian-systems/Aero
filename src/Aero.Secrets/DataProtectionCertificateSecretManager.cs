using System.Security.Cryptography.X509Certificates;
using Aero.Secrets.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Aero.Secrets;

public sealed class DataProtectionCertificateSecretManager : SecretManagerBase
{
    private const string EncryptedPrefix = "enc:";
    private readonly IDataProtector _protector;

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

    protected override StoredSecretReference StoreCore(string secret, string name, SecretProviderType providerType)
        => new(providerType, name, $"{EncryptedPrefix}{_protector.Protect(secret)}");

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
