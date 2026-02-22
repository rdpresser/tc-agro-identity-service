namespace TC.Agro.Identity.Application.UseCases.CheckEmailAvailability
{
    internal sealed class CheckEmailAvailabilityQueryHandler : BaseQueryHandler<CheckEmailAvailabilityQuery, CheckEmailAvailabilityResponse>
    {
        private readonly IUserReadStore _userReadStore;

        public CheckEmailAvailabilityQueryHandler(IUserReadStore userReadStore)
        {
            _userReadStore = userReadStore ?? throw new ArgumentNullException(nameof(userReadStore));
        }

        public override async Task<Result<CheckEmailAvailabilityResponse>> ExecuteAsync(CheckEmailAvailabilityQuery query, CancellationToken ct = default)
        {
            var isAvailable = await _userReadStore
                .IsEmailAvailableAsync(query.Email, ct)
                .ConfigureAwait(false);

            return Result.Success(new CheckEmailAvailabilityResponse
            {
                Email = query.Email,
                IsAvailable = isAvailable
            });
        }
    }
}
