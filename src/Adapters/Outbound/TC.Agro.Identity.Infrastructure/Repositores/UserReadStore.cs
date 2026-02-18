using TC.Agro.Identity.Application.Abstractions;
using TC.Agro.Identity.Infrastructure.Extensions;
using TC.Agro.SharedKernel.Infrastructure.UserClaims;

namespace TC.Agro.Identity.Infrastructure.Repositores
{
    public sealed class UserReadStore : IUserReadStore
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserContext _userContext;

        public UserReadStore(ApplicationDbContext dbContext, IUserContext userContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        private IQueryable<UserAggregate> FilteredDbSet => _userContext.Role == AppConstants.AdminRole
            ? _dbContext.Users
            : _dbContext.Users.Where(x => x.Id == _userContext.Id);

        public async Task<UserByEmailResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await FilteredDbSet
                .AsNoTracking()
                .Where(u => EF.Functions.ILike(u.Email.Value, email))
                .Select(u => new UserByEmailResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    Username = u.Username,
                    Email = u.Email.Value,
                    Role = u.Role.Value,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserTokenProvider?> GetUserTokenInfoAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var userAggregate = await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(entity => EF.Functions.ILike(entity.Email.Value, email), cancellationToken)
                .ConfigureAwait(false);

            if (userAggregate is null)
                return null;

            if (!Password.FromHash(userAggregate.PasswordHash).Value.Verify(password))
                return null;

            return new UserTokenProvider(
                userAggregate.Id,
                userAggregate.Name,
                userAggregate.Email,
                userAggregate.Username,
                userAggregate.Role);
        }

        public async Task<(IReadOnlyList<UserResponse> Users, int TotalCount)> GetUserListAsync(
            GetUserListQuery query,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = FilteredDbSet
                .AsNoTracking()
                .ApplyTextFilter(query.Filter);

            var totalCount = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            baseQuery = baseQuery.ApplySorting(query.SortBy, query.SortDirection);

            var users = await baseQuery
                .ApplyPagination(query.PageNumber, query.PageSize)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    Username = u.Username,
                    Email = u.Email.Value,
                    Role = u.Role.Value,
                    IsActive = u.IsActive
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return ([.. users], totalCount);
        }
    }
}
