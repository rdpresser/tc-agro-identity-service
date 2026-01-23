namespace TC.Agro.Identity.Infrastructure.Repositores
{
    public abstract class BaseRepository<TAggregate> : IBaseRepository<TAggregate>
        where TAggregate : BaseAggregateRoot
    {
        protected ApplicationDbContext DbContext { get; }
        protected DbSet<TAggregate> DbSet { get; }

        protected BaseRepository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            DbSet = dbContext.Set<TAggregate>();
        }

        public void Add(TAggregate aggregate) => DbSet.Add(aggregate);

        public void Delete(TAggregate aggregateRoot) => DbSet.Remove(aggregateRoot);

        public async Task DeleteAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            var aggregateRoot = await GetByIdAsync(aggregateId, cancellationToken)
                .ConfigureAwait(false);

            if (aggregateRoot != null) Delete(aggregateRoot);
        }

        public Task<IEnumerable<TAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregate?> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}