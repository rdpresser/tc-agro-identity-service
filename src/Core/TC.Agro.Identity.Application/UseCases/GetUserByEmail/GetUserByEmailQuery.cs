namespace TC.Agro.Identity.Application.UseCases.GetUserByEmail
{
    public sealed record GetUserByEmailQuery : ICachedQuery<UserByEmailResponse>
    {
        public string Email { get; init; } = default!;

        private string? _cacheKey;
        public string GetCacheKey => _cacheKey ?? $"GetUserByEmailQuery-{Email}";
        public TimeSpan? Duration => null;
        public TimeSpan? DistributedCacheDuration => null;

        public void SetCacheKey(string cacheKey)
            => _cacheKey = $"GetUserByEmailQuery-{Email}-{cacheKey}";
    }
}
