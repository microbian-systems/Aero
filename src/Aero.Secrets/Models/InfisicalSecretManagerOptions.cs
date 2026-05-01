namespace Aero.Secrets.Models;

public sealed record InfisicalSecretManagerOptions
{
    public Uri HostUri { get; init; } = new("http://localhost:8080");

    public string ProjectId { get; init; } = string.Empty;

    public string EnvironmentSlug { get; init; } = string.Empty;

    public string SecretPath { get; init; } = "/";

    public string MachineId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;
}
