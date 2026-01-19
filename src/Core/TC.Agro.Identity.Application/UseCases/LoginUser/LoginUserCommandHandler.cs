using TC.Agro.SharedKernel.Infrastructure.Authentication;

namespace TC.Agro.Identity.Application.UseCases.LoginUser
{
    internal sealed class LoginUserCommandHandler : BaseHandler<LoginUserCommand, LoginUserResponse>
    {
        private readonly IUserAggregateRepository _repository;
        private readonly ITokenProvider _tokenProvider;

        public LoginUserCommandHandler(IUserAggregateRepository repository, ITokenProvider tokenProvider)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        public override async Task<Result<LoginUserResponse>> ExecuteAsync(LoginUserCommand command, CancellationToken ct = default)
        {
            var userTokenInfo = await _repository
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
