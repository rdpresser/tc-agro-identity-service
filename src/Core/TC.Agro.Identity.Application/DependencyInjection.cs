using TC.Agro.Identity.Application.UseCases.ReSyncUsers;

namespace TC.Agro.Identity.Application
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddScoped<IReSyncUsersUseCase, ReSyncUsersCommandHandler>();

            return services;
        }
    }
}
