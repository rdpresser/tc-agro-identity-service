namespace TC.Agro.Identity.Infrastructure.Repositores
{
    public sealed class UserAggregateRepository : BaseRepository<UserAggregate>, IUserAggregateRepository
    {
        public UserAggregateRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsNoTracking()
                .AnyAsync(userAggregate => userAggregate.Email == Email.FromDb(email), cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserByEmailResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            ////var lowerEmail = email.ToLower();
            //testar qual a form que ira funcionar melhor com o EF, ou u.Email.value.ToLower() ou comparar direto com o Email.FromDb(email)

            var projection = await DbSet
                .Where(u => u.IsActive && u.Email == Email.FromDb(email))
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

            if (projection == null)
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
            var userAggregate = await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(entity =>
                    entity.Email == Email.FromDb(email), cancellationToken).ConfigureAwait(false);

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

        public async Task<IReadOnlyList<UserListResponse>> GetUserListAsync(GetUserListQuery query, CancellationToken cancellationToken = default)
        {
            var usersQuery = DbSet.Where(u => u.IsActive);

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                //Verificar se propriedade value será acessada corretamente pelo EF na hora de comparar os strings
                var filter = query.Filter.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
                    u.Username.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
                    u.Email.Value.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
                    u.Role.Value.Contains(filter, StringComparison.CurrentCultureIgnoreCase));
            }

            //aplicar sort dinamico usando a propriedade SortBy e SortDirection
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var isAscending = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
                usersQuery = query.SortBy.ToLower() switch
                {
                    "name" => isAscending ? usersQuery.OrderBy(u => u.Name) : usersQuery.OrderByDescending(u => u.Name),
                    "username" => isAscending ? usersQuery.OrderBy(u => u.Username) : usersQuery.OrderByDescending(u => u.Username),
                    "email" => isAscending ? usersQuery.OrderBy(u => u.Email.Value) : usersQuery.OrderByDescending(u => u.Email.Value),
                    "role" => isAscending ? usersQuery.OrderBy(u => u.Role.Value) : usersQuery.OrderByDescending(u => u.Role.Value),
                    _ => usersQuery
                };
            }

            //Paginação simples
            usersQuery = usersQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize);

            var userList = await usersQuery
                .Select(u => new UserListResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return userList;
        }
    }
}
