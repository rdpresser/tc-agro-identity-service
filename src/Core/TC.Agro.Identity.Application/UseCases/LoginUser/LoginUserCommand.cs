namespace TC.Agro.Identity.Application.UseCases.LoginUser
{
    public sealed record LoginUserCommand(
        string Email,
        string Password) : IBaseCommand<LoginUserResponse>;
}
