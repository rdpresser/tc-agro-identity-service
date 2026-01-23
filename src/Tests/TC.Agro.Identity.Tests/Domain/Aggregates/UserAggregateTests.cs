using TC.Agro.Identity.Domain.Aggregates;

namespace TC.Agro.Identity.Tests.Domain.Aggregates
{
    public class UserAggregateTests
    {
        [Fact]
        public void Create_WithValidParameters_ShouldSucceed()
        {
            // Arrange
            var name = "John Doe";
            var email = "john@example.com";
            var username = "johndoe";
            var password = "Test@1234";
            var role = "User";

            // Act
            var result = UserAggregate.Create(name, email, username, password, role);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(name);
            result.Value.Email.Value.Should().Be(email);
            result.Value.Username.Should().Be(username);
            result.Value.Role.Value.Should().Be(role);
            result.Value.IsActive.Should().BeTrue();
            result.Value.Id.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("", "john@example.com", "johndoe", "Test@1234", "User")]
        [InlineData("John Doe", "invalid-email", "johndoe", "Test@1234", "User")]
        [InlineData("John Doe", "john@example.com", "", "Test@1234", "User")]
        [InlineData("John Doe", "john@example.com", "johndoe", "weak", "User")]
        [InlineData("John Doe", "john@example.com", "johndoe", "Test@1234", "InvalidRole")]
        public void Create_WithInvalidParameters_ShouldReturnValidationErrors(
            string name, string email, string username, string password, string role)
        {
            // Act
            var result = UserAggregate.Create(name, email, username, password, role);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ValidationErrors.Should().NotBeEmpty();
        }

        [Fact]
        public void Create_ShouldGenerateUserCreatedDomainEvent()
        {
            // Arrange & Act
            var result = UserAggregate.Create(
                "John Doe",
                "john@example.com",
                "johndoe",
                "Test@1234",
                "User");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.UncommittedEvents.Should().ContainSingle();
            result.Value.UncommittedEvents[0].Should().BeOfType<UserAggregate.UserCreatedDomainEvent>();
        }

        [Fact]
        public void Update_WithValidParameters_ShouldSucceed()
        {
            // Arrange
            var user = UserAggregate.Create(
                "John Doe",
                "john@example.com",
                "johndoe",
                "Test@1234",
                "User").Value;

            var newName = "Jane Doe";
            var newUsername = "janedoe";

            // Act
            var result = user.Update(newName, newUsername);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.Name.Should().Be(newName);
            user.Username.Should().Be(newUsername);
            user.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Update_WithNewRole_ShouldUpdateRole()
        {
            // Arrange
            var user = UserAggregate.Create(
                "John Doe",
                "john@example.com",
                "johndoe",
                "Test@1234",
                "User").Value;

            // Act
            var result = user.Update("John Doe", "johndoe", "Admin");

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.Role.Value.Should().Be("Admin");
        }

        [Fact]
        public void Deactivate_WhenActive_ShouldSucceed()
        {
            // Arrange
            var user = UserAggregate.Create(
                "John Doe",
                "john@example.com",
                "johndoe",
                "Test@1234",
                "User").Value;

            // Act
            var result = user.Deactivate();

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Deactivate_WhenAlreadyInactive_ShouldReturnError()
        {
            // Arrange
            var user = UserAggregate.Create(
                "John Doe",
                "john@example.com",
                "johndoe",
                "Test@1234",
                "User").Value;

            user.Deactivate();

            // Act
            var result = user.Deactivate();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ValidationErrors.Should().Contain(e => e.Identifier == "User.AlreadyDeactivated");
        }

        [Fact]
        public void Activate_WhenInactive_ShouldSucceed()
        {
            // Arrange
            var user = UserAggregate.Create(
                "John Doe",
                "john@example.com",
                "johndoe",
                "Test@1234",
                "User").Value;

            user.Deactivate();

            // Act
            var result = user.Activate();

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Activate_WhenAlreadyActive_ShouldReturnError()
        {
            // Arrange
            var user = UserAggregate.Create(
                "John Doe",
                "john@example.com",
                "johndoe",
                "Test@1234",
                "User").Value;

            // Act
            var result = user.Activate();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ValidationErrors.Should().Contain(e => e.Identifier == "User.AlreadyActive");
        }
    }
}
