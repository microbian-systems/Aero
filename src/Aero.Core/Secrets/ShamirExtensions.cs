namespace Aero.Core.Secrets;

public static class ShamirExtensions
{
    public static string Deconstruct(this ISecretManager manager, byte[] secret)
    {
        var result = Encoding.UTF8.GetString(secret);
        return result;
    }

    public static string Deconstruct(this IEncryptingSecretManager manager, byte[] secret)
    {
        var result = Encoding.UTF8.GetString(secret);
        return result;
    }
}