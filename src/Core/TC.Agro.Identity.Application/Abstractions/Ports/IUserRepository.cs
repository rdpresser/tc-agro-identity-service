using TC.Agro.Identity.Application.UseCases.GetUserByEmail;
using TC.Agro.Identity.Application.UseCases.GetUserList;
using TC.Agro.Identity.Domain.Aggregates;
using TC.Agro.SharedKernel.Application.Ports;
using TC.Agro.SharedKernel.Infrastructure.Authentication;

namespace TC.Agro.Identity.Application.Abstractions.Ports
{
    public interface IUserRepository : IBaseRepository<UserAggregate>
    {
        Task<UserByEmailResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<UserTokenProvider?> GetUserTokenInfoAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserListResponse>> GetUserListAsync(GetUserListQuery query, CancellationToken cancellationToken = default);
    }
}
