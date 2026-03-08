using Ardalis.Result;
using Microsoft.Extensions.Logging;
using TC.Agro.Contracts.Events.Identity;
using TC.Agro.Identity.Application.Abstractions.Ports;
using TC.Agro.Identity.Application.UseCases.UpdateUser;
using TC.Agro.Identity.Domain.Aggregates;
using TC.Agro.Identity.Tests.TestHelpers;
using TC.Agro.SharedKernel.Application.Ports;
using TC.Agro.SharedKernel.Infrastructure.Messaging;
using TC.Agro.SharedKernel.Infrastructure.UserClaims;

namespace TC.Agro.Identity.Tests.Application.UseCases.UpdateUser;

public sealed class UpdateUserCommandHandlerTests
{
    private readonly IUserAggregateRepository _repository = A.Fake<IUserAggregateRepository>();
    private readonly ITransactionalOutbox _outbox = A.Fake<ITransactionalOutbox>();
    private readonly ILogger<UpdateUserCommandHandler> _logger = A.Fake<ILogger<UpdateUserCommandHandler>>();

    public UpdateUserCommandHandlerTests()
    {
        FastEndpointsTestBootstrap.EnsureInitialized();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIsNotFound_ShouldReturnNotFound()
    {
        var command = new UpdateUserCommand(Guid.NewGuid(), "John", "john@tcagro.com", "john001");
        var userContext = TestUserContextFactory.CreateAdmin();

        A.CallTo(() => _repository.GetByIdAsync(command.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(null));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCallerIsNotOwnerAndNotAdmin_ShouldReturnUnauthorized()
    {
        var aggregate = CreateAggregate(email: "john@tcagro.com");
        var command = new UpdateUserCommand(aggregate.Id, "John Updated", "john@tcagro.com", "johnupdated");
        var userContext = TestUserContextFactory.CreateUser(Guid.NewGuid(), "other@tcagro.com");

        A.CallTo(() => _repository.GetByIdAsync(command.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(aggregate));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Unauthorized);
        A.CallTo(() => _outbox.EnqueueAsync(A<EventContext<UserUpdatedIntegrationEvent>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailIsAlreadyTaken_ShouldReturnInvalid()
    {
        var aggregate = CreateAggregate(email: "john@tcagro.com");
        var command = new UpdateUserCommand(aggregate.Id, "John Updated", "taken@tcagro.com", "johnupdated");
        var userContext = TestUserContextFactory.CreateAdmin();

        A.CallTo(() => _repository.GetByIdAsync(command.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(aggregate));
        A.CallTo(() => _repository.EmailExistsAsync(command.Email, A<CancellationToken>._))
            .Returns(Task.FromResult(true));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(error => error.ErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase));

        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDataIsValid_ShouldUpdateAndPublishEvent()
    {
        var aggregate = CreateAggregate(email: "john@tcagro.com");
        var command = new UpdateUserCommand(aggregate.Id, "John Updated", "john.updated@tcagro.com", "johnupdated");
        var userContext = TestUserContextFactory.CreateAdmin();
        EventContext<UserUpdatedIntegrationEvent>? publishedEvent = null;

        A.CallTo(() => _repository.GetByIdAsync(command.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<UserAggregate?>(aggregate));
        A.CallTo(() => _repository.EmailExistsAsync(command.Email, A<CancellationToken>._))
            .Returns(Task.FromResult(false));
        A.CallTo(() => _outbox.EnqueueAsync(A<EventContext<UserUpdatedIntegrationEvent>>._, A<CancellationToken>._))
            .Invokes(call => publishedEvent = call.GetArgument<EventContext<UserUpdatedIntegrationEvent>>(0));
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).Returns(Task.FromResult(1));

        var sut = CreateHandler(userContext);

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(aggregate.Id);
        result.Value.Email.ShouldBe("john.updated@tcagro.com");
        result.Value.Name.ShouldBe("John Updated");
        result.Value.Username.ShouldBe("johnupdated");

        aggregate.Email.Value.ShouldBe("john.updated@tcagro.com");
        publishedEvent.ShouldNotBeNull();
        publishedEvent!.EventData.OwnerId.ShouldBe(aggregate.Id);
        publishedEvent.EventData.Email.ShouldBe("john.updated@tcagro.com");

        A.CallTo(() => _outbox.EnqueueAsync(A<EventContext<UserUpdatedIntegrationEvent>>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    private UpdateUserCommandHandler CreateHandler(IUserContext userContext)
        => new(_repository, userContext, _outbox, _logger);

    private static UserAggregate CreateAggregate(string email)
    {
        var aggregateResult = UserAggregate.Create(
            name: "John Doe",
            emailValue: email,
            username: "john001",
            passwordValue: "Strong@123",
            roleValue: "Producer");

        aggregateResult.IsSuccess.ShouldBeTrue();
        return aggregateResult.Value;
    }
}
