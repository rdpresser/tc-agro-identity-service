using Ardalis.Result;
using Microsoft.Extensions.Logging;
using TC.Agro.Identity.Application.Abstractions.Ports;
using TC.Agro.Identity.Application.UseCases.ChangePassword;
using TC.Agro.Identity.Domain.Aggregates;
using TC.Agro.Identity.Tests.TestHelpers;
using TC.Agro.SharedKernel.Application.Ports;

namespace TC.Agro.Identity.Tests.Application.UseCases.ChangePassword;

public sealed class ChangePasswordCommandHandlerTests
{
    private readonly IUserAggregateRepository _repository = A.Fake<IUserAggregateRepository>();
    private readonly ITransactionalOutbox _outbox = A.Fake<ITransactionalOutbox>();
    private readonly ILogger<ChangePasswordCommandHandler> _logger = A.Fake<ILogger<ChangePasswordCommandHandler>>();

    public ChangePasswordCommandHandlerTests()
    {
        FastEndpointsTestBootstrap.EnsureInitialized();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIsNotFound_ShouldReturnNotFound()
    {
        var command = new ChangePasswordCommand("john@tcagro.com", "NewStrong@123");
        var userContext = TestUserContextFactory.CreateAdmin();

        A.CallTo(() => _repository.GetByEmailAsync(command.Email, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(null));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIsInactive_ShouldReturnInvalid()
    {
        var aggregate = CreateAggregate();
        aggregate.Deactivate().IsSuccess.ShouldBeTrue();

        var command = new ChangePasswordCommand("john@tcagro.com", "NewStrong@123");
        var userContext = TestUserContextFactory.CreateAdmin();

        A.CallTo(() => _repository.GetByEmailAsync(command.Email, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(aggregate));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(error => error.Identifier == "User.Inactive");
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPasswordMatchesCurrentOne_ShouldReturnInvalid()
    {
        var aggregate = CreateAggregate();
        var command = new ChangePasswordCommand("john@tcagro.com", "Strong@123");
        var userContext = TestUserContextFactory.CreateAdmin();

        A.CallTo(() => _repository.GetByEmailAsync(command.Email, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(aggregate));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(error => error.Identifier == "Password.SameAsCurrent");
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPayloadIsValid_ShouldChangePasswordAndCommit()
    {
        var aggregate = CreateAggregate();
        var command = new ChangePasswordCommand("john@tcagro.com", "BrandNew@123");
        var userContext = TestUserContextFactory.CreateAdmin();

        A.CallTo(() => _repository.GetByEmailAsync(command.Email, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(aggregate));
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).Returns(Task.FromResult(1));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(aggregate.Id);
        result.Value.Email.ShouldBe("john@tcagro.com");
        result.Value.Message.ShouldBe("Password changed successfully.");
        aggregate.PasswordHash.Verify("BrandNew@123").ShouldBeTrue();

        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    private ChangePasswordCommandHandler CreateHandler(TC.Agro.SharedKernel.Infrastructure.UserClaims.IUserContext userContext)
        => new(_repository, userContext, _outbox, _logger);

    private static UserAggregate CreateAggregate()
    {
        var aggregateResult = UserAggregate.Create(
            name: "John Doe",
            emailValue: "john@tcagro.com",
            username: "john001",
            passwordValue: "Strong@123",
            roleValue: "Producer");

        aggregateResult.IsSuccess.ShouldBeTrue();
        return aggregateResult.Value;
    }
}
