using System.Text;
using Amazon.S3;
using Amazon.Runtime;
using FluentValidation;

namespace Aero.Storage.S3;

/// <summary>Trusted server binding; clients must never select bucket, scope or endpoint.</summary>
public sealed record S3StorageOptions
{
    /// <summary>Existing private bucket; adapter never creates a bucket.</summary>
    public required string Bucket { get; init; }
    /// <summary>Nonempty scope without trailing slash, e.g. shared/public or tenants/123.</summary>
    public required string ScopePrefix { get; init; }
    /// <summary>Bounded single PUT and GET size; multipart is deliberately outside this slice.</summary>
    public long MaxObjectBytes { get; init; } = 20 * 1024 * 1024;
    /// <summary>Enable the explicit R2 request compatibility profile.</summary>
    public bool UseR2Compatibility { get; init; }
}
internal sealed class S3StorageOptionsValidator : AbstractValidator<S3StorageOptions>
{
    public S3StorageOptionsValidator()
    {
        RuleFor(x => x.Bucket).NotEmpty().Matches("^[a-z0-9][a-z0-9.-]{1,61}[a-z0-9]$");
        RuleFor(x => x.ScopePrefix).Cascade(CascadeMode.Stop).NotEmpty()
            .Must(x => Encoding.UTF8.GetByteCount(x) <= 255)
            .Must(x => x.All(c => char.IsLetterOrDigit(c) || c is '/' or '-' or '_' or '.'))
            .Must(x => x.Split('/').All(segment => segment.Length > 0 && segment is not "." and not ".."));
        RuleFor(x => x.MaxObjectBytes).InclusiveBetween(1, 256L * 1024 * 1024);
    }
}
/// <summary>Local configuration only; credentials supplied separately by the host/AeroVault.</summary>
public static class R2ClientConfiguration
{
    /// <summary>Produces explicit HTTPS path-style configuration without reading global credentials/settings.</summary>
    public static AmazonS3Config Create(Uri endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (!endpoint.IsAbsoluteUri || endpoint.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(endpoint.UserInfo) || endpoint.AbsolutePath != "/"
            || !string.IsNullOrEmpty(endpoint.Query) || !string.IsNullOrEmpty(endpoint.Fragment))
            throw new ArgumentException("R2 requires an operator-approved HTTPS origin.", nameof(endpoint));
        return new AmazonS3Config
        {
            ServiceURL = endpoint.AbsoluteUri.TrimEnd('/'), AuthenticationRegion = "auto", ForcePathStyle = true,
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            MaxErrorRetry = 2
        };
    }
}
