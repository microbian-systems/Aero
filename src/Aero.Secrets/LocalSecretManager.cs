using Aero.Secrets.Models;

namespace Aero.Secrets;

public sealed class LocalSecretManager : SecretManagerBase
{
    protected override StoredSecretReference StoreCore(string secret, string name, SecretProviderType providerType)
        => new(providerType, name, secret);

    protected override string ReadCore(StoredSecretReference secretReference)
        => secretReference.Value ?? throw new InvalidOperationException("Stored secret reference does not contain a value.");
}
