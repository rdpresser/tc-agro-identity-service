using TC.Agro.Identity.Application.UseCases.ChangePassword;

namespace TC.Agro.Identity.Service.Endpoints.Auth
{
    public sealed class ChangePasswordEndpoint : BaseApiEndpoint<ChangePasswordCommand, ChangePasswordResponse>
    {
        public override void Configure()
        {
            Post("change-password");
            RoutePrefixOverride("auth");
            PostProcessor<LoggingCommandPostProcessorBehavior<ChangePasswordCommand, ChangePasswordResponse>>();
            this.AddCacheInvalidationIfNotTesting();
            AllowAnonymous();
            Description(
                x => x.Produces<ChangePasswordResponse>(200)
                      .ProducesProblemDetails(400)
                      .ProducesProblemDetails(404));

            Summary(s =>
            {
                s.Summary = "Endpoint for changing user password.";
                s.Description = "This endpoint changes a user's password by email and validates password strength rules before persisting.";
                s.ExampleRequest = new ChangePasswordCommand("john.smith@gmail.com", "NewPassword@123");
                s.ResponseExamples[200] = new ChangePasswordResponse(Guid.NewGuid(), "john.smith@gmail.com", "Password changed successfully.");
                s.Responses[200] = "Returned when the password is successfully changed.";
                s.Responses[400] = "Returned when the request is invalid.";
                s.Responses[404] = "Returned when no user is found for the provided email.";
            });
        }

        public override async Task HandleAsync(ChangePasswordCommand req, CancellationToken ct)
        {
            var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);
            await MatchResultAsync(response, ct).ConfigureAwait(false);
        }
    }
}

