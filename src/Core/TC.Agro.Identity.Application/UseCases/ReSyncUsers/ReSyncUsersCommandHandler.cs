using Wolverine;

namespace TC.Agro.Identity.Application.UseCases.ReSyncUsers
{
    public sealed class ReSyncUsersCommandHandler : IReSyncUsersUseCase
    {
        private readonly IUserReadStore _userReadStore;
        private readonly IUserContext _userContext;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<ReSyncUsersCommandHandler> _logger;

        public ReSyncUsersCommandHandler(
            IUserReadStore userReadStore,
            IUserContext userContext,
            IMessageBus messageBus,
            ILogger<ReSyncUsersCommandHandler> logger)
        {
            _userReadStore = userReadStore ?? throw new ArgumentNullException(nameof(userReadStore));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<ReSyncUsersResponse>> ExecuteAsync(CancellationToken ct = default)
        {
            if (!_userContext.IsAdmin)
                return Result<ReSyncUsersResponse>.Unauthorized("Only administrators can execute user re-sync.");

            var activeUsers = await _userReadStore
                .GetActiveUsersForReSyncAsync(ct)
                .ConfigureAwait(false);

            if (activeUsers.Count == 0)
            {
                _logger.LogInformation("User re-sync requested by {UserId}, but no active users were found.", _userContext.Id);

                return Result.Success(new ReSyncUsersResponse(
                    TotalActiveUsers: 0,
                    RepublishedUsers: 0,
                    ExecutedAtUtc: DateTimeOffset.UtcNow));
            }

            var occurredOn = DateTimeOffset.UtcNow;
            var events = activeUsers
                .Select(user => EventContext<UserCreatedIntegrationEvent>.Create<UserAggregate>(
                    data: new UserCreatedIntegrationEvent(
                        OwnerId: user.Id,
                        Name: user.Name,
                        Email: user.Email,
                        Username: user.Username,
                        Role: user.Role,
                        OccurredOn: occurredOn),
                    aggregateId: user.Id,
                    userId: _userContext.Id.ToString(),
                    isAuthenticated: _userContext.IsAuthenticated,
                    correlationId: _userContext.CorrelationId,
                    source: $"Identity.Service.{nameof(ReSyncUsersCommandHandler)}.{nameof(UserCreatedIntegrationEvent)}"))
                .ToArray();

            foreach (var integrationEvent in events)
            {
                ct.ThrowIfCancellationRequested();
                await _messageBus.PublishAsync(integrationEvent).ConfigureAwait(false);
            }

            _logger.LogInformation(
                "User re-sync completed by {UserId}. Republished {PublishedCount} active users.",
                _userContext.Id,
                events.Length);

            return Result.Success(new ReSyncUsersResponse(
                TotalActiveUsers: activeUsers.Count,
                RepublishedUsers: events.Length,
                ExecutedAtUtc: DateTimeOffset.UtcNow));
        }
    }
}
