namespace TC.Agro.Identity.Application.Abstractions.Ports
{
    public interface IUnitOfWork
    {
        public DbSet<UserAggregate> Users { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
