using Aero.Identity.Models;
using Xunit;

namespace Aero.Identity.Tests.Models;

public class AeroRoleTests
{
    [Fact]
    public void CanInitializeAeroRole()
    {
        // Arrange & Act
        var role = new AeroRole();

        // Assert
        Assert.NotNull(role);
    }

    [Fact]
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
