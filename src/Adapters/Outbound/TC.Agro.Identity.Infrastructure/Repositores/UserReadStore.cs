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

            return await _dbContext.Users
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
            var baseQuery = _dbContext.Users
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var pattern = $"%{query.Filter}%";

                baseQuery = baseQuery.Where(u =>
                    EF.Functions.ILike(u.Name, pattern) ||
                    EF.Functions.ILike(u.Username, pattern) ||
                    EF.Functions.ILike(u.Email.Value, pattern) ||
                    EF.Functions.ILike(u.Role.Value, pattern)
                );
            }

            // Get total count before pagination
            var totalCount = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            // sorting - IMPORTANT: Must have OrderBy before Skip/Take to avoid unpredictable results
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var isAscending = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

                baseQuery = query.SortBy.ToLowerInvariant() switch
                {
                    "id" => isAscending
                        ? baseQuery.OrderBy(u => u.Id)
                        : baseQuery.OrderByDescending(u => u.Id),
                    "name" => isAscending
                        ? baseQuery.OrderBy(u => u.Name)
                        : baseQuery.OrderByDescending(u => u.Name),
                    "username" => isAscending
                        ? baseQuery.OrderBy(u => u.Username)
                        : baseQuery.OrderByDescending(u => u.Username),
                    "email" => isAscending
                        ? baseQuery.OrderBy(u => u.Email.Value)
                        : baseQuery.OrderByDescending(u => u.Email.Value),
                    "role" => isAscending
                        ? baseQuery.OrderBy(u => u.Role.Value)
                        : baseQuery.OrderByDescending(u => u.Role.Value),
                    "createdat" => isAscending
                        ? baseQuery.OrderBy(u => u.CreatedAt)
                        : baseQuery.OrderByDescending(u => u.CreatedAt),
                    _ => baseQuery.OrderByDescending(u => u.CreatedAt)
                };
            }
            else
            {
                baseQuery = baseQuery.OrderByDescending(u => u.CreatedAt);
            }

            var users = await baseQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
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
