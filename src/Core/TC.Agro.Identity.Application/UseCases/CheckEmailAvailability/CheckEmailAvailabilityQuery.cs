using CacheTagCatalog = TC.Agro.Identity.Application.Abstractions.CacheTags;

namespace TC.Agro.Identity.Application.UseCases.CheckEmailAvailability
{
    public sealed record CheckEmailAvailabilityQuery : ICachedQuery<CheckEmailAvailabilityResponse>
    {
        public string Email { get; init; } = default!;

        private string? _cacheKey;
        public string GetCacheKey => _cacheKey ?? $"CheckEmailAvailabilityQuery-{Email}";
        public TimeSpan? Duration => null;
        public TimeSpan? DistributedCacheDuration => null;

        public IReadOnlyCollection<string> CacheTags =>
        [
            CacheTagCatalog.Users,
            CacheTagCatalog.UserByEmail
        ];

        public void SetCacheKey(string cacheKey)
            => _cacheKey = $"CheckEmailAvailabilityQuery-{Email}-{cacheKey}";
    }
}
