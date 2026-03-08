using Ardalis.Result;
using TC.Agro.Identity.Application.Abstractions.Ports;
using TC.Agro.Identity.Application.UseCases.GetUserByEmail;
using TC.Agro.Identity.Tests.TestHelpers;
using TC.Agro.SharedKernel.Infrastructure.UserClaims;

namespace TC.Agro.Identity.Tests.Application.UseCases.GetUserByEmail;

public sealed class GetUserByEmailQueryHandlerTests
{
    private readonly IUserReadStore _readStore = A.Fake<IUserReadStore>();

    public GetUserByEmailQueryHandlerTests()
    {
        FastEndpointsTestBootstrap.EnsureInitialized();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCallerIsNotAdminAndRequestsDifferentEmail_ShouldReturnUnauthorized()
    {
        var userContext = TestUserContextFactory.CreateUser(email: "caller@tcagro.com");
        var query = new GetUserByEmailQuery { Email = "other@tcagro.com" };

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(query, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Unauthorized);
        A.CallTo(() => _readStore.GetByEmailAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        const string email = "missing@tcagro.com";
        var userContext = TestUserContextFactory.CreateAdmin();

        A.CallTo(() => _readStore.GetByEmailAsync(email, A<CancellationToken>._))
            .Returns((UserByEmailResponse?)null);

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(new GetUserByEmailQuery { Email = email }, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueryIsAuthorizedAndUserExists_ShouldReturnResponse()
    {
        const string email = "owner@tcagro.com";
        var userContext = TestUserContextFactory.CreateUser(email: email);
        var expected = new UserByEmailResponse
        {
            Id = Guid.NewGuid(),
            Name = "Owner",
            Username = "owner001",
            Email = email,
            Role = "Producer",
            IsActive = true
        };

        A.CallTo(() => _readStore.GetByEmailAsync(email, A<CancellationToken>._)).Returns(expected);

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(new GetUserByEmailQuery { Email = email }, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(expected.Id);
        result.Value.Email.ShouldBe(email);
        result.Value.Name.ShouldBe("Owner");
    }

    private GetUserByEmailQueryHandler CreateHandler(IUserContext userContext)
        => new(_readStore, userContext);
}
