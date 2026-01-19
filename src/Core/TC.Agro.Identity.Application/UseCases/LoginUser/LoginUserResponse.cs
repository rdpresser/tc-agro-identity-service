namespace TC.Agro.Identity.Application.UseCases.LoginUser
{
    public sealed record LoginUserResponse(
        string JwtToken,
        string Email);
}
