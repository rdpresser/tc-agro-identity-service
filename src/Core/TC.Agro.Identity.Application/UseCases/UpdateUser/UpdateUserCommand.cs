using CacheTagCatalog = TC.Agro.Identity.Application.Abstractions.CacheTags;

namespace TC.Agro.Identity.Application.UseCases.UpdateUser
{
    public sealed record UpdateUserCommand(
        Guid Id,
        string Name,
        string Email,
        string Username) : IBaseCommand<UpdateUserResponse>, IInvalidateCache
    {
        public IReadOnlyCollection<string> CacheTags =>
        [
            CacheTagCatalog.Users
        ];
    }
}
