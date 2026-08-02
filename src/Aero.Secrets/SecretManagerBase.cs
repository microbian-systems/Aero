using Aero.Secrets.Models;

namespace Aero.Secrets;

/// <summary>
/// Represents a class for SecretManagerBase.
/// </summary>
public abstract class SecretManagerBase : ISecretManager
{
        /// <summary>
    /// Store method.
    /// </summary>
public virtual StoredSecretReference Store(string secret, string name, SecretProviderType providerType = SecretProviderType.Local)
    {
        ArgumentException.ThrowIfNullOrEmpty(secret);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return StoreCore(secret, name, providerType);
    }

        /// <summary>
    /// Read method.
    /// </summary>
public virtual string Read(StoredSecretReference secretReference)
    {
        ArgumentNullException.ThrowIfNull(secretReference);
        return ReadCore(secretReference);
    }

        /// <summary>
    /// StoreCore method.
    /// </summary>
protected abstract StoredSecretReference StoreCore(string secret, string name, SecretProviderType providerType);

        /// <summary>
    /// ReadCore method.
    /// </summary>
protected abstract string ReadCore(StoredSecretReference secretReference);
}
