using TC.Agro.Identity.Domain.Aggregates;
using TC.Agro.Identity.Domain.ValueObjects;

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
            result.Value.UncommittedEvents.Count.ShouldBe(1);
            result.Value.UncommittedEvents[^1].ShouldBeOfType<UserAggregate.UserCreatedDomainEvent>();
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

        [Fact]
        public void UpdateInfo_WithValidData_ShouldUpdateStateAndAppendDomainEvent()
        {
            var aggregate = UserAggregate.Create(
                name: "John Doe",
                emailValue: "john.doe@tcagro.com",
                username: "john001",
                passwordValue: "Strong@123",
                roleValue: "User").Value;

            var newEmail = Email.Create("jane.doe@tcagro.com").Value;

            var result = aggregate.UpdateInfo("Jane Doe", newEmail, "jane001");

            result.IsSuccess.ShouldBeTrue();
            aggregate.Name.ShouldBe("Jane Doe");
            aggregate.Email.Value.ShouldBe("jane.doe@tcagro.com");
            aggregate.Username.ShouldBe("jane001");
            aggregate.UncommittedEvents[^1].ShouldBeOfType<UserAggregate.UserUpdatedDomainEvent>();
        }

        [Fact]
        public void UpdateInfo_WhenEmailIsNull_ShouldReturnValidationError()
        {
            var aggregate = UserAggregate.Create(
                name: "John Doe",
                emailValue: "john.doe@tcagro.com",
                username: "john001",
                passwordValue: "Strong@123",
                roleValue: "User").Value;

            var result = aggregate.UpdateInfo("Jane Doe", null!, "jane001");

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(error => error.Identifier == "Email.Required");
        }

        [Fact]
        public void UpdateInfo_WithInvalidNameAndUsername_ShouldReturnValidationErrors()
        {
            var aggregate = UserAggregate.Create(
                name: "John Doe",
                emailValue: "john.doe@tcagro.com",
                username: "john001",
                passwordValue: "Strong@123",
                roleValue: "User").Value;

            var newEmail = Email.Create("valid@tcagro.com").Value;

            var result = aggregate.UpdateInfo("", newEmail, "a");

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(error => error.Identifier == "Name.Required");
            result.ValidationErrors.ShouldContain(error => error.Identifier == "Username.TooShort");
        }

        [Fact]
        public void Deactivate_WhenUserIsActive_ShouldDeactivateAndEmitDomainEvent()
        {
            var aggregate = UserAggregate.Create(
                name: "John Doe",
                emailValue: "john.doe@tcagro.com",
                username: "john001",
                passwordValue: "Strong@123",
                roleValue: "User").Value;

            var result = aggregate.Deactivate();

            result.IsSuccess.ShouldBeTrue();
            aggregate.IsActive.ShouldBeFalse();
            aggregate.UncommittedEvents[^1].ShouldBeOfType<UserAggregate.UserDeactivatedDomainEvent>();
        }

        [Fact]
        public void Deactivate_WhenUserIsAlreadyInactive_ShouldReturnValidationError()
        {
            var aggregate = UserAggregate.Create(
                name: "John Doe",
                emailValue: "john.doe@tcagro.com",
                username: "john001",
                passwordValue: "Strong@123",
                roleValue: "User").Value;

            aggregate.Deactivate().IsSuccess.ShouldBeTrue();

            var result = aggregate.Deactivate();

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(error => error.Identifier == "User.AlreadyInactive");
        }

        [Fact]
        public void ChangePassword_WithValidNewPassword_ShouldUpdateHashAndAppendDomainEvent()
        {
            var aggregate = UserAggregate.Create(
                name: "John Doe",
                emailValue: "john.doe@tcagro.com",
                username: "john001",
                passwordValue: "Strong@123",
                roleValue: "User").Value;

            var originalHash = aggregate.PasswordHash.Hash;

            var result = aggregate.ChangePassword("NewStrong@456");

            result.IsSuccess.ShouldBeTrue();
            aggregate.PasswordHash.Hash.ShouldNotBe(originalHash);
            aggregate.PasswordHash.Verify("NewStrong@456").ShouldBeTrue();
            aggregate.PasswordHash.Verify("Strong@123").ShouldBeFalse();
            aggregate.UncommittedEvents[^1].ShouldBeOfType<UserAggregate.UserPasswordChangedDomainEvent>();
        }

        [Fact]
        public void ChangePassword_WhenUsingCurrentPassword_ShouldReturnValidationError()
        {
            const string currentPassword = "Strong@123";

            var aggregate = UserAggregate.Create(
                name: "John Doe",
                emailValue: "john.doe@tcagro.com",
                username: "john001",
                passwordValue: currentPassword,
                roleValue: "User").Value;

            var result = aggregate.ChangePassword(currentPassword);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(error => error.Identifier == "Password.SameAsCurrent");
        }

        [Fact]
        public void ChangePassword_WithWeakPassword_ShouldReturnValidationError()
        {
            var aggregate = UserAggregate.Create(
                name: "John Doe",
                emailValue: "john.doe@tcagro.com",
                username: "john001",
                passwordValue: "Strong@123",
                roleValue: "User").Value;

            var result = aggregate.ChangePassword("weakpass");

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(error => error.Identifier == "Password.Weak");
        }
    }
}
