namespace TC.Agro.Identity.Application.UseCases.UpdateUser
{
    public sealed class UpdateUserCommandValidator : Validator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                    .WithMessage("User ID is required.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Id)}.Required")
                .Must(id => id != Guid.Empty)
                    .WithMessage("User ID must be a valid GUID.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Id)}.Invalid");

            #region Name | Validation Rules
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("Name is required.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Name)}.Required")
                .MinimumLength(3)
                    .WithMessage("Name must be at least 3 characters long.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Name)}.MinimumLength")
                .MaximumLength(100)
                    .WithMessage("Name must not exceed 100 characters.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Name)}.MaximumLength")
                .Matches(@"^[a-zA-Z ]+$")
                    .WithMessage("Name can only contain letters and spaces.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Name)}.InvalidCharacters");
            #endregion

            #region Email | Validation Rules
            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage("Email is required.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Email)}.Required")
                .EmailAddress()
                    .WithMessage("Invalid email format.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Email)}.InvalidFormat");
            #endregion

            #region Username | Validation Rules
            RuleFor(x => x.Username)
                .NotEmpty()
                    .WithMessage("Username is required.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Username)}.Required")
                .MinimumLength(3)
                    .WithMessage("Username must be at least 3 characters long.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Username)}.MinimumLength")
                .MaximumLength(100)
                    .WithMessage("Username must not exceed 100 characters.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Username)}.MaximumLength")
                .Matches(@"^[a-zA-Z][a-zA-Z0-9]*$")
                    .WithMessage("Username must start with a letter and can contain only letters and numbers.")
                    .WithErrorCode($"{nameof(UpdateUserCommand.Username)}.InvalidCharacters");
            #endregion
        }
    }
}
