using TUnit.Core;
using Aero.Cms;
using Aero.Cms.Manager;

namespace Aero.Identity.Tests;

/// <summary>
/// Represents a class for AeroIdentityModuleTests.
/// </summary>
public class AeroIdentityModuleTests
{
        /// <summary>
    /// Init_RegistersPermissions method.
    /// </summary>
[Test]
    public void Init_RegistersPermissions()
    {
        // Arrange
        var module = new AeroIdentityModule();

        // Act
        module.Init();

        // Assert
        Assert.Contains(App.Permissions["Manager"], p => p.Name == Permissions.Users);
        Assert.Contains(App.Permissions["Manager"], p => p.Name == Permissions.Roles);
    }

        /// <summary>
    /// Init_AddsMenuItems method.
    /// </summary>
[Test]
    public void Init_AddsMenuItems()
    {
        // Arrange
        var module = new AeroIdentityModule();

        // Act
        module.Init();

        // Assert
        var systemMenu = Menu.Items["System"];
        Assert.Contains(systemMenu.Items, i => i.InternalId == "Users");
        Assert.Contains(systemMenu.Items, i => i.InternalId == "Roles");
}
}
