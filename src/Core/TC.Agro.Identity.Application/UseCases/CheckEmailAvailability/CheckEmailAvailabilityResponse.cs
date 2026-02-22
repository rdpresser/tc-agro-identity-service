namespace TC.Agro.Identity.Application.UseCases.CheckEmailAvailability
{
    public sealed class CheckEmailAvailabilityResponse
    {
        public required string Email { get; init; }
        public required bool IsAvailable { get; init; }
    }
}
