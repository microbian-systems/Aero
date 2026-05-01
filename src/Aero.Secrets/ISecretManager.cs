using Aero.Secrets.Models;

namespace Aero.Secrets;

public interface ISecretManager
{
    StoredSecretReference Store(string secret, string name, SecretProviderType providerType = SecretProviderType.Local);

    string Read(StoredSecretReference secretReference);
}
