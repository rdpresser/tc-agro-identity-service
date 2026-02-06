namespace TC.Agro.Identity.Application.UseCases.DeactivateUser
{
    public sealed record DeactivateUserCommand(
        Guid Id) : IBaseCommand<DeactivateUserResponse>;
}
