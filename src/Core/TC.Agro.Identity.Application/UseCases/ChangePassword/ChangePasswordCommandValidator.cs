namespace TC.Agro.Identity.Application.UseCases.ChangePassword
{
    public sealed class ChangePasswordCommandValidator : Validator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage("Email is required.")
                    .WithErrorCode($"{nameof(ChangePasswordCommand.Email)}.Required")
                .EmailAddress()
                    .WithMessage("Invalid email format.")
                    .WithErrorCode($"{nameof(ChangePasswordCommand.Email)}.InvalidFormat");

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage("Password is required.")
                    .WithErrorCode($"{nameof(ChangePasswordCommand.Password)}.Required")
                .MinimumLength(8)
                    .WithMessage("Password must be at least 8 characters long.")
                    .WithErrorCode($"{nameof(ChangePasswordCommand.Password)}.MinimumLength")
                .Matches(@"[A-Z]")
                    .WithMessage("Password must contain at least one uppercase letter.")
                    .WithErrorCode($"{nameof(ChangePasswordCommand.Password)}.Uppercase")
                .Matches(@"[a-z]")
                    .WithMessage("Password must contain at least one lowercase letter.")
                    .WithErrorCode($"{nameof(ChangePasswordCommand.Password)}.Lowercase")
                .Matches(@"\d")
                    .WithMessage("Password must contain at least one number.")
                    .WithErrorCode($"{nameof(ChangePasswordCommand.Password)}.Digit")
                .Matches(@"[\W_]")
                    .WithMessage("Password must contain at least one special character.")
                    .WithErrorCode($"{nameof(ChangePasswordCommand.Password)}.SpecialCharacter");
        }
    }
}
