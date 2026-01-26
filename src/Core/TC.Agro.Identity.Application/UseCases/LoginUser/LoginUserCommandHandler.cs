namespace TC.Agro.Identity.Application.UseCases.LoginUser
{
    internal sealed class LoginUserCommandHandler : BaseHandler<LoginUserCommand, LoginUserResponse>
    {
        private readonly IUserReadStore _readStore;
        private readonly ITokenProvider _tokenProvider;

        public LoginUserCommandHandler(IUserReadStore readStore, ITokenProvider tokenProvider)
        {
            _readStore = readStore ?? throw new ArgumentNullException(nameof(readStore));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        public override async Task<Result<LoginUserResponse>> ExecuteAsync(LoginUserCommand command, CancellationToken ct = default)
        {
            var userTokenInfo = await _readStore
                .GetUserTokenInfoAsync(command.Email, command.Password, ct)
                .ConfigureAwait(false);

            if (userTokenInfo is null)
            {
                AddError(UserDomainErrors.InvalidCredentials.Property,
                         UserDomainErrors.InvalidCredentials.ErrorMessage,
                         UserDomainErrors.InvalidCredentials.ErrorCode);

                // Returns a NotAuthorized result using the shared validation helper
                return BuildNotAuthorizedResult();
            }

            var response = new LoginUserResponse(
                JwtToken: _tokenProvider.Create(userTokenInfo),
                Email: userTokenInfo.Email
            );

            return Result<LoginUserResponse>.Success(response);
        }
    }
}
