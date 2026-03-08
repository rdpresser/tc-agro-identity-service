using Ardalis.Result;
using Microsoft.Extensions.Logging;
using TC.Agro.Contracts.Events.Identity;
using TC.Agro.Identity.Application.Abstractions.Ports;
using TC.Agro.Identity.Application.UseCases.CreateUser;
using TC.Agro.Identity.Domain.Aggregates;
using TC.Agro.Identity.Tests.TestHelpers;
using TC.Agro.SharedKernel.Application.Ports;
using TC.Agro.SharedKernel.Infrastructure.Messaging;
using TC.Agro.SharedKernel.Infrastructure.UserClaims;

namespace TC.Agro.Identity.Tests.Application.UseCases.CreateUser;

public sealed class CreateUserCommandHandlerTests
{
    private readonly IUserAggregateRepository _repository = A.Fake<IUserAggregateRepository>();
    private readonly IUserContext _userContext = TestUserContextFactory.CreateAdmin();
    private readonly ITransactionalOutbox _outbox = A.Fake<ITransactionalOutbox>();
    private readonly ILogger<CreateUserCommandHandler> _logger = A.Fake<ILogger<CreateUserCommandHandler>>();

    public CreateUserCommandHandlerTests()
    {
        FastEndpointsTestBootstrap.EnsureInitialized();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandIsValid_ShouldPersistAndPublishIntegrationEvent()
    {
        var command = new CreateUserCommand(
            Name: "John Smith",
            Email: "john.smith@tcagro.com",
            Username: "johnsmith",
            Password: "Strong@123",
            Role: "Producer");

        UserAggregate? addedAggregate = null;
        EventContext<UserCreatedIntegrationEvent>? publishedEvent = null;

        A.CallTo(() => _repository.EmailExistsAsync(command.Email, A<CancellationToken>._))
            .Returns(false);
        A.CallTo(() => _repository.Add(A<UserAggregate>._))
            .Invokes(call => addedAggregate = call.GetArgument<UserAggregate>(0));
        A.CallTo(() => _outbox.EnqueueAsync(A<EventContext<UserCreatedIntegrationEvent>>._, A<CancellationToken>._))
            .Invokes(call => publishedEvent = call.GetArgument<EventContext<UserCreatedIntegrationEvent>>(0));
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._))
            .Returns(Task.FromResult(1));

        var sut = CreateHandler();

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe("john.smith@tcagro.com");
        result.Value.Name.ShouldBe(command.Name);
        result.Value.Username.ShouldBe(command.Username);
        result.Value.Role.ShouldBe(command.Role);

        addedAggregate.ShouldNotBeNull();
        addedAggregate!.Email.Value.ShouldBe("john.smith@tcagro.com");

        publishedEvent.ShouldNotBeNull();
        publishedEvent!.EventData.OwnerId.ShouldBe(result.Value.Id);
        publishedEvent.EventData.Email.ShouldBe("john.smith@tcagro.com");
        publishedEvent.EventData.Role.ShouldBe(command.Role);
        publishedEvent.UserId.ShouldBe(_userContext.Id.ToString());

        A.CallTo(() => _repository.Add(A<UserAggregate>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _outbox.EnqueueAsync(A<EventContext<UserCreatedIntegrationEvent>>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailAlreadyExists_ShouldReturnInvalidAndSkipPersistence()
    {
        var command = new CreateUserCommand(
            Name: "John Smith",
            Email: "existing@tcagro.com",
            Username: "johnsmith",
            Password: "Strong@123",
            Role: "Producer");

        A.CallTo(() => _repository.EmailExistsAsync(command.Email, A<CancellationToken>._))
            .Returns(true);

        var sut = CreateHandler();

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(error => error.ErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase));

        A.CallTo(() => _repository.Add(A<UserAggregate>._)).MustNotHaveHappened();
        A.CallTo(() => _outbox.EnqueueAsync(A<EventContext<UserCreatedIntegrationEvent>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandHasInvalidPayload_ShouldReturnInvalidWithoutCallingRepository()
    {
        var command = new CreateUserCommand(
            Name: "John Smith",
            Email: "john.smith@tcagro.com",
            Username: "johnsmith",
            Password: "weak",
            Role: "Producer");

        var sut = CreateHandler();

        var result = await sut.ExecuteAsync(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(error => error.Identifier == "Password.TooShort");

        A.CallTo(() => _repository.EmailExistsAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _repository.Add(A<UserAggregate>._)).MustNotHaveHappened();
        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    private CreateUserCommandHandler CreateHandler()
        => new(_repository, _userContext, _outbox, _logger);
}
