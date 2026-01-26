namespace TC.Agro.Identity.Application.UseCases.GetUserList
{
    public sealed record GetUserListQuery : ICachedQuery<IReadOnlyList<UserListResponse>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string SortBy { get; init; } = "id";
        public string SortDirection { get; init; } = "asc";
        public string Filter { get; init; } = "";

        private string? _cacheKey;
        public string GetCacheKey
        {
            get => _cacheKey ?? $"GetUserListQuery-{PageNumber}-{PageSize}-{SortBy}-{SortDirection}-{Filter}";
        }

        public TimeSpan? Duration => null;
        public TimeSpan? DistributedCacheDuration => null;

        public void SetCacheKey(string cacheKey)
        {
            _cacheKey = $"GetUserListQuery-{PageNumber}-{PageSize}-{SortBy}-{SortDirection}-{Filter}-{cacheKey}";
        }
    }
}
