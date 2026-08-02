using Aero.Core.Configuration;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aero.SendGrid.Tests;

/// <summary>
/// Represents a class for SendGridTests.
/// </summary>
public class SendGridTests
{
    private readonly IConfiguration config;
    private readonly ILogger log;

        /// <summary>
    /// Initializes a new instance of the <see cref="SendGridTests"/> class.
    /// </summary>
public SendGridTests()
    {
        config = ConfigHelper.GetConfigurationRoot();
        this.log = A.Fake<ILogger>();
    }
}