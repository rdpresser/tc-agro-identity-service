namespace TC.Agro.Identity.Application.UseCases.GetUserByEmail
{
    internal sealed class GetUserByEmailQueryHandler : BaseQueryHandler<GetUserByEmailQuery, UserByEmailResponse>
    {
        private readonly IUserReadStore _userReadStore;
        private readonly IUserContext _userContext;

        public GetUserByEmailQueryHandler(IUserReadStore userReadStore, IUserContext userContext)
        {
            _userReadStore = userReadStore ?? throw new ArgumentNullException(nameof(userReadStore));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public override async Task<Result<UserByEmailResponse>> ExecuteAsync(GetUserByEmailQuery command, CancellationToken ct = default)
        {
            if (_userContext.Role == AppConstants.UserRole
                && !_userContext.Email.Equals(command.Email, StringComparison.InvariantCultureIgnoreCase))
            {
                AddError(x => x.Email, "You are not authorized to access this user.", $"{nameof(GetUserByEmailQuery.Email)}.NotAuthorized");
                return BuildNotAuthorizedResult();
            }

            var userResponse = await _userReadStore
                .GetByEmailAsync(command.Email, ct)
                .ConfigureAwait(false);

            if (userResponse is not null)
                return userResponse;

            AddError(x => x.Email, $"User with email '{command.Email}' not found.", UserDomainErrors.NotFound.ErrorCode);
            return BuildNotFoundResult();
        }
    }
}
