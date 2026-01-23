using TC.Agro.SharedKernel.Domain.Events;

namespace TC.Agro.Identity.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public sealed class ApplicationDbContext : DbContext, IUnitOfWork
    {
        public DbSet<UserAggregate> Users { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schemas.Default);

            // Ignore domain events - they are not persisted as separate entities
            // Domain events are stored as part of the aggregate root via event sourcing or similar patterns
            modelBuilder.Ignore<BaseDomainEvent>();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);            
        }
    }
}
