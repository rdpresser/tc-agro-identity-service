using Ardalis.Result;
using Microsoft.Extensions.Logging;
using TC.Agro.Contracts.Events.Identity;
using TC.Agro.Identity.Application.Abstractions.Ports;
using TC.Agro.Identity.Application.UseCases.DeactivateUser;
using TC.Agro.Identity.Domain.Aggregates;
using TC.Agro.Identity.Tests.TestHelpers;
using TC.Agro.SharedKernel.Application.Ports;
using TC.Agro.SharedKernel.Infrastructure.Messaging;
using TC.Agro.SharedKernel.Infrastructure.UserClaims;

namespace TC.Agro.Identity.Tests.Application.UseCases.DeactivateUser;

public sealed class DeactivateUserCommandHandlerTests
{
    private readonly IUserAggregateRepository _repository = A.Fake<IUserAggregateRepository>();
    private readonly ITransactionalOutbox _outbox = A.Fake<ITransactionalOutbox>();
    private readonly ILogger<DeactivateUserCommandHandler> _logger = A.Fake<ILogger<DeactivateUserCommandHandler>>();

    public DeactivateUserCommandHandlerTests()
    {
        FastEndpointsTestBootstrap.EnsureInitialized();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIsNotFound_ShouldReturnNotFound()
    {
        var command = new DeactivateUserCommand(Guid.NewGuid());
        var userContext = TestUserContextFactory.CreateAdmin();

        A.CallTo(() => _repository.GetByIdAsync(command.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(null));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserTriesToDeactivateSelf_ShouldReturnInvalid()
    {
        var aggregate = CreateAggregate();
        var command = new DeactivateUserCommand(aggregate.Id);
        var userContext = TestUserContextFactory.CreateUser(aggregate.Id, "self@tcagro.com");

        A.CallTo(() => _repository.GetByIdAsync(command.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(aggregate));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(error => error.Identifier == "User.SelfDeactivation");

        A.CallTo(() => _outbox.EnqueueAsync(A<EventContext<UserDeactivatedIntegrationEvent>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetUserIsAlreadyInactive_ShouldReturnInvalid()
    {
        var aggregate = CreateAggregate();
        aggregate.Deactivate().IsSuccess.ShouldBeTrue();

        var command = new DeactivateUserCommand(aggregate.Id);
        var userContext = TestUserContextFactory.CreateAdmin();

        A.CallTo(() => _repository.GetByIdAsync(command.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(aggregate));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(error => error.Identifier == "User.AlreadyInactive");
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDataIsValid_ShouldDeactivateAndPublishEvent()
    {
        var aggregate = CreateAggregate();
        var command = new DeactivateUserCommand(aggregate.Id);
        var userContext = TestUserContextFactory.CreateAdmin();
        EventContext<UserDeactivatedIntegrationEvent>? publishedEvent = null;

        A.CallTo(() => _repository.GetByIdAsync(command.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(aggregate));
        A.CallTo(() => _outbox.EnqueueAsync(A<EventContext<UserDeactivatedIntegrationEvent>>._, A<CancellationToken>._))
            .Invokes(call => publishedEvent = call.GetArgument<EventContext<UserDeactivatedIntegrationEvent>>(0));
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).Returns(Task.FromResult(1));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(aggregate.Id);
        result.Value.Message.ShouldBe("User deactivated successfully.");
        aggregate.IsActive.ShouldBeFalse();

        publishedEvent.ShouldNotBeNull();
        publishedEvent!.EventData.OwnerId.ShouldBe(aggregate.Id);

        A.CallTo(() => _outbox.EnqueueAsync(A<EventContext<UserDeactivatedIntegrationEvent>>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    private DeactivateUserCommandHandler CreateHandler(IUserContext userContext)
        => new(_repository, userContext, _outbox, _logger);

    private static UserAggregate CreateAggregate()
    {
        var aggregateResult = UserAggregate.Create(
            name: "John Doe",
            emailValue: "john.doe@tcagro.com",
            username: "john001",
            passwordValue: "Strong@123",
            roleValue: "Producer");

        aggregateResult.IsSuccess.ShouldBeTrue();
        return aggregateResult.Value;
    }
}
