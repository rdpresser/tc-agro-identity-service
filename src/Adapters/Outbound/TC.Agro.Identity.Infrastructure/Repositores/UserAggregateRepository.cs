namespace TC.Agro.Identity.Infrastructure.Repositores
{
    public sealed class UserAggregateRepository : BaseRepository<UserAggregate>, IUserAggregateRepository
    {
        public UserAggregateRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        /// <summary>
        /// Loads UserAggregate with owned entities (Email, Role) explicitly included.
        /// FindAsync does not guarantee eager loading of owned entities, which causes
        /// null constraint violations on SaveChanges when only non-owned properties are modified.
        /// </summary>
        public override async Task<UserAggregate?> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .FirstOrDefaultAsync(u => u.Id == aggregateId && u.IsActive, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsNoTracking()
                .AnyAsync(userAggregate => EF.Functions.ILike(userAggregate.Email.Value, email) && userAggregate.IsActive, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
