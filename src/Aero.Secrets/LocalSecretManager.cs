using Aero.Secrets.Models;

namespace Aero.Secrets;

/// <summary>
/// Represents a class for LocalSecretManager.
/// </summary>
public sealed class LocalSecretManager : SecretManagerBase
{
        /// <summary>
    /// StoreCore method.
    /// </summary>
protected override StoredSecretReference StoreCore(string secret, string name, SecretProviderType providerType)
        => new(providerType, name, secret);

        /// <summary>
    /// ReadCore method.
    /// </summary>
protected override string ReadCore(StoredSecretReference secretReference)
        => secretReference.Value ?? throw new InvalidOperationException("Stored secret reference does not contain a value.");
}
