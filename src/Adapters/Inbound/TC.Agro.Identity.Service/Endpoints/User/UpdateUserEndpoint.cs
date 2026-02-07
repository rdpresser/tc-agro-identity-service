namespace TC.Agro.Identity.Service.Endpoints.User
{
    public sealed class UpdateUserEndpoint : BaseApiEndpoint<UpdateUserCommand, UpdateUserResponse>
    {
        public override void Configure()
        {
            Put("user/{id:guid}");
            Roles(AppConstants.AdminRole);
            PostProcessor<LoggingCommandPostProcessorBehavior<UpdateUserCommand, UpdateUserResponse>>();
            PostProcessor<CacheInvalidationPostProcessorBehavior<UpdateUserCommand, UpdateUserResponse>>();

            Description(x => x.Produces<UpdateUserResponse>(200)
                .ProducesProblemDetails(400)
                .ProducesProblemDetails(404)
                .Produces((int)HttpStatusCode.Unauthorized)
                .Produces((int)HttpStatusCode.Forbidden));

            Summary(s =>
            {
                s.Summary = "Endpoint for updating user details.";
                s.Description = "Updates user name, email, and username. This is a soft update on the existing user.";
                s.ExampleRequest = new UpdateUserCommand(
                    Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    "John Smith",
                    "john.smith@gmail.com",
                    "johnsmith");
                s.ResponseExamples[200] = new UpdateUserResponse(
                    Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    "John Smith",
                    "john.smith@gmail.com",
                    "johnsmith",
                    "Admin");
                s.Responses[200] = "Returned when the user is successfully updated.";
                s.Responses[400] = "Returned when the request is invalid.";
                s.Responses[404] = "Returned when the user with the specified ID is not found.";
            });
        }

        public override async Task HandleAsync(UpdateUserCommand req, CancellationToken ct)
        {
            var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);
            await MatchResultAsync(response, ct).ConfigureAwait(false);
        }
    }
}
