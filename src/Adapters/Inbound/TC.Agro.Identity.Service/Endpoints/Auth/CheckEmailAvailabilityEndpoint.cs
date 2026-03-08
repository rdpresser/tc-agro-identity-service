using TC.Agro.Identity.Application.UseCases.CheckEmailAvailability;

namespace TC.Agro.Identity.Service.Endpoints.Auth
{
    public sealed class CheckEmailAvailabilityEndpoint : BaseApiEndpoint<CheckEmailAvailabilityQuery, CheckEmailAvailabilityResponse>
    {
        public override void Configure()
        {
            Get("check-email/{email}");
            RoutePrefixOverride("auth");

            RequestBinder(new RequestBinder<CheckEmailAvailabilityQuery>(BindingSource.RouteValues));

            this.AddQueryCachingIfNotTesting();
            AllowAnonymous();

            Description(x => x.Produces<CheckEmailAvailabilityResponse>(200)
                .ProducesProblemDetails(400));

            Summary(s =>
            {
                s.Summary = "Check email availability for sign up.";
                s.Description = "This endpoint checks whether an email is available for a new account without requiring authentication.";
                s.ExampleRequest = new CheckEmailAvailabilityQuery { Email = "john.smith@gmail.com" };
                s.ResponseExamples[200] = new CheckEmailAvailabilityResponse
                {
                    Email = "john.smith@gmail.com",
                    IsAvailable = true
                };
                s.Responses[200] = "Returned when availability check is successfully completed.";
                s.Responses[400] = "Returned when the request is invalid.";
            });
        }

        public override async Task HandleAsync(CheckEmailAvailabilityQuery req, CancellationToken ct)
        {
            var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);

            await MatchResultAsync(response, ct).ConfigureAwait(false);
        }
    }
}

