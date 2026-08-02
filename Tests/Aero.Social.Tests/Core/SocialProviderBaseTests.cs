using TUnit.Core;
using Aero.Core;
using Aero.Core.Railway;
using Aero.Social.Abstractions;
using Aero.Social.Models;
using Aero.Social.Tests.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Tests.Core;

/// <summary>
/// Represents a class for SocialProviderBaseTests.
/// </summary>
public class SocialProviderBaseTests : ProviderTestBase
{
    private readonly Mock<ILogger<SocialProviderBase>> _loggerMock = new();

        /// <summary>
    /// CheckScopes_WhenAllScopesGranted_ShouldNotThrow method.
    /// </summary>
[Test]
    public void CheckScopes_WhenAllScopesGranted_ShouldNotThrow()
    {
        var required = new[] { "read", "write", "email" };
        var granted = new[] { "read", "write", "email", "profile" };

        var result = CreateTestProvider().TestCheckScopes(required, granted);

        result.IsSuccess.ShouldBeTrue();
    }

        /// <summary>
    /// CheckScopes_WhenScopeMissing_ShouldReturnFailure method.
    /// </summary>
[Test]
    public void CheckScopes_WhenScopeMissing_ShouldReturnFailure()
    {
        var required = new[] { "read", "write", "admin" };
        var granted = new[] { "read", "write" };

        var result = CreateTestProvider().TestCheckScopes(required, granted);

        result.IsFailure.ShouldBeTrue();
        ((Result<NoneType, AeroError>.Failure)result).Error.ShouldBeOfType<AeroError.Forbidden>();
    }

        /// <summary>
    /// CheckScopes_WhenScopesGrantedAsString_ShouldParseCorrectly method.
    /// </summary>
[Test]
    public void CheckScopes_WhenScopesGrantedAsString_ShouldParseCorrectly()
    {
        var required = new[] { "read", "write" };
        var grantedScopes = "read write email";

        var result = CreateTestProvider().TestCheckScopes(required, grantedScopes);

        result.IsSuccess.ShouldBeTrue();
    }

        /// <summary>
    /// CheckScopes_WhenScopesGrantedAsCommaDelimited_ShouldParseCorrectly method.
    /// </summary>
[Test]
    public void CheckScopes_WhenScopesGrantedAsCommaDelimited_ShouldParseCorrectly()
    {
        var required = new[] { "read", "write" };
        var grantedScopes = "read,write,email";

        var result = CreateTestProvider().TestCheckScopes(required, grantedScopes);

        result.IsSuccess.ShouldBeTrue();
    }

        /// <summary>
    /// CheckScopes_ShouldBeCaseInsensitive method.
    /// </summary>
[Test]
    public void CheckScopes_ShouldBeCaseInsensitive()
    {
        var required = new[] { "READ", "Write" };
        var granted = new[] { "read", "WRITE" };

        var result = CreateTestProvider().TestCheckScopes(required, granted);

        result.IsSuccess.ShouldBeTrue();
    }

        /// <summary>
    /// MakeId_ShouldGenerateStringOfCorrectLength method.
    /// </summary>
[Test]
    public void MakeId_ShouldGenerateStringOfCorrectLength()
    {
        var result = CreateTestProvider().TestMakeId(10);
        
        result.Length.ShouldBe(10);
    }

        /// <summary>
    /// MakeId_ShouldGenerateAlphanumericString method.
    /// </summary>
[Test]
    public void MakeId_ShouldGenerateAlphanumericString()
    {
        var result = CreateTestProvider().TestMakeId(20);
        
        result.ShouldAllBe(c => char.IsLetterOrDigit(c));
    }

        /// <summary>
    /// MakeId_ShouldGenerateDifferentValues method.
    /// </summary>
[Test]
    public void MakeId_ShouldGenerateDifferentValues()
    {
        var results = new HashSet<string>();
        for (int i = 0; i < 100; i++)
        {
            results.Add(CreateTestProvider().TestMakeId(10));
        }
        
        results.Count.ShouldBeGreaterThan(90);
    }

    private TestSocialProvider CreateTestProvider()
    {
        return new TestSocialProvider(HttpClient, _loggerMock.Object);
    }
}

/// <summary>
/// Represents a class for TestSocialProvider.
/// </summary>
public class TestSocialProvider : SocialProviderBase
{
        /// <summary>
    /// Initializes a new instance of the <see cref="TestSocialProvider"/> class.
    /// </summary>
public TestSocialProvider(HttpClient httpClient, ILogger<SocialProviderBase> logger) 
        : base(httpClient, logger)
    {
    }

        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "test";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "Test Provider";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => new[] { "read", "write" };

        /// <summary>
    /// MaxLength method.
    /// </summary>
public override int MaxLength(object? additionalSettings = null) => 1000;

        /// <summary>
    /// PostAsync method.
    /// </summary>
public override Task<Result<PostResponse[], AeroError>> PostAsync(
        string id, string accessToken, List<PostDetails> posts, 
        Integration integration, CancellationToken cancellationToken = default)
        => Task.FromResult<Result<PostResponse[], AeroError>>(Array.Empty<PostResponse>());

        /// <summary>
    /// GenerateAuthUrlAsync method.
    /// </summary>
public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(new GenerateAuthUrlResponse());

        /// <summary>
    /// AuthenticateAsync method.
    /// </summary>
public override Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<Result<AuthTokenDetails, AeroError>>(new AuthTokenDetails());

        /// <summary>
    /// RefreshTokenAsync method.
    /// </summary>
public override Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
        => Task.FromResult<Result<AuthTokenDetails, AeroError>>(new AuthTokenDetails());

        /// <summary>
    /// TestCheckScopes method.
    /// </summary>
public Result<NoneType, AeroError> TestCheckScopes(string[] required, string[] granted)
        => CheckScopes(required, granted);

        /// <summary>
    /// TestCheckScopes method.
    /// </summary>
public Result<NoneType, AeroError> TestCheckScopes(string[] required, string grantedScopes)
        => CheckScopes(required, grantedScopes);

        /// <summary>
    /// TestMakeId method.
    /// </summary>
public string TestMakeId(int length) => MakeId(length);
}
