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
                builder.AddWolverineMessaging();
            }

            services.AddHttpClient()
                .AddCorrelationIdGenerator()
                .AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>()
                .AddCaching()
                .AddCustomCors(builder.Configuration)
                .AddCustomAuthentication(builder.Configuration)
                .AddCustomFastEndpoints(builder.Configuration)
                .AddCustomHealthCheck();
            ////.AddCustomOpenTelemetry(builder, builder.Configuration);------------------> será usado mais tarde

            return services;
        }

        // CORS Configuration
        public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy", builder =>
                {
                    builder
                        .SetIsOriginAllowed((host) => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

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

        private static WebApplicationBuilder AddWolverineMessaging(this WebApplicationBuilder builder)
        {
            builder.Host.UseWolverine(opts =>
            {
                opts.UseSystemTextJsonForSerialization();
                opts.ServiceName = "tc-agro-identity-service";
                opts.ApplicationAssembly = typeof(Program).Assembly;

                // Include Application assembly for handlers
                opts.Discovery.IncludeAssembly(typeof(Application.DependencyInjection).Assembly);

                // -------------------------------
                // Durability schema (same database, different schema)
                // -------------------------------
                opts.Durability.MessageStorageSchemaName = DefaultSchemas.Wolverine;

                // IMPORTANT:
                // Use the same Postgres DB as EF Core.
                // This enables transactional outbox with EF Core.
                opts.PersistMessagesWithPostgresql(
                    PostgresHelper.Build(builder.Configuration).ConnectionString,
                    DefaultSchemas.Wolverine);

                // -------------------------------
                // Retry policy
                // -------------------------------
                opts.Policies.OnAnyException()
                    .RetryWithCooldown(
                        TimeSpan.FromMilliseconds(200),
                        TimeSpan.FromMilliseconds(400),
                        TimeSpan.FromMilliseconds(600),
                        TimeSpan.FromMilliseconds(800),
                        TimeSpan.FromMilliseconds(1000)
                    );

                // -------------------------------
                // Enable durable local queues and auto transaction application
                // -------------------------------
                opts.Policies.UseDurableLocalQueues();
                opts.Policies.AutoApplyTransactions();
                opts.UseEntityFrameworkCoreTransactions();

                // -------------------------------
                // OUTBOX (for sending)
                // -------------------------------
                opts.Policies.UseDurableOutboxOnAllSendingEndpoints();

                // -------------------------------
                // INBOX (for receiving) - optional but recommended
                // -------------------------------
                // This makes message consumption safe in face of retries/crashes.
                // It gives "at-least-once safe" processing with deduplication.
                ////opts.Policies.UseDurableInboxOnAllListeners();

                // -------------------------------
                // Load and configure message broker
                // -------------------------------
                var mqConnectionFactory = RabbitMqHelper.Build(builder.Configuration);

                var rabbitOpts = opts.UseRabbitMq(factory =>
                {
                    factory.Uri = new Uri(mqConnectionFactory.ConnectionString);
                    factory.VirtualHost = mqConnectionFactory.VirtualHost;
                    factory.ClientProperties["application"] = opts.ServiceName;
                    factory.ClientProperties["environment"] = builder.Environment.EnvironmentName;
                });

                if (mqConnectionFactory.AutoProvision)
                    rabbitOpts.AutoProvision();
                if (mqConnectionFactory.UseQuorumQueues)
                    rabbitOpts.UseQuorumQueues();
                if (mqConnectionFactory.AutoPurgeOnStartup)
                    rabbitOpts.AutoPurgeOnStartup();

                var exchangeName = $"{mqConnectionFactory.Exchange}-exchange";

                // -------------------------------
                // Publishing example
                // -------------------------------
                opts.PublishMessage<EventContext<UserCreatedIntegrationEvent>>()
                    .ToRabbitExchange(exchangeName)
                    .BufferedInMemory()
                    .UseDurableOutbox();

                // -------------------------------
                // Receiving (Inbox) - FUTURE USE (commented for now)
                // -------------------------------
                // When you want to consume events from other services:
                //
                // opts.ListenToRabbitQueue("tc-agro.identity.queue")
                //     .UseDurableInbox(); // ensures deduplication on receive
                //
                // Then create a handler class:
                // public static Task Handle(FarmCreatedIntegrationEvent evt) { ... }
            });

            // -------------------------------
            // Ensure all messaging resources and schema are created at startup
            // -------------------------------
            builder.Services.AddResourceSetupOnStartup();

            return builder;
        }
    }
}
