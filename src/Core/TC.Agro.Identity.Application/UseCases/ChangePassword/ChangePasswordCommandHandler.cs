namespace TC.Agro.Identity.Application.UseCases.ChangePassword
{
    internal sealed class ChangePasswordCommandHandler
        : BaseCommandHandler<ChangePasswordCommand, ChangePasswordResponse, UserAggregate, IUserAggregateRepository>
    {
        private readonly ILogger<ChangePasswordCommandHandler> _logger;

        public ChangePasswordCommandHandler(
            IUserAggregateRepository repository,
            IUserContext userContext,
            ITransactionalOutbox outbox,
            ILogger<ChangePasswordCommandHandler> logger)
            : base(repository, userContext, outbox, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<Result<UserAggregate>> MapAsync(ChangePasswordCommand command, CancellationToken ct)
        {
            var aggregate = await Repository.GetByEmailAsync(command.Email, ct).ConfigureAwait(false);
            if (aggregate == null)
            {
                return Result<UserAggregate>.NotFound($"User with email '{command.Email}' not found.");
            }

            if (!aggregate.IsActive)
            {
                return Result<UserAggregate>.Invalid(new ValidationError("User.Inactive", "Inactive users cannot change password."));
            }

            var changePasswordResult = aggregate.ChangePassword(command.Password);
            if (!changePasswordResult.IsSuccess)
            {
                return Result<UserAggregate>.Invalid(changePasswordResult.ValidationErrors.ToArray());
            }

            return Result<UserAggregate>.Success(aggregate);
        }

        protected override Task PersistAsync(UserAggregate aggregate, CancellationToken ct)
            => Task.CompletedTask;

        protected override Task PublishIntegrationEventsAsync(UserAggregate aggregate, CancellationToken ct)
        {
            _logger.LogInformation("Password changed for user {UserId} ({Email})", aggregate.Id, aggregate.Email.Value);
            return Task.CompletedTask;
        }

        protected override Task<ChangePasswordResponse> BuildResponseAsync(UserAggregate aggregate, CancellationToken ct)
            => Task.FromResult(ChangePasswordMapper.FromAggregate(aggregate));
    }
}
