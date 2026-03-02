namespace TC.Agro.Identity.Application.UseCases.DeactivateUser
{
    internal sealed class DeactivateUserCommandHandler
        : BaseCommandHandler<DeactivateUserCommand, DeactivateUserResponse, UserAggregate, IUserAggregateRepository>
    {
        private readonly ILogger<DeactivateUserCommandHandler> _logger;

        public DeactivateUserCommandHandler(
            IUserAggregateRepository repository,
            IUserContext userContext,
            ITransactionalOutbox outbox,
            ILogger<DeactivateUserCommandHandler> logger)
            : base(repository, userContext, outbox, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<Result<UserAggregate>> MapAsync(DeactivateUserCommand command, CancellationToken ct)
        {
            var aggregate = await Repository.GetByIdAsync(command.Id, ct).ConfigureAwait(false);
            if (aggregate == null)
            {
                return Result<UserAggregate>.NotFound($"User with ID '{command.Id}' not found.");
            }

            var deactivateResult = aggregate.Deactivate();
            if (!deactivateResult.IsSuccess)
            {
                return Result<UserAggregate>.Invalid(deactivateResult.ValidationErrors.ToArray());
            }

            return Result<UserAggregate>.Success(aggregate);
        }

        protected override Task<Result> ValidateAsync(UserAggregate aggregate, CancellationToken ct)
        {
            // Prevent users from deactivating themselves
            if (aggregate.Id == UserContext.Id)
            {
                var error = UserDomainErrors.SelfDeactivation;
                return Task.FromResult(
                    Result.Invalid(
                        new ValidationError(error.ErrorCode, error.ErrorMessage)));
            }

            // Additional business validations can be added here if needed
            return Task.FromResult(Result.Success());
        }

        protected override Task PersistAsync(UserAggregate aggregate, CancellationToken ct)
        {
            // Since the aggregate is already tracked by the repository, we just need to call SaveChangesAsync to persist the changes.
            return Task.CompletedTask;
        }

        protected override async Task PublishIntegrationEventsAsync(UserAggregate aggregate, CancellationToken ct)
        {
            var integrationEvents = aggregate.UncommittedEvents
                .MapToIntegrationEvents(
                    aggregate: aggregate,
                    userContext: UserContext,
                    handlerName: nameof(DeactivateUserCommandHandler),
                    mappings: new Dictionary<Type, Func<BaseDomainEvent, UserDeactivatedIntegrationEvent>>
                    {
                        { typeof(UserDeactivatedDomainEvent), e => DeactivateUserMapper.ToIntegrationEvent((UserDeactivatedDomainEvent)e, aggregate) }
                    })
                .ToList();

            if (integrationEvents.Count > 0)
            {
                await Outbox.EnqueueAsync(integrationEvents, ct).ConfigureAwait(false);
            }

            _logger.LogInformation(
                "Enqueued {Count} integration events for user {UserId}",
                integrationEvents.Count,
                aggregate.Id);

            _logger.LogInformation(
                "User {UserId} deactivated successfully by {CurrentUserId}",
                aggregate.Id,
                UserContext.Id);
        }

        protected override Task<DeactivateUserResponse> BuildResponseAsync(UserAggregate aggregate, CancellationToken ct)
        {
            var response = new DeactivateUserResponse(
                aggregate.Id,
                "User deactivated successfully.");

            return Task.FromResult(response);
        }
    }
}
