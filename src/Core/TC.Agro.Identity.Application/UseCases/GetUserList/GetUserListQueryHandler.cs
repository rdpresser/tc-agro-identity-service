namespace TC.Agro.Identity.Application.UseCases.GetUserList
{
    internal sealed class GetUserListQueryHandler : BaseQueryHandler<GetUserListQuery, UserListResponse<UserResponse>>
    {
        private readonly IUserReadStore _userReadStore;

        public GetUserListQueryHandler(IUserReadStore userReadStore)
        {
            _userReadStore = userReadStore ?? throw new ArgumentNullException(nameof(userReadStore));
        }

        public override async Task<Result<UserListResponse<UserResponse>>> ExecuteAsync(GetUserListQuery query,
            CancellationToken ct = default)
        {
            var (users, totalCount) = await _userReadStore.GetUserListAsync(query, ct).ConfigureAwait(false);

            if (users is null || !users.Any())
                return Result<UserListResponse<UserResponse>>.Success(
                    new UserListResponse<UserResponse>([], 0, query.PageNumber, query.PageSize));

            var response = new UserListResponse<UserResponse>(
                data: [.. users],
                totalCount: totalCount,
                pageNumber: query.PageNumber,
                pageSize: query.PageSize
            );

            return Result.Success(response);
        }
    }
}
