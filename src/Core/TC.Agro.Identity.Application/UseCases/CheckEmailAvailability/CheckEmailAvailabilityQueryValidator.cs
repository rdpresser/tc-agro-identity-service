namespace TC.Agro.Identity.Application.UseCases.CheckEmailAvailability
{
    public sealed class CheckEmailAvailabilityQueryValidator : Validator<CheckEmailAvailabilityQuery>
    {
        public CheckEmailAvailabilityQueryValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage("Email is required.")
                    .WithErrorCode($"{nameof(CheckEmailAvailabilityQuery.Email)}.Required")
                .EmailAddress()
                    .WithMessage("Invalid email format.")
                    .WithErrorCode($"{nameof(CheckEmailAvailabilityQuery.Email)}.InvalidFormat");
        }
    }
}
