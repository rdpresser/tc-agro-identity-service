namespace TC.Agro.Identity.Application.UseCases.GetUserList
{
    internal sealed class GetUserListQueryHandler : BaseQueryHandler<GetUserListQuery, IReadOnlyList<UserListResponse>>
    {
        private readonly IUserReadStore _userReadStore;

        public GetUserListQueryHandler(IUserReadStore userReadStore)
        {
            _userReadStore = userReadStore ?? throw new ArgumentNullException(nameof(userReadStore));
        }

        public override async Task<Result<IReadOnlyList<UserListResponse>>> ExecuteAsync(GetUserListQuery query,
            CancellationToken ct = default)
        {
            var users = await _userReadStore.GetUserListAsync(query, ct).ConfigureAwait(false);

            if (users is null || !users.Any())
                return Result<IReadOnlyList<UserListResponse>>.Success([]);

            return Result.Success<IReadOnlyList<UserListResponse>>([.. users]);
        }
    }
}
