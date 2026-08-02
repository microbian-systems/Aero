namespace Aero.Core.Secrets;

/// <summary>
/// Defines an interface for ISecretManager.
/// </summary>
public interface ISecretManager
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