using Aero.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Marten;
using Xunit;

namespace Aero.Identity.Tests.Extensions;

public class AeroIdentityExtensionsTests
{
    [Fact]
    public void AddAeroDbIdentity_RegistersStores()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockSession = new Mock<IDocumentSession>();
        services.AddScoped(_ => mockSession.Object);

        // Act
        services.AddIdentityCore<AeroUser>()
            .AddRoles<AeroRole>()
            //.AddAeroDbStores()
            ;

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var userStore = serviceProvider.GetService<IUserStore<AeroUser>>();
        var roleStore = serviceProvider.GetService<IRoleStore<AeroRole>>();

        Assert.NotNull(userStore);
        Assert.IsType<AeroUserStore<AeroUser>>(userStore);
        Assert.NotNull(roleStore);
        Assert.IsType<AeroRoleStore<AeroRole>>(roleStore);
    }

    [Fact]
    public void AddAeroDbIdentity_WithoutRoles_RegistersUserStoreOnly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockSession = new Mock<IDocumentSession>();
        services.AddScoped(_ => mockSession.Object);

        // Act
        services.AddIdentityCore<AeroUser>()
            //.AddAeroDbStores()
            ;

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var userStore = serviceProvider.GetService<IUserStore<AeroUser>>();
        var roleStore = serviceProvider.GetService<IRoleStore<AeroRole>>();

        Assert.NotNull(userStore);
        Assert.IsType<AeroUserStore<AeroUser>>(userStore);
        Assert.Null(roleStore);
    }

    /*
    [Fact]
    public void AddAeroAeroDbIdentity_RegistersEverything()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockStore = new Mock<Marten.Client.Documents.IDocumentStore>();
        var mockSession = new Mock<IDocumentSession>();
        services.AddSingleton(mockStore.Object);
        services.AddScoped(_ => mockSession.Object);

        // Act
        services.AddAeroAeroDbIdentity();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var userStore = serviceProvider.GetService<IUserStore<AeroUser>>();
        var security = serviceProvider.GetService<ISecurity>();

        Assert.NotNull(userStore);
        Assert.IsType<AeroUserStore<AeroUser>>(userStore);
        Assert.NotNull(security);
        Assert.IsType<AeroIdentitySecurity>(security);

        // Verify module registration
        Assert.True(App.Modules.Any(m => m.Instance is AeroIdentityModule));
    }
    */
}
