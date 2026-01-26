namespace TC.Agro.Identity.Infrastructure.Repositores
{
    /// <summary>
    /// Base repository for Identity service aggregates.
    /// Extends SharedKernel's BaseRepository with ApplicationDbContext binding.
    /// </summary>
    public abstract class BaseRepository<TAggregate>
        : BaseRepository<TAggregate, ApplicationDbContext>
        where TAggregate : BaseAggregateRoot
    {
        protected BaseRepository(ApplicationDbContext dbContext) : base(dbContext) { }
    }
}