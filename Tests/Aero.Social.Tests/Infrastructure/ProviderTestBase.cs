using System.Net;
using Aero.Social.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Aero.Social.Tests.Infrastructure;

/// <summary>
/// Represents a class for ProviderTestBase.
/// </summary>
public abstract class ProviderTestBase
{
        /// <summary>
    /// LoggerMock.
    /// </summary>
protected readonly Mock<ILogger> LoggerMock = new();
        /// <summary>
    /// HttpHandler.
    /// </summary>
protected readonly MockHttpMessageHandler HttpHandler = new();
        /// <summary>
    /// ConfigurationMock.
    /// </summary>
protected readonly Mock<IConfiguration> ConfigurationMock = new();
        /// <summary>
    /// Gets or sets the Http Client.
    /// </summary>
protected HttpClient HttpClient => new(HttpHandler);

        /// <summary>
    /// CreateLoggerMock method.
    /// </summary>
protected Mock<ILogger<T>> CreateLoggerMock<T>()
    {
        return new Mock<ILogger<T>>();
    }

        /// <summary>
    /// SetupConfiguration method.
    /// </summary>
protected void SetupConfiguration(string key, string value)
    {
        ConfigurationMock.Setup(x => x[key]).Returns(value);
        ConfigurationMock.Setup(x => x.GetSection(key)).Returns(new Mock<IConfigurationSection>().Object);
    }

        /// <summary>
    /// SetupConfigurationSection method.
    /// </summary>
protected void SetupConfigurationSection(string key, Dictionary<string, string> values)
    {
        var sectionMock = new Mock<IConfigurationSection>();
        foreach (var kvp in values)
        {
            sectionMock.Setup(x => x[kvp.Key]).Returns(kvp.Value);
        }
        ConfigurationMock.Setup(x => x.GetSection(key)).Returns(sectionMock.Object);
    }

        /// <summary>
    /// VerifyLog method.
    /// </summary>
protected void VerifyLog(LogLevel level, Times times)
    {
        LoggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

        /// <summary>
    /// VerifyLog method.
    /// </summary>
protected static void VerifyLog<T>(Mock<ILogger<T>> loggerMock, LogLevel level, Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

        /// <summary>
    /// AssertContainsScope method.
    /// </summary>
protected static void AssertContainsScope(string[] required, string[] granted)
    {
        foreach (var scope in required)
        {
            if (!granted.Contains(scope, StringComparer.OrdinalIgnoreCase))
            {
                throw new NotEnoughScopesException();
            }
        }
    }
}
