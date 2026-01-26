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
            result.IsSuccess.ShouldBeTrue();
            Role.ValidRoles.ShouldContain(result.Value.Value);
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
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier == "Role.Invalid");
        }

        [Fact]
        public void PredefinedRoles_ShouldHaveCorrectValues()
        {
            // Assert
            Role.User.Value.ShouldBe("User");
            Role.Admin.Value.ShouldBe("Admin");
            Role.Moderator.Value.ShouldBe("Moderator");
        }

        [Fact]
        public void ValidRoles_ShouldContainAllPredefinedRoles()
        {
            // Assert
            Role.ValidRoles.ShouldContain("User");
            Role.ValidRoles.ShouldContain("Admin");
            Role.ValidRoles.ShouldContain("Moderator");
            Role.ValidRoles.Length.ShouldBe(3);
        }

        [Fact]
        public void Create_WithCaseInsensitiveRole_ShouldNormalizeToCorrectCase()
        {
            // Act
            var result = Role.Create("admin");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Value.ShouldBe("Admin");
        }

        [Fact]
        public void ImplicitConversion_ToRole_ShouldReturnValue()
        {
            // Arrange
            var role = Role.Create("User").Value;

            // Act
            string value = role;

            // Assert
            value.ShouldBe("User");
        }
    }
}
