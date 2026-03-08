using Ardalis.Result;
using TC.Agro.Identity.Application.Abstractions.Ports;
using TC.Agro.Identity.Application.UseCases.LoginUser;
using TC.Agro.Identity.Tests.TestHelpers;
using TC.Agro.SharedKernel.Infrastructure.Authentication;

namespace TC.Agro.Identity.Tests.Application.UseCases.LoginUser;

public sealed class LoginUserCommandHandlerTests
{
    private readonly IUserReadStore _readStore = A.Fake<IUserReadStore>();
    private readonly ITokenProvider _tokenProvider = A.Fake<ITokenProvider>();

    public LoginUserCommandHandlerTests()
    {
        FastEndpointsTestBootstrap.EnsureInitialized();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCredentialsAreInvalid_ShouldReturnUnauthorized()
    {
        var command = new LoginUserCommand("john.smith@tcagro.com", "Wrong@123");

        A.CallTo(() => _readStore.GetUserTokenInfoAsync(command.Email, command.Password, A<CancellationToken>._))
            .Returns((UserTokenProvider?)null);

        var sut = new LoginUserCommandHandler(_readStore, _tokenProvider);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Unauthorized);
        A.CallTo(() => _tokenProvider.Create(A<UserTokenProvider>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCredentialsAreValid_ShouldReturnJwtAndEmail()
    {
        var command = new LoginUserCommand("john.smith@tcagro.com", "Strong@123");
        var tokenUser = new UserTokenProvider(
            Id: Guid.NewGuid(),
            Name: "John Smith",
            Email: command.Email,
            Username: "johnsmith",
            Role: "Producer");

        A.CallTo(() => _readStore.GetUserTokenInfoAsync(command.Email, command.Password, A<CancellationToken>._))
            .Returns(tokenUser);
        A.CallTo(() => _tokenProvider.Create(tokenUser)).Returns("jwt-token-value");

        var sut = new LoginUserCommandHandler(_readStore, _tokenProvider);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.JwtToken.ShouldBe("jwt-token-value");
        result.Value.Email.ShouldBe(command.Email);

        A.CallTo(() => _tokenProvider.Create(tokenUser)).MustHaveHappenedOnceExactly();
    }
}
