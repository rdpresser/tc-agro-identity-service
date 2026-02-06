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
                Role = projection.Role,
                IsActive = projection.IsActive
            };
        }

        public async Task<UserTokenProvider?> GetUserTokenInfoAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var userAggregate = await _dbContext.Set<UserAggregate>()
                .AsNoTracking()
                .SingleOrDefaultAsync(entity => EF.Functions.ILike(entity.Email.Value, email) && entity.IsActive, cancellationToken)
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
            var baseQuery = _dbContext.Set<UserAggregate>()
                .AsNoTracking()
                .Where(u => u.IsActive);

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

                baseQuery = query.SortBy.ToLower() switch
                {
                    "name" => isAscending ? baseQuery.OrderBy(u => u.Name) : baseQuery.OrderByDescending(u => u.Name),
                    "username" => isAscending ? baseQuery.OrderBy(u => u.Username) : baseQuery.OrderByDescending(u => u.Username),

                    // IMPORTANT: use EF.Property for ValueObjects
                    "email" => isAscending
                        ? baseQuery.OrderBy(u => u.Email.Value)
                        : baseQuery.OrderByDescending(u => u.Email.Value),

                    "role" => isAscending
                        ? baseQuery.OrderBy(u => u.Role.Value)
                        : baseQuery.OrderByDescending(u => u.Role.Value),

                    _ => baseQuery.OrderByDescending(u => u.Id)  // Default: order by ID descending
                };
            }
            else
            {
                // Default ordering when no sort specified (required for predictable pagination)
                baseQuery = baseQuery.OrderByDescending(u => u.Id);
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
