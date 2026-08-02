using Aero.Secrets.Models;

namespace Aero.Secrets;

/// <summary>
/// Defines an interface for ISecretManager.
/// </summary>
public interface ISecretManager
{
        /// <summary>
    /// Store method.
    /// </summary>
StoredSecretReference Store(string secret, string name, SecretProviderType providerType = SecretProviderType.Local);

        /// <summary>
    /// Read method.
    /// </summary>
string Read(StoredSecretReference secretReference);
}
