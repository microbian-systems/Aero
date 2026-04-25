using Aero.Auth.Services;
using Aero.Common.Web.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using TUnit.Core;

namespace Aero.Auth.Tests.Services;

public class DefaultApiKeyFactoryTests
{
    [Test]
    public async Task GenerateApiKey_Should_Respect_Length_And_Prefix()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            KeyPrefix = "AERO-",
            LengthOfKey = 32,
            GenerateUrlSafeKeys = true
        };
        var optionsMock = Substitute.For<IOptions<ApiKeyOptions>>();
        optionsMock.Value.Returns(options);
        var factory = new DefaultApiKeyFactory(optionsMock);

        // Act
        var key = factory.GenerateApiKey();

        // Assert
        key.Should().NotBeNull();
        key.Should().StartWith("AERO-");
        key!.Length.Should().Be(32);
    }

    [Test]
    public async Task GenerateApiKey_Should_Be_Unique()
    {
        // Arrange
        var options = new ApiKeyOptions
        {
            KeyPrefix = "KEY-",
            LengthOfKey = 24,
            GenerateUrlSafeKeys = false
        };
        var optionsMock = Substitute.For<IOptions<ApiKeyOptions>>();
        optionsMock.Value.Returns(options);
        var factory = new DefaultApiKeyFactory(optionsMock);

        // Act
        var key1 = factory.GenerateApiKey();
        var key2 = factory.GenerateApiKey();

        // Assert
        key1.Should().NotBe(key2);
    }
}