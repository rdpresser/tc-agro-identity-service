namespace TC.Agro.Identity.Application.UseCases.LoginUser
{
    public sealed class LoginUserCommandValidator : Validator<LoginUserCommand>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage("Email is required.")
                    .WithErrorCode($"{nameof(LoginUserCommand.Email)}.Required")
                .EmailAddress()
                    .WithMessage("Invalid email format.")
                    .WithErrorCode($"{nameof(LoginUserCommand.Email)}.InvalidEmailFormat");

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage("Password is required.")
                    .WithErrorCode($"{nameof(LoginUserCommand.Password)}.Required");
        }
    }
}
