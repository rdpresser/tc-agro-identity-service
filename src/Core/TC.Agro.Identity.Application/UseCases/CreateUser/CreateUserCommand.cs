using CacheTagCatalog = TC.Agro.Identity.Application.Abstractions.CacheTags;

namespace TC.Agro.Identity.Application.UseCases.CreateUser
{
    public sealed record CreateUserCommand(
        string Name,
        string Email,
        string Username,
        string Password,
        string Role) : IBaseCommand<CreateUserResponse>, IInvalidateCache
    {
        public IReadOnlyCollection<string> CacheTags =>
        [
            CacheTagCatalog.Users,
            CacheTagCatalog.UserList,
            CacheTagCatalog.UserByEmail
        ];
    }
}
