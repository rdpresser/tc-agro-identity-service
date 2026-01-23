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
            result.IsSuccess.ShouldBeTrue();
            result.Value.Name.ShouldBe(name);
            result.Value.Email.Value.ShouldBe(email);
            result.Value.Username.ShouldBe(username);
            result.Value.Role.Value.ShouldBe(role);
            result.Value.IsActive.ShouldBeTrue();
            result.Value.Id.ShouldNotBe(Guid.Empty);
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
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldNotBeEmpty();
        }
    }
}
