namespace Aero.Core.Secrets;

/// <summary>
/// Represents a class for ShamirExtensions.
/// </summary>
public static class ShamirExtensions
{
        /// <summary>
    /// Deconstruct method.
    /// </summary>
public static string Deconstruct(this ISecretManager manager, byte[] secret)
    {
        var result = Encoding.UTF8.GetString(secret);
        return result;
    }

        /// <summary>
    /// Deconstruct method.
    /// </summary>
public static string Deconstruct(this IEncryptingSecretManager manager, byte[] secret)
    {
        var result = Encoding.UTF8.GetString(secret);
        return result;
    }
}