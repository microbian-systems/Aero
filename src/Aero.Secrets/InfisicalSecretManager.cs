using System.Text.Json;
using Aero.Secrets.Models;
using Infisical.Sdk;
using Infisical.Sdk.Model;

namespace Aero.Secrets;

public sealed class InfisicalSecretManager : SecretManagerBase
{
    private readonly InfisicalClient _client;
    private readonly InfisicalSecretManagerOptions _options;

    public InfisicalSecretManager(InfisicalSecretManagerOptions? options = null)
    {
        _options = options ?? new InfisicalSecretManagerOptions();
        _client = new InfisicalClient(new InfisicalSdkSettingsBuilder()
            .WithHostUri(_options.HostUri.ToString())
            .Build());
    }

    protected override StoredSecretReference StoreCore(string secret, string name, SecretProviderType providerType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.ProjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.EnvironmentSlug);
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.MachineId);
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.ClientSecret);

        _client.Auth().UniversalAuth().LoginAsync(_options.MachineId, _options.ClientSecret).GetAwaiter().GetResult();

        var created = _client.Secrets().CreateAsync(new CreateSecretOptions
        {
            ProjectId = _options.ProjectId,
            EnvironmentSlug = _options.EnvironmentSlug,
            SecretPath = _options.SecretPath,
            SecretName = name,
            SecretValue = secret,
            Metadata = BuildMetadata(name, providerType)
        }).GetAwaiter().GetResult();

        return new StoredSecretReference(providerType, name, null, BuildReferenceMetadata(name, providerType, created.Id));
    }

    protected override string ReadCore(StoredSecretReference secretReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.ProjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.EnvironmentSlug);
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.MachineId);
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.ClientSecret);

        _client.Auth().UniversalAuth().LoginAsync(_options.MachineId, _options.ClientSecret).GetAwaiter().GetResult();

        var reference = ParseMetadata(secretReference.Metadata);
        var secret = _client.Secrets().GetAsync(new GetSecretOptions
        {
            ProjectId = reference.ProjectId ?? _options.ProjectId,
            EnvironmentSlug = reference.EnvironmentSlug ?? _options.EnvironmentSlug,
            SecretPath = reference.SecretPath ?? _options.SecretPath,
            SecretName = reference.SecretName ?? secretReference.Name,
            ViewSecretValue = true,
            ExpandSecretReferences = false
        }).GetAwaiter().GetResult();

        return secret.SecretValue ?? throw new InvalidOperationException("Infisical secret did not contain a value.");
    }

    private static SecretMetadata[] BuildMetadata(string name, SecretProviderType providerType, string? secretId = null)
        =>
        [
            new SecretMetadata { Key = "providerType", Value = providerType.ToString() },
            new SecretMetadata { Key = "secretName", Value = name },
            new SecretMetadata { Key = "secretId", Value = secretId ?? string.Empty }
        ];

    private string BuildReferenceMetadata(string name, SecretProviderType providerType, string? secretId = null)
        => JsonSerializer.Serialize(new InfisicalSecretReferenceMetadata(
            ProviderType: providerType,
            SecretName: name,
            ProjectId: _options.ProjectId,
            EnvironmentSlug: _options.EnvironmentSlug,
            SecretPath: _options.SecretPath,
            SecretId: secretId,
            HostUri: _options.HostUri.ToString()));

    private static InfisicalSecretReferenceMetadata ParseMetadata(string? metadata)
        => string.IsNullOrWhiteSpace(metadata)
            ? new InfisicalSecretReferenceMetadata(SecretProviderType.Infisical, null, null, null, null, null, null)
            : JsonSerializer.Deserialize<InfisicalSecretReferenceMetadata>(metadata) ?? new InfisicalSecretReferenceMetadata(SecretProviderType.Infisical, null, null, null, null, null, null);

    private sealed record InfisicalSecretReferenceMetadata(
        SecretProviderType ProviderType,
        string? SecretName,
        string? ProjectId,
        string? EnvironmentSlug,
        string? SecretPath,
        string? SecretId,
        string? HostUri);
}
