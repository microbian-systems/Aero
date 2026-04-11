using System.Security.Cryptography.X509Certificates;
using Aero.Secrets.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Aero.Secrets;

public sealed class DataProtectionCertificateSecretManager : SecretManagerBase
{
    private readonly IDataProtector _protector;

    public DataProtectionCertificateSecretManager(X509Certificate2 certificate, string applicationName = "AeroCMS-Vault")
    {
        ArgumentNullException.ThrowIfNull(certificate);

        var services = new ServiceCollection();
        services.AddDataProtection()
            .SetApplicationName(applicationName)
            .ProtectKeysWithCertificate(certificate);

        var provider = services.BuildServiceProvider();
        _protector = provider.GetDataProtector("Aero.Secrets.V1");
    }

    protected override StoredSecretReference StoreCore(string secret, string name, SecretProviderType providerType)
        => new(providerType, name, _protector.Protect(secret));

    protected override string ReadCore(StoredSecretReference secretReference)
    {
        if (secretReference.Value is null)
        {
            throw new InvalidOperationException("Stored secret reference does not contain a value.");
        }

        return _protector.Unprotect(secretReference.Value);
    }
}
