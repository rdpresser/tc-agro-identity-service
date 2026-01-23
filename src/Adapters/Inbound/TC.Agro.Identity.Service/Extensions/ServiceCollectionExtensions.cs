using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json.Converters;
using TC.Agro.SharedKernel.Infrastructure.Caching.HealthCheck;

namespace TC.Agro.Identity.Service.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            // Configure FluentValidation globally
            ConfigureFluentValidationGlobals();

            // Add Marten configuration only if not testing
            if (!builder.Environment.IsEnvironment("Testing"))
            {
                ////builder.AddWolverineMessaging(connectionFactory, mqConnectionFactory); -> será usado mais tarde
            }

            services.AddHttpClient()
                .AddCorrelationIdGenerator()
                .AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>()
                .AddCaching()
                .AddCustomAuthentication(builder.Configuration)
                .AddCustomFastEndpoints(builder.Configuration)
                .AddCustomHealthCheck();
            ////.AddCustomOpenTelemetry(builder, builder.Configuration);------------------> será usado mais tarde

            return services;
        }

        // Health Checks with Enhanced Telemetry
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddNpgSql(sp =>
                {
                    var connectionProvider = sp.GetRequiredService<DbConnectionFactory>();
                    return connectionProvider.ConnectionString;
                },
                    name: "PostgreSQL",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["db", "sql", "postgres", "live", "ready"])

                .AddTypeActivatedCheck<RedisHealthCheck>("Redis",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["cache", "redis", "live", "ready"])

                .AddCheck("Memory", () =>
                {
                    var allocated = GC.GetTotalMemory(false);
                    var mb = allocated / 1024 / 1024;

                    return mb < 1024
                    ? HealthCheckResult.Healthy($"Memory usage: {mb} MB")
                    : HealthCheckResult.Degraded($"High memory usage: {mb} MB");
                },
                    tags: ["memory", "system", "live"])
                .AddCheck("Custom-Metrics", () =>
                {
                    // Add any custom health logic for your metrics system
                    return HealthCheckResult.Healthy("Custom metrics are functioning");
                },
                    tags: ["metrics", "telemetry", "live"]);

            return services;
        }

        // FastEndpoints Configuration
        public static IServiceCollection AddCustomFastEndpoints(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddFastEndpoints(dicoveryOptions =>
            {
                dicoveryOptions.Assemblies = [typeof(Application.DependencyInjection).Assembly];
            })
            .SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.Title = "TC.CloudGames.Users API";
                    s.Version = "v1";
                    s.Description = "User API for TC.CloudGames";
                    s.MarkNonNullablePropsAsRequired();
                };

                o.RemoveEmptyRequestSchema = true;
                o.NewtonsoftSettings = s => { s.Converters.Add(new StringEnumConverter()); };
            });

            return services;
        }

        // Authentication and Authorization
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Auth:Jwt").Get<JwtOptions>();

            services.AddAuthenticationJwtBearer(s => s.SigningKey = jwtSettings!.SecretKey)
                    .AddAuthorization()
                    .AddHttpContextAccessor();

            return services;
        }

        // FluentValidation Global Setup
        private static void ConfigureFluentValidationGlobals()
        {
            ValidatorOptions.Global.PropertyNameResolver = (type, memberInfo, expression) => memberInfo?.Name;
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => memberInfo?.Name;
            ValidatorOptions.Global.ErrorCodeResolver = validator => validator.Name;
            ValidatorOptions.Global.LanguageManager = new LanguageManager
            {
                Enabled = true,
                Culture = new System.Globalization.CultureInfo("en")
            };
        }

        private static IServiceCollection AddCaching(this IServiceCollection services)
        {
            // Add FusionCache for caching
            services.AddFusionCache()
                .WithDefaultEntryOptions(options =>
                {
                    options.Duration = TimeSpan.FromSeconds(20);
                    options.DistributedCacheDuration = TimeSpan.FromSeconds(30);
                })
                .WithDistributedCache(sp =>
                {
                    var cacheProvider = sp.GetRequiredService<ICacheProvider>();

                    var options = new RedisCacheOptions { Configuration = cacheProvider.ConnectionString, InstanceName = cacheProvider.InstanceName };

                    return new RedisCache(options);
                })
                .WithSerializer(new FusionCacheSystemTextJsonSerializer())
                .AsHybridCache();

            return services;
        }

        ////private static WebApplicationBuilder AddWolverineMessaging(this WebApplicationBuilder builder, DbConnectionFactory connectionFactory, RabbitMqConnectionFactory mqConnectionFactory)
        ////{
        ////    builder.Host.UseWolverine(opts =>
        ////    {
        ////        // -------------------------------
        ////        // Define schema for Wolverine durability and Postgres persistence
        ////        // -------------------------------

        ////        opts.UseSystemTextJsonForSerialization();
        ////        opts.ApplicationAssembly = typeof(Program).Assembly;

        ////        opts.Durability.MessageStorageSchemaName = Schemas.Wolverine;
        ////        opts.ServiceName = "tcagro";

        ////        // -------------------------------
        ////        // Persist Wolverine messages in Postgres using the same schema
        ////        // -------------------------------
        ////        opts.PersistMessagesWithPostgresql(connectionFactory.ConnectionString, Schemas.Wolverine);

        ////        opts.Policies.OnAnyException()
        ////            .RetryWithCooldown(
        ////                TimeSpan.FromMilliseconds(200),
        ////                TimeSpan.FromMilliseconds(400),
        ////                TimeSpan.FromMilliseconds(600),
        ////                TimeSpan.FromMilliseconds(800),
        ////                TimeSpan.FromMilliseconds(1000)
        ////            );

        ////        // -------------------------------
        ////        // Enable durable local queues and auto transaction application
        ////        // -------------------------------
        ////        opts.Policies.UseDurableLocalQueues();
        ////        opts.Policies.AutoApplyTransactions();

        ////        // -------------------------------
        ////        // Load and configure message broker
        ////        // -------------------------------

        ////        var rabbitOpts = opts.UseRabbitMq(factory =>
        ////        {
        ////            factory.Uri = new Uri(mqConnectionFactory.ConnectionString);
        ////            factory.VirtualHost = mqConnectionFactory.VirtualHost;
        ////            factory.ClientProperties["application"] = "TC.Agro.Identity.Service";
        ////            factory.ClientProperties["environment"] = builder.Environment.EnvironmentName;
        ////        });

        ////        if (mqConnectionFactory.AutoProvision) rabbitOpts.AutoProvision();
        ////        if (mqConnectionFactory.UseQuorumQueues) rabbitOpts.UseQuorumQueues();
        ////        if (mqConnectionFactory.AutoPurgeOnStartup) rabbitOpts.AutoPurgeOnStartup();

        ////        // Durable outbox 
        ////        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();

        ////        var exchangeName = $"{mqConnectionFactory.Exchange}-exchange";
        ////        // Register messages
        ////        opts.PublishMessage<EventContext<UserCreatedIntegrationEvent>>()
        ////            .ToRabbitExchange(exchangeName)
        ////            .BufferedInMemory()
        ////            .UseDurableOutbox();
        ////    });

        ////    // -------------------------------
        ////    // Ensure all messaging resources and schema are created at startup
        ////    // -------------------------------
        ////    builder.Services.AddResourceSetupOnStartup();

        ////    return builder;
        ////}
    }
}
