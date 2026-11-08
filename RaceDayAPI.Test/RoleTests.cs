using Xunit;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDayAPI.Test
{
    public class RoleTests
    {
        [Fact]
        public void Role_WithValidProperties_ShouldInitialize()
        {
            // Arrange & Act
            var role = new Role
            {
                RoleId = 1,
                RoleName = "Admin"
            };

            // Assert
            Assert.NotNull(role);
            Assert.Equal(1, role.RoleId);
            Assert.Equal("Admin", role.RoleName);
        }

        [Fact]
        public void RequireRoleAttribute_ShouldAcceptValidRole()
        {
            // Arrange
            var attribute = new RequireRoleAttribute("Admin");

            // Act & Assert
            Assert.NotNull(attribute);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Organizer")]
        [InlineData("Participant")]
        public void Role_WithDifferentRoleNames_ShouldBeValid(string roleName)
        {
            // Arrange & Act
            var role = new Role { RoleId = 1, RoleName = roleName };

            // Assert
            Assert.Equal(roleName, role.RoleName);
        }
    }
}
