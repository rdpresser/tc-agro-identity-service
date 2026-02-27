namespace TC.Agro.Identity.Application.UseCases.ChangePassword
{
    public sealed record ChangePasswordResponse(
        Guid Id,
        string Email,
        string Message);
}
