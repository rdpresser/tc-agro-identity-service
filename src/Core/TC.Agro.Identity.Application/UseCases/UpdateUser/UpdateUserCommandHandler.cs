namespace TC.Agro.Identity.Application.UseCases.UpdateUser
{
    internal sealed class UpdateUserCommandHandler
        : BaseCommandHandler<UpdateUserCommand, UpdateUserResponse, UserAggregate, IUserAggregateRepository>
    {
        private readonly ILogger<UpdateUserCommandHandler> _logger;

        public UpdateUserCommandHandler(
            IUserAggregateRepository repository,
            IUserContext userContext,
            ITransactionalOutbox outbox,
            ILogger<UpdateUserCommandHandler> logger)
            : base(repository, userContext, outbox, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<Result<UserAggregate>> MapAsync(UpdateUserCommand command, CancellationToken ct)
        {
            var aggregate = await Repository.GetByIdAsync(command.Id, ct).ConfigureAwait(false);
            if (aggregate == null)
            {
                return Result<UserAggregate>.NotFound($"User with ID '{command.Id}' not found.");
            }

            if (!string.Equals(aggregate.Email.Value, command.Email, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await Repository.EmailExistsAsync(command.Email, ct).ConfigureAwait(false);
                if (exists)
                {
                    return Result<UserAggregate>.Invalid(new ValidationError("Email", "Email already exists."));
                }
            }

            var emailResult = Email.Create(command.Email);
            if (!emailResult.IsSuccess)
            {
                return Result<UserAggregate>.Invalid(emailResult.ValidationErrors.ToArray());
            }

            if (UserContext.Id != aggregate.Id && !UserContext.IsAdmin)
            {
                return Result<UserAggregate>.Unauthorized("You do not have permission to update this user.");
            }

            var updateResult = aggregate.UpdateInfo(command.Name, emailResult.Value, command.Username);
            if (!updateResult.IsSuccess)
            {
                return Result<UserAggregate>.Invalid(updateResult.ValidationErrors.ToArray());
            }

            return Result<UserAggregate>.Success(aggregate);
        }

        protected override Task PersistAsync(UserAggregate aggregate, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        protected override async Task PublishIntegrationEventsAsync(UserAggregate aggregate, CancellationToken ct)
        {
            var integrationEvents = aggregate.UncommittedEvents
                .MapToIntegrationEvents(
                    aggregate: aggregate,
                    userContext: UserContext,
                    handlerName: nameof(UpdateUserCommandHandler),
                    mappings: new Dictionary<Type, Func<BaseDomainEvent, UserUpdatedIntegrationEvent>>
                    {
                        { typeof(UserUpdatedDomainEvent), e => UpdateUserMapper.ToIntegrationEvent((UserUpdatedDomainEvent)e) }
                    })
                .ToList();

            if (integrationEvents.Count > 0)
            {
                foreach (var evt in integrationEvents)
                {
                    await Outbox.EnqueueAsync(evt, ct).ConfigureAwait(false);
                }
            }

            _logger.LogInformation(
                "Enqueued {Count} integration events for user {UserId}",
                integrationEvents.Count,
                aggregate.Id);

            _logger.LogInformation(
                "User {UserId} updated successfully by {CurrentUserId}",
                aggregate.Id,
                UserContext.Id);
        }

        protected override Task<UpdateUserResponse> BuildResponseAsync(UserAggregate aggregate, CancellationToken ct)
            => Task.FromResult(UpdateUserMapper.FromAggregate(aggregate));
    }
}
