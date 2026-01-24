namespace TC.Agro.Identity.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IUserAggregateRepository, UserAggregateRepository>();

            // -------------------------------
            // EF Core with Wolverine Integration
            // IMPORTANT: Use AddDbContextWithWolverineIntegration instead of AddDbContext
            // This enables the transactional outbox pattern with Wolverine
            // -------------------------------
            services.AddDbContextWithWolverineIntegration<ApplicationDbContext>((sp, opts) =>
            {
                var dbFactory = sp.GetRequiredService<DbConnectionFactory>();

                opts.UseNpgsql(dbFactory.ConnectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default);
                });

                opts.UseSnakeCaseNamingConvention();

                // Enable lazy loading proxies
                ////opts.UseLazyLoadingProxies();

                // Use Serilog for EF Core logging
                opts.LogTo(Log.Logger.Information, LogLevel.Information);

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    opts.EnableSensitiveDataLogging(true);
                    opts.EnableDetailedErrors();
                }

            });

            // Unit of Work (for simple handlers that don't need outbox)
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Transactional Outbox (for handlers that publish integration events)
            // Uses Wolverine for atomic EF Core persistence + message publishing
            services.AddScoped<ITransactionalOutbox, WolverineEfCoreOutbox>();

            SharedKernel.Infrastructure.DependencyInjection.AddAgroInfrastructure(services, configuration);

            return services;
        }
    }
}
