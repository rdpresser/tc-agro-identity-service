using CacheTagCatalog = TC.Agro.Identity.Application.Abstractions.CacheTags;

namespace TC.Agro.Identity.Application.UseCases.ChangePassword
{
    public sealed record ChangePasswordCommand(
        string Email,
        string Password) : IBaseCommand<ChangePasswordResponse>, IInvalidateCache
    {
        public IReadOnlyCollection<string> CacheTags =>
        [
            CacheTagCatalog.Users,
            CacheTagCatalog.UserList,
            CacheTagCatalog.UserByEmail
        ];
    }
}
