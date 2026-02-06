namespace TC.Agro.Identity.Application.UseCases.DeactivateUser
{
    public sealed record DeactivateUserResponse(
        Guid Id,
        string Message);
}
