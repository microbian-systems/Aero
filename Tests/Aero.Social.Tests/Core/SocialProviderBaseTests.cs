using Aero.Core;
using Aero.Core.Railway;
using Aero.Social.Abstractions;
using Aero.Social.Models;
using Aero.Social.Tests.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Tests.Core;

public class SocialProviderBaseTests : ProviderTestBase
{
    private readonly Mock<ILogger<SocialProviderBase>> _loggerMock = new();

    [Fact]
    public void CheckScopes_WhenAllScopesGranted_ShouldNotThrow()
    {
        var required = new[] { "read", "write", "email" };
        var granted = new[] { "read", "write", "email", "profile" };

        var result = CreateTestProvider().TestCheckScopes(required, granted);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void CheckScopes_WhenScopeMissing_ShouldReturnFailure()
    {
        var required = new[] { "read", "write", "admin" };
        var granted = new[] { "read", "write" };

        var result = CreateTestProvider().TestCheckScopes(required, granted);

        result.IsFailure.ShouldBeTrue();
        ((Result<NoneType, AeroError>.Failure)result).Error.ShouldBeOfType<AeroError.Forbidden>();
    }

    [Fact]
    public void CheckScopes_WhenScopesGrantedAsString_ShouldParseCorrectly()
    {
        var required = new[] { "read", "write" };
        var grantedScopes = "read write email";

        var result = CreateTestProvider().TestCheckScopes(required, grantedScopes);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void CheckScopes_WhenScopesGrantedAsCommaDelimited_ShouldParseCorrectly()
    {
        var required = new[] { "read", "write" };
        var grantedScopes = "read,write,email";

        var result = CreateTestProvider().TestCheckScopes(required, grantedScopes);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void CheckScopes_ShouldBeCaseInsensitive()
    {
        var required = new[] { "READ", "Write" };
        var granted = new[] { "read", "WRITE" };

        var result = CreateTestProvider().TestCheckScopes(required, granted);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void MakeId_ShouldGenerateStringOfCorrectLength()
    {
        var result = CreateTestProvider().TestMakeId(10);
        
        result.Length.ShouldBe(10);
    }

    [Fact]
    public void MakeId_ShouldGenerateAlphanumericString()
    {
        var result = CreateTestProvider().TestMakeId(20);
        
        result.ShouldAllBe(c => char.IsLetterOrDigit(c));
    }

    [Fact]
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

public class TestSocialProvider : SocialProviderBase
{
    public TestSocialProvider(HttpClient httpClient, ILogger<SocialProviderBase> logger) 
        : base(httpClient, logger)
    {
    }

    public override string Identifier => "test";
    public override string Name => "Test Provider";
    public override string[] Scopes => new[] { "read", "write" };

    public override int MaxLength(object? additionalSettings = null) => 1000;

    public override Task<Result<PostResponse[], AeroError>> PostAsync(
        string id, string accessToken, List<PostDetails> posts, 
        Integration integration, CancellationToken cancellationToken = default)
        => Task.FromResult<Result<PostResponse[], AeroError>>(Array.Empty<PostResponse>());

    public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(new GenerateAuthUrlResponse());

    public override Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<Result<AuthTokenDetails, AeroError>>(new AuthTokenDetails());

    public override Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
        => Task.FromResult<Result<AuthTokenDetails, AeroError>>(new AuthTokenDetails());

    public Result<NoneType, AeroError> TestCheckScopes(string[] required, string[] granted)
        => CheckScopes(required, granted);

    public Result<NoneType, AeroError> TestCheckScopes(string[] required, string grantedScopes)
        => CheckScopes(required, grantedScopes);

    public string TestMakeId(int length) => MakeId(length);
}
