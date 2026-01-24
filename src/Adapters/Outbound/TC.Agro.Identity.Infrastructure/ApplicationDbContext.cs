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

        // Explicit implementation of IUnitOfWork.SaveChangesAsync
        // This ensures the UnitOfWork pattern works correctly with the DbContext
        async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken ct)
        {
            // Log for debugging (using Serilog directly since logger injection would require constructor changes)
            Log.Debug("ApplicationDbContext.SaveChangesAsync called. ChangeTracker has {Count} entries",
                ChangeTracker.Entries().Count());

            var entriesBeforeSave = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                           e.State == EntityState.Modified ||
                           e.State == EntityState.Deleted)
                .ToList();

            Log.Debug("Entries to save: Added={Added}, Modified={Modified}, Deleted={Deleted}",
                entriesBeforeSave.Count(e => e.State == EntityState.Added),
                entriesBeforeSave.Count(e => e.State == EntityState.Modified),
                entriesBeforeSave.Count(e => e.State == EntityState.Deleted));

            if (!entriesBeforeSave.Any())
            {
                Log.Warning("SaveChangesAsync called but ChangeTracker has no pending changes!");
                return 0;
            }

            var result = await base.SaveChangesAsync(ct);

            Log.Information("Successfully saved {Count} changes to database", result);

            return result;
        }
    }
}
