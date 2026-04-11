using Aero.Secrets.Models;

namespace Aero.Secrets;

public abstract class SecretManagerBase : ISecretManager
{
    public virtual StoredSecretReference Store(string secret, string name, SecretProviderType providerType = SecretProviderType.Local)
    {
        ArgumentException.ThrowIfNullOrEmpty(secret);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return StoreCore(secret, name, providerType);
    }

    public virtual string Read(StoredSecretReference secretReference)
    {
        ArgumentNullException.ThrowIfNull(secretReference);
        return ReadCore(secretReference);
    }

    protected abstract StoredSecretReference StoreCore(string secret, string name, SecretProviderType providerType);

    protected abstract string ReadCore(StoredSecretReference secretReference);
}
