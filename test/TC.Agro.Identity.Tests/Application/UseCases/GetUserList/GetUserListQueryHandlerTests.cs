using TC.Agro.Identity.Application.Abstractions.Ports;
using TC.Agro.Identity.Application.UseCases.GetUserList;

namespace TC.Agro.Identity.Tests.Application.UseCases.GetUserList;

public sealed class GetUserListQueryHandlerTests
{
    private readonly IUserReadStore _readStore = A.Fake<IUserReadStore>();

    public GetUserListQueryHandlerTests()
    {
        TestHelpers.FastEndpointsTestBootstrap.EnsureInitialized();
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreReturnsNoUsers_ShouldReturnEmptyPaginatedResponse()
    {
        var query = new GetUserListQuery
        {
            PageNumber = 2,
            PageSize = 10,
            SortBy = "name",
            SortDirection = "asc",
            Filter = ""
        };

        A.CallTo(() => _readStore.GetUserListAsync(query, A<CancellationToken>._))
            .Returns((Array.Empty<UserResponse>(), 0));

        var sut = new GetUserListQueryHandler(_readStore);

        var result = await sut.ExecuteAsync(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
        result.Value.Data.ShouldBeEmpty();
        result.Value.PageNumber.ShouldBe(2);
        result.Value.PageSize.ShouldBe(10);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreReturnsUsers_ShouldReturnPaginatedPayload()
    {
        var query = new GetUserListQuery
        {
            PageNumber = 1,
            PageSize = 2,
            SortBy = "name",
            SortDirection = "asc",
            Filter = "john"
        };

        var users = new[]
        {
            new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "John Smith",
                Username = "john001",
                Email = "john.smith@tcagro.com",
                Role = "Producer",
                IsActive = true
            },
            new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Johnny Doe",
                Username = "john002",
                Email = "johnny.doe@tcagro.com",
                Role = "User",
                IsActive = true
            }
        };

        A.CallTo(() => _readStore.GetUserListAsync(query, A<CancellationToken>._))
            .Returns((users, 5));

        var sut = new GetUserListQueryHandler(_readStore);

        var result = await sut.ExecuteAsync(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(5);
        result.Value.Data.Count.ShouldBe(2);
        result.Value.Data[0].Name.ShouldBe("John Smith");
        result.Value.Data[1].Name.ShouldBe("Johnny Doe");
    }
}
