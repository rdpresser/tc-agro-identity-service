namespace TC.Agro.Identity.Service.Endpoints.User
{
    public sealed class DeleteUserEndpoint : BaseApiEndpoint<DeactivateUserCommand, DeactivateUserResponse>
    {
        public override void Configure()
        {
            Delete("user/{id}");
            Roles(AppConstants.AdminRole);
            PostProcessor<LoggingCommandPostProcessorBehavior<DeactivateUserCommand, DeactivateUserResponse>>();
            this.AddCacheInvalidationIfNotTesting();
            // 🔥 Force FastEndpoints to bind from route params (not JSON body)
            RequestBinder(new RequestBinder<DeactivateUserCommand>(BindingSource.RouteValues));

            // Requires authentication - Only authenticated users can deactivate users
            Description(
                x => x.Produces<DeactivateUserResponse>(200)
                      .ProducesProblemDetails(400)
                      .ProducesProblemDetails(404));

            Summary(s =>
            {
                s.Summary = "Endpoint for deactivating (soft delete) a user.";
                s.Description = "This endpoint allows for the deactivation of an existing user by their ID. The user is not permanently deleted but marked as inactive. This is a soft delete operation to maintain data integrity and audit trails.";
                s.ExampleRequest = new DeactivateUserCommand(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
                s.ResponseExamples[200] = new DeactivateUserResponse(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "User deactivated successfully.");
                s.Responses[200] = "Returned when the user is successfully deactivated.";
                s.Responses[400] = "Returned when the request is invalid or the user is already inactive.";
                s.Responses[404] = "Returned when the user with the specified ID is not found.";
            });
        }

        public override async Task HandleAsync(DeactivateUserCommand req, CancellationToken ct)
        {
            var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);

            await MatchResultAsync(response, ct).ConfigureAwait(false);
        }
    }
}

