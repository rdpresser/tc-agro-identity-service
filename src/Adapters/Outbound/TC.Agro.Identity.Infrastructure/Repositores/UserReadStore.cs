namespace TC.Agro.Identity.Infrastructure.Repositores
{
    public sealed class UserReadStore : IUserReadStore
    {
        private readonly ApplicationDbContext _dbContext;

        public UserReadStore(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<UserByEmailResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var projection = await _dbContext.Set<UserAggregate>()
                .AsNoTracking()
                .Where(u => u.IsActive && EF.Functions.ILike(u.Email.Value, email))
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Username,
                    x.Email,
                    x.Role,
                    x.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (projection is null)
                return null;

            return new UserByEmailResponse
            {
                Id = projection.Id,
                Name = projection.Name,
                Username = projection.Username,
                Email = projection.Email,
                Role = projection.Role
            };
        }

        public async Task<UserTokenProvider?> GetUserTokenInfoAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var userAggregate = await _dbContext.Set<UserAggregate>()
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

        public async Task<IReadOnlyList<UserListResponse>> GetUserListAsync(
            GetUserListQuery query,
            CancellationToken cancellationToken = default)
        {
            var usersQuery = _dbContext.Set<UserAggregate>()
                .AsNoTracking()
                .Where(u => u.IsActive);

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var pattern = $"%{query.Filter}%";

                usersQuery = usersQuery.Where(u =>
                    EF.Functions.ILike(u.Name, pattern) ||
                    EF.Functions.ILike(u.Username, pattern) ||
                    EF.Functions.ILike(u.Email.Value, pattern) ||
                    EF.Functions.ILike(u.Role.Value, pattern)
                );
            }

            // sorting
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var isAscending = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

                usersQuery = query.SortBy.ToLower() switch
                {
                    "name" => isAscending ? usersQuery.OrderBy(u => u.Name) : usersQuery.OrderByDescending(u => u.Name),
                    "username" => isAscending ? usersQuery.OrderBy(u => u.Username) : usersQuery.OrderByDescending(u => u.Username),

                    // IMPORTANT: use EF.Property for ValueObjects
                    "email" => isAscending
                        ? usersQuery.OrderBy(u => u.Email.Value)
                        : usersQuery.OrderByDescending(u => u.Email.Value),

                    "role" => isAscending
                        ? usersQuery.OrderBy(u => u.Role.Value)
                        : usersQuery.OrderByDescending(u => u.Role.Value),

                    _ => usersQuery
                };
            }

            usersQuery = usersQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize);

            return await usersQuery
                .Select(u => new UserListResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    Username = u.Username,
                    Email = u.Email.Value,
                    Role = u.Role.Value
                })
                .ToListAsync(cancellationToken);
        }

    }
}
