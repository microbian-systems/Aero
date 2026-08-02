namespace Aero.Secrets.Models;

/// <summary>
/// Represents a record for StoredSecretReference.
/// </summary>
public sealed record StoredSecretReference(
    SecretProviderType ProviderType,
    string Name,
    string? Value = null,
    string? Metadata = null);
