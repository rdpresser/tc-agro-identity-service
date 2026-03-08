using Microsoft.EntityFrameworkCore;
using TC.Agro.Identity.Infrastructure;
using TC.Agro.Identity.Infrastructure.Repositores;
using TC.Agro.Identity.Tests.TestHelpers;

namespace TC.Agro.Identity.Tests.Infrastructure.Repositores;

public sealed class UserReadStoreTests
{
    [Fact]
    public void Constructor_WhenDbContextIsNull_ShouldThrowArgumentNullException()
    {
        var userContext = TestUserContextFactory.CreateAdmin();

        Should.Throw<ArgumentNullException>(() => new UserReadStore(null!, userContext));
    }

    [Fact]
    public void Constructor_WhenUserContextIsNull_ShouldThrowArgumentNullException()
    {
        using var dbContext = CreateDbContext();

        Should.Throw<ArgumentNullException>(() => new UserReadStore(dbContext, null!));
    }

    [Fact]
    public async Task GetByEmailAsync_WhenEmailIsBlank_ShouldReturnNullWithoutQuerying()
    {
        using var dbContext = CreateDbContext();
        var userContext = TestUserContextFactory.CreateAdmin();
        var sut = new UserReadStore(dbContext, userContext);

        var result = await sut.GetByEmailAsync("   ", CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task IsEmailAvailableAsync_WhenEmailIsBlank_ShouldReturnFalseWithoutQuerying()
    {
        using var dbContext = CreateDbContext();
        var userContext = TestUserContextFactory.CreateAdmin();
        var sut = new UserReadStore(dbContext, userContext);

        var result = await sut.IsEmailAvailableAsync(" ", CancellationToken.None);

        result.ShouldBeFalse();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"identity-tests-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
