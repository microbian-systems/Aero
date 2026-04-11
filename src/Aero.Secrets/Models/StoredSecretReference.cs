namespace Aero.Secrets.Models;

public sealed record StoredSecretReference(
    SecretProviderType ProviderType,
    string Name,
    string? Value = null,
    string? Metadata = null);
