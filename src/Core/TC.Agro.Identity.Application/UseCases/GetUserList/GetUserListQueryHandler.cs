namespace TC.Agro.Identity.Application.UseCases.GetUserList
{
    internal sealed class GetUserListQueryHandler : BaseQueryHandler<GetUserListQuery, IReadOnlyList<UserListResponse>>
    {
        private readonly IUserAggregateRepository _userRepository;

        public GetUserListQueryHandler(IUserAggregateRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public override async Task<Result<IReadOnlyList<UserListResponse>>> ExecuteAsync(GetUserListQuery query,
            CancellationToken ct = default)
        {
            var users = await _userRepository.GetUserListAsync(query, ct).ConfigureAwait(false);

            if (users is null || !users.Any())
                return Result<IReadOnlyList<UserListResponse>>.Success([]);

            return Result.Success<IReadOnlyList<UserListResponse>>([.. users]);
        }
    }
}
