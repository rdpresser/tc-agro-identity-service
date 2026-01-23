using TC.Agro.Identity.Application.Abstractions.Messaging;

namespace TC.Agro.Identity.Application.UseCases.CreateUser;

internal sealed class CreateUserCommandHandler
    : BaseCommandHandler<CreateUserCommand, CreateUserResponse, UserAggregate, IUserAggregateRepository>
{
    private readonly IIntegrationEventOutbox _outbox;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserAggregateRepository repository,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IIntegrationEventOutbox outbox,
        ILogger<CreateUserCommandHandler> logger)
        : base(repository, userContext, unitOfWork, logger)
    {
        _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override Task<Result<UserAggregate>> MapAsync(CreateUserCommand command, CancellationToken ct)
    {
        var aggregateResult = CreateUserMapper.ToAggregate(command);
        return Task.FromResult(aggregateResult);
    }

    protected override async Task<Result> ValidateAsync(UserAggregate aggregate, CancellationToken ct)
    {
        var exists = await Repository.EmailExistsAsync(aggregate.Email.Value, ct).ConfigureAwait(false);
        if (exists)
            return Result.Invalid(new ValidationError("Email", "Email already registered"));

        return Result.Success();
    }

    protected override async Task PublishIntegrationEventsAsync(UserAggregate aggregate, CancellationToken ct)
    {
        var integrationEvents = aggregate.UncommittedEvents
            .MapToIntegrationEvents(
                aggregate: aggregate,
                userContext: UserContext,
                handlerName: nameof(CreateUserCommandHandler),
                mappings: new Dictionary<Type, Func<BaseDomainEvent, UserCreatedIntegrationEvent>>
                {
                    { typeof(UserCreatedDomainEvent), e => CreateUserMapper.ToIntegrationEvent((UserCreatedDomainEvent)e) }
                });

        foreach (var evt in integrationEvents)
        {
            await _outbox.EnqueueAsync(evt, ct).ConfigureAwait(false);
        }

        _logger.LogInformation(
            "Enqueued {Count} integration events for user {UserId}",
            integrationEvents.Count(),
            aggregate.Id);
    }

    protected override Task<CreateUserResponse> BuildResponseAsync(UserAggregate aggregate, CancellationToken ct)
        => Task.FromResult(CreateUserMapper.FromAggregate(aggregate));
}
