namespace TC.Agro.Identity.Service.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserServices(this IServiceCollection services, WebApplicationBuilder builder, DbConnectionFactory connectionFactory, RabbitMqConnectionFactory mqConnectionFactory)
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
                ////.AddCaching() -> será usado mais tarde
                .AddCustomAuthentication(builder.Configuration);
            ////.AddCustomFastEndpoints(builder.Configuration)
            ////.ConfigureAppSettings(builder.Configuration)
            ////.AddCustomHealthCheck()
            ////.AddCustomOpenTelemetry(builder, builder.Configuration);

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

        //private static IServiceCollection AddCaching(this IServiceCollection services)
        //{
        //    // Add FusionCache for caching
        //    services.AddFusionCache()
        //        .WithDefaultEntryOptions(options =>
        //        {
        //            options.Duration = TimeSpan.FromSeconds(20);
        //            options.DistributedCacheDuration = TimeSpan.FromSeconds(30);
        //        })
        //        .WithDistributedCache(sp =>
        //        {
        //            var cacheProvider = sp.GetRequiredService<ICacheProvider>();

        //            var options = new RedisCacheOptions { Configuration = cacheProvider.ConnectionString, InstanceName = cacheProvider.InstanceName };

        //            return new RedisCache(options);
        //        })
        //        .WithSerializer(new FusionCacheSystemTextJsonSerializer())
        //        .AsHybridCache();

        //    return services;
        //}

        //private static WebApplicationBuilder AddWolverineMessaging(this WebApplicationBuilder builder, DbConnectionFactory connectionFactory, RabbitMqConnectionFactory mqConnectionFactory)
        //{
        //    builder.Host.UseWolverine(opts =>
        //    {
        //        // -------------------------------
        //        // Define schema for Wolverine durability and Postgres persistence
        //        // -------------------------------

        //        opts.UseSystemTextJsonForSerialization();
        //        opts.ApplicationAssembly = typeof(Program).Assembly;

        //        opts.Durability.MessageStorageSchemaName = Schemas.Wolverine;
        //        opts.ServiceName = "tcagro";

        //        // -------------------------------
        //        // Persist Wolverine messages in Postgres using the same schema
        //        // -------------------------------
        //        opts.PersistMessagesWithPostgresql(connectionFactory.ConnectionString, Schemas.Wolverine);

        //        opts.Policies.OnAnyException()
        //            .RetryWithCooldown(
        //                TimeSpan.FromMilliseconds(200),
        //                TimeSpan.FromMilliseconds(400),
        //                TimeSpan.FromMilliseconds(600),
        //                TimeSpan.FromMilliseconds(800),
        //                TimeSpan.FromMilliseconds(1000)
        //            );

        //        // -------------------------------
        //        // Enable durable local queues and auto transaction application
        //        // -------------------------------
        //        opts.Policies.UseDurableLocalQueues();
        //        opts.Policies.AutoApplyTransactions();

        //        // -------------------------------
        //        // Load and configure message broker
        //        // -------------------------------

        //        var rabbitOpts = opts.UseRabbitMq(factory =>
        //        {
        //            factory.Uri = new Uri(mqConnectionFactory.ConnectionString);
        //            factory.VirtualHost = mqConnectionFactory.VirtualHost;
        //            factory.ClientProperties["application"] = "TC.Agro.Identity.Service";
        //            factory.ClientProperties["environment"] = builder.Environment.EnvironmentName;
        //        });

        //        if (mqConnectionFactory.AutoProvision) rabbitOpts.AutoProvision();
        //        if (mqConnectionFactory.UseQuorumQueues) rabbitOpts.UseQuorumQueues();
        //        if (mqConnectionFactory.AutoPurgeOnStartup) rabbitOpts.AutoPurgeOnStartup();

        //        // Durable outbox 
        //        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();

        //        var exchangeName = $"{mqConnectionFactory.Exchange}-exchange";
        //        // Register messages
        //        opts.PublishMessage<EventContext<UserCreatedIntegrationEvent>>()
        //            .ToRabbitExchange(exchangeName)
        //            .BufferedInMemory()
        //            .UseDurableOutbox();
        //    });

        //    // -------------------------------
        //    // Ensure all messaging resources and schema are created at startup
        //    // -------------------------------
        //    builder.Services.AddResourceSetupOnStartup();

        //    return builder;
        //}
    }
}
