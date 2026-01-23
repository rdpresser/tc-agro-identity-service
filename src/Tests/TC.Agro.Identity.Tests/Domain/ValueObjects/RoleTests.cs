using TC.Agro.Identity.Domain.ValueObjects;

namespace TC.Agro.Identity.Tests.Domain.ValueObjects
{
    public class RoleTests
    {
        [Theory]
        [InlineData("User")]
        [InlineData("Admin")]
        [InlineData("Moderator")]
        [InlineData("user")]
        [InlineData("ADMIN")]
        public void Create_WithValidRole_ShouldSucceed(string roleValue)
        {
            // Act
            var result = Role.Create(roleValue);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().BeOneOf(Role.ValidRoles);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("InvalidRole")]
        [InlineData("SuperAdmin")]
        public void Create_WithInvalidRole_ShouldReturnInvalidError(string? roleValue)
        {
            // Act
            var result = Role.Create(roleValue!);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ValidationErrors.Should().Contain(e => e.Identifier == "Role.Invalid");
        }

        [Fact]
        public void PredefinedRoles_ShouldHaveCorrectValues()
        {
            // Assert
            Role.User.Value.Should().Be("User");
            Role.Admin.Value.Should().Be("Admin");
            Role.Moderator.Value.Should().Be("Moderator");
        }

        [Fact]
        public void ValidRoles_ShouldContainAllPredefinedRoles()
        {
            // Assert
            Role.ValidRoles.Should().Contain("User");
            Role.ValidRoles.Should().Contain("Admin");
            Role.ValidRoles.Should().Contain("Moderator");
            Role.ValidRoles.Should().HaveCount(3);
        }

        [Fact]
        public void Create_WithCaseInsensitiveRole_ShouldNormalizeToCorrectCase()
        {
            // Act
            var result = Role.Create("admin");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be("Admin");
        }

        [Fact]
        public void ImplicitConversion_ToRole_ShouldReturnValue()
        {
            // Arrange
            var role = Role.Create("User").Value;

            // Act
            string value = role;

            // Assert
            value.Should().Be("User");
        }
    }
}
