using Microsoft.AspNetCore.Builder;

namespace TC.Agro.Identity.Infrastructure.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Seeds initial users if database is empty.
        /// Should be called after migrations are applied.
        /// </summary>
        public static async Task SeedInitialDataAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                logger.LogInformation("Checking initial users...");

                var usersCreated = 0;

                // Check and create Admin user if not exists
                var adminEmail = "admin@tcagro.com";
                var adminExists = await dbContext.Set<UserAggregate>()
                    .AnyAsync(u => u.Email.Value == adminEmail);

                if (!adminExists)
                {
                    logger.LogInformation("Admin user not found. Creating admin user...");

                    var adminResult = UserAggregate.Create(
                        name: "System Administrator",
                        emailValue: adminEmail,
                        username: "admin",
                        passwordValue: "Admin@123",
                        roleValue: "Admin"
                    );

                    if (!adminResult.IsSuccess)
                    {
                        logger.LogError("Failed to create admin user: {Errors}",
                            string.Join(", ", adminResult.ValidationErrors.Select(e => e.ErrorMessage)));
                    }
                    else
                    {
                        await dbContext.Set<UserAggregate>().AddAsync(adminResult.Value);
                        usersCreated++;
                        logger.LogInformation("Admin user created successfully.");
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already exists. Skipping creation.");
                }

                // Check and create Producer user if not exists
                var producerEmail = "producer@tcagro.com";
                var producerExists = await dbContext.Set<UserAggregate>()
                    .AnyAsync(u => u.Email.Value == producerEmail);

                if (!producerExists)
                {
                    logger.LogInformation("Producer user not found. Creating producer user...");

                    var producerResult = UserAggregate.Create(
                        name: "Test Producer",
                        emailValue: producerEmail,
                        username: "producer",
                        passwordValue: "Producer@123",
                        roleValue: "Producer"
                    );

                    if (!producerResult.IsSuccess)
                    {
                        logger.LogError("Failed to create producer user: {Errors}",
                            string.Join(", ", producerResult.ValidationErrors.Select(e => e.ErrorMessage)));
                    }
                    else
                    {
                        await dbContext.Set<UserAggregate>().AddAsync(producerResult.Value);
                        usersCreated++;
                        logger.LogInformation("Producer user created successfully.");
                    }
                }
                else
                {
                    logger.LogInformation("Producer user already exists. Skipping creation.");
                }

                // Save changes if any users were created
                if (usersCreated > 0)
                {
                    await dbContext.SaveChangesAsync(CancellationToken.None);
                    logger.LogInformation(
                        "Successfully seeded {Count} initial user(s). Both required users are now in the database.",
                        usersCreated
                    );
                }
                else
                {
                    logger.LogInformation("All required users already exist. No seeding needed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding initial data. The application may not have initial users.");
                // Don't rethrow - seeding is not critical for application startup
            }
        }
    }
}
