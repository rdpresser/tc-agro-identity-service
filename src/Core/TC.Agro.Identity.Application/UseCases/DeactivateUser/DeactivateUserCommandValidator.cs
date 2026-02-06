namespace TC.Agro.Identity.Application.UseCases.DeactivateUser
{
    public sealed class DeactivateUserCommandValidator : Validator<DeactivateUserCommand>
    {
        public DeactivateUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                    .WithMessage("User ID is required.")
                    .WithErrorCode($"{nameof(DeactivateUserCommand.Id)}.Required")
                .Must(id => id != Guid.Empty)
                    .WithMessage("User ID must be a valid GUID.")
                    .WithErrorCode($"{nameof(DeactivateUserCommand.Id)}.Invalid");
        }
    }
}
