using TUnit.Core;
using Aero.Identity.Models;

namespace Aero.Identity.Tests.Models;

/// <summary>
/// Represents a class for AeroRoleTests.
/// </summary>
public class AeroRoleTests
{
        /// <summary>
    /// CanInitializeAeroRole method.
    /// </summary>
[Test]
    public void CanInitializeAeroRole()
    {
        // Arrange & Act
        var role = new AeroRole();

        // Assert
        Assert.NotNull(role);
    }

        /// <summary>
    /// CanSetRoleProperties method.
    /// </summary>
[Test]
    public void CanSetRoleProperties()
    {
        // Arrange
        var role = new AeroRole();
        var roleId = "roles/1";
        var roleName = "Admin";
        var normalizedName = "ADMIN";

        // Act
        role.Id = roleId;
        role.Name = roleName;
        role.NormalizedName = normalizedName;

        // Assert
        Assert.Equal(roleId, role.Id);
        Assert.Equal(roleName, role.Name);
        Assert.Equal(normalizedName, role.NormalizedName);
}
}
