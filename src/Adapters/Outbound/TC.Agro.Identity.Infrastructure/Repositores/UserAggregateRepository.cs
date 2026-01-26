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
                .AnyAsync(userAggregate => EF.Functions.ILike(userAggregate.Email.Value, email), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
