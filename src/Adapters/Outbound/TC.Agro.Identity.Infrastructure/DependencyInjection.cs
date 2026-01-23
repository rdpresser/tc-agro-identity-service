namespace TC.Agro.Identity.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IUserAggregateRepository, UserAggregateRepository>();
            services.AddDbContext<ApplicationDbContext>(contextLifetime: ServiceLifetime.Scoped, optionsLifetime: ServiceLifetime.Scoped);
            services.AddScoped<IUnitOfWork, ApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            SharedKernel.Infrastructure.DependencyInjection.AddAgroInfrastructure(services, configuration);

            return services;
        }
    }
}
