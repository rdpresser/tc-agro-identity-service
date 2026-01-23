using TC.Agro.Identity.Domain.ValueObjects;

namespace TC.Agro.Identity.Tests.Domain.ValueObjects
{
    public class PasswordTests
    {
        [Theory]
        [InlineData("Test@123")]
        [InlineData("SecureP@ss1")]
        [InlineData("MyP@ssw0rd!")]
        public void Create_WithValidPassword_ShouldSucceed(string password)
        {
            // Act
            var result = Password.Create(password);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Hash.ShouldNotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_WithEmptyOrNullPassword_ShouldReturnRequiredError(string? password)
        {
            // Act
            var result = Password.Create(password!);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier == "Password.Required");
        }

        [Theory]
        [InlineData("Aa1@")]
        [InlineData("Short1!")]
        public void Create_WithShortPassword_ShouldReturnTooShortError(string password)
        {
            // Act
            var result = Password.Create(password);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier == "Password.TooShort");
        }

        [Theory]
        [InlineData("password")]
        [InlineData("PASSWORD")]
        [InlineData("Password")]
        [InlineData("password1")]
        [InlineData("Password1")]
        public void Create_WithWeakPassword_ShouldReturnWeakPasswordError(string password)
        {
            // Act
            var result = Password.Create(password);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier == "Password.Weak");
        }

        [Fact]
        public void Verify_WithCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            var plainPassword = "Test@1234";
            var passwordResult = Password.Create(plainPassword);

            // Act
            var isValid = passwordResult.Value.Verify(plainPassword);

            // Assert
            isValid.ShouldBeTrue();
        }

        [Fact]
        public void Verify_WithIncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            var passwordResult = Password.Create("Test@1234");

            // Act
            var isValid = passwordResult.Value.Verify("WrongPassword1!");

            // Assert
            isValid.ShouldBeFalse();
        }

        [Fact]
        public void FromHash_WithValidHash_ShouldSucceed()
        {
            // Arrange
            var passwordResult = Password.Create("Test@1234");
            var hash = passwordResult.Value.Hash;

            // Act
            var result = Password.FromHash(hash);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Hash.ShouldBe(hash);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void FromHash_WithEmptyOrNullHash_ShouldReturnRequiredError(string? hash)
        {
            // Act
            var result = Password.FromHash(hash!);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier == "Password.Required");
        }
    }
}
