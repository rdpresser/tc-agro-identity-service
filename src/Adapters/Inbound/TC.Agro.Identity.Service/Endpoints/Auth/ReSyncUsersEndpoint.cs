using TC.Agro.Identity.Application.UseCases.ReSyncUsers;

namespace TC.Agro.Identity.Service.Endpoints.Auth
{
    public sealed class ReSyncUsersEndpoint : BaseApiEndpointWithoutRequest<ReSyncUsersResponse>
    {
        private readonly IReSyncUsersUseCase _reSyncUsersUseCase;

        public ReSyncUsersEndpoint(IReSyncUsersUseCase reSyncUsersUseCase)
        {
            _reSyncUsersUseCase = reSyncUsersUseCase;
        }

        public override void Configure()
        {
            Post("resync/users");
            RoutePrefixOverride("auth");
            Roles(AppConstants.AdminRole);

            Description(x => x.Produces<ReSyncUsersResponse>(200)
                .ProducesProblemDetails(400)
                .Produces((int)HttpStatusCode.Forbidden)
                .Produces((int)HttpStatusCode.Unauthorized));

            Summary(s =>
            {
                s.Summary = "Re-sync all active users to RabbitMQ.";
                s.Description = "Admin-only endpoint that reloads all active users from database and republishes them as UserCreated integration events to the identity exchange.";
                s.ResponseExamples[200] = new ReSyncUsersResponse(
                    TotalActiveUsers: 120,
                    RepublishedUsers: 120,
                    ExecutedAtUtc: DateTimeOffset.UtcNow);
                s.Responses[200] = "Returned when active users are successfully republished to the message broker.";
                s.Responses[403] = "Returned when the caller is not an administrator.";
                s.Responses[401] = "Returned when the request is unauthenticated.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var response = await _reSyncUsersUseCase
                .ExecuteAsync(ct)
                .ConfigureAwait(false);

            await MatchResultAsync(response, ct).ConfigureAwait(false);
        }
    }
}
