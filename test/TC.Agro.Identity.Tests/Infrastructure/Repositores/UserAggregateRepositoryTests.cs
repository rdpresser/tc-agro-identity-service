using Microsoft.EntityFrameworkCore;
using TC.Agro.Identity.Infrastructure;
using TC.Agro.Identity.Infrastructure.Repositores;

namespace TC.Agro.Identity.Tests.Infrastructure.Repositores;

public sealed class UserAggregateRepositoryTests
{
    [Fact]
    public async Task GetByEmailAsync_WhenEmailIsBlank_ShouldReturnNull()
    {
        using var dbContext = CreateDbContext();
        var sut = new UserAggregateRepository(dbContext);

        var result = await sut.GetByEmailAsync("   ", CancellationToken.None);

        result.ShouldBeNull();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"identity-tests-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
