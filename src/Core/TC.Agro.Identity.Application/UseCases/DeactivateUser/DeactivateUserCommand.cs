using CacheTagCatalog = TC.Agro.Identity.Application.Abstractions.CacheTags;

namespace TC.Agro.Identity.Application.UseCases.DeactivateUser
{
    public sealed record DeactivateUserCommand(
        Guid Id) : IBaseCommand<DeactivateUserResponse>, IInvalidateCache
    {
        public IReadOnlyCollection<string> CacheTags =>
        [
            CacheTagCatalog.Users,
            CacheTagCatalog.UserList,
            CacheTagCatalog.UserByEmail
        ];
    }
}
