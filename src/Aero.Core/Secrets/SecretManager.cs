namespace Aero.Core.Secrets;

/// <summary>
/// Represents a class for SecretManager.
/// </summary>
public abstract class SecretManager : ISecretManager
{
        /// <summary>
    /// CreateFragments method.
    /// </summary>
public virtual string[]? CreateFragments(string? secret, ushort numFragments = 3)
    {
        ArgumentException.ThrowIfNullOrEmpty(secret);
        return CreateFragments(Encoding.UTF8.GetBytes(secret), numFragments);
    }

        /// <summary>
    /// CreateFragments method.
    /// </summary>
public abstract string[]? CreateFragments(byte[]? secret, ushort numFragments = 3);
        /// <summary>
    /// ComputeFragments method.
    /// </summary>
public abstract byte[]? ComputeFragments(string[] fragments);
}