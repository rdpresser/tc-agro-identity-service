namespace TC.Agro.Identity.Application.UseCases.UpdateUser
{
    public sealed record UpdateUserResponse(
        Guid Id,
        string Name,
        string Email,
        string Username,
        string Role);
}
