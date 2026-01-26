namespace TC.Agro.Identity.Application.Abstractions.Ports
{
    public interface IUserReadStore
    {
        Task<UserByEmailResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserTokenProvider?> GetUserTokenInfoAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserListResponse>> GetUserListAsync(GetUserListQuery query, CancellationToken cancellationToken = default);
    }
}
