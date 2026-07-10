namespace Aero.Core.Secrets;

/// <summary>
/// Defines an interface for IEncryptingSecretManager.
/// </summary>
public interface IEncryptingSecretManager
{
        /// <summary>
    /// CreateFragments method.
    /// </summary>
string[]? CreateFragments(string? secret, ushort numFragments = 3);
        /// <summary>
    /// CreateFragments method.
    /// </summary>
string[]? CreateFragments(byte[]? secret, ushort nbFragments);
        /// <summary>
    /// ComputeFragments method.
    /// </summary>
byte[]? ComputeFragments(string[] fragments);
}