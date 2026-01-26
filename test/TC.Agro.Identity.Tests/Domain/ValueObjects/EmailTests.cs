using TC.Agro.Identity.Domain.ValueObjects;

namespace TC.Agro.Identity.Tests.Domain.ValueObjects
{
    public class EmailTests
    {
        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user.name@domain.org")]
        [InlineData("john_doe123@company.co.uk")]
        public void Create_WithValidEmail_ShouldSucceed(string emailValue)
        {
            // Act
            var result = Email.Create(emailValue);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Value.ShouldBe(emailValue.ToLowerInvariant());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_WithEmptyOrNullEmail_ShouldReturnRequiredError(string? emailValue)
        {
            // Act
            var result = Email.Create(emailValue!);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier == "Email.Required");
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("test@")]
        [InlineData("@domain.com")]
        [InlineData("test.domain.com")]
        public void Create_WithInvalidFormat_ShouldReturnInvalidFormatError(string emailValue)
        {
            // Act
            var result = Email.Create(emailValue);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier == "Email.InvalidFormat");
        }

        [Fact]
        public void Create_WithExceedingMaxLength_ShouldReturnMaxLengthError()
        {
            // Arrange
            var longEmail = new string('a', 190) + "@example.com"; // > 200 chars

            // Act
            var result = Email.Create(longEmail);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier == "Email.MaximumLength");
        }

        [Fact]
        public void FromDb_WithValidEmail_ShouldSucceed()
        {
            // Arrange
            var emailValue = "test@example.com";

            // Act
            var result = Email.FromDb(emailValue);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Value.ShouldBe(emailValue);
        }

        [Fact]
        public void ImplicitConversion_ToString_ShouldReturnValue()
        {
            // Arrange
            var email = Email.Create("test@example.com").Value;

            // Act
            string value = email;

            // Assert
            value.ShouldBe("test@example.com");
        }
    }
}
