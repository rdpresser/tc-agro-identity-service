using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using TC.Agro.SharedKernel.Infrastructure.Caching.Service;

namespace TC.Agro.Identity.Tests.TestHelpers;

internal static class FastEndpointsTestBootstrap
{
    private static readonly object SyncRoot = new();

    public static void EnsureInitialized()
    {
        lock (SyncRoot)
        {
            // Reinitialize FastEndpoints unit-test services on every call so
            // tests remain isolated even if a previous test disposed the provider.
            InitializeFactory();
        }
    }

    private static void InitializeFactory()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICacheService, NoOpCacheService>();
        Factory.AddServicesForUnitTesting(services);
    }

    private sealed class NoOpCacheService : ICacheService
    {
        public NoOpCacheService()
        {
        }

        public Task<T?> GetAsync<T>(
            string key,
            TimeSpan? duration = null,
            TimeSpan? distributedCacheDuration = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<T?>(default);

        public Task<T?> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? duration = null,
            TimeSpan? distributedCacheDuration = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<T?>(default);

        public Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? duration = null,
            TimeSpan? distributedCacheDuration = null,
            IReadOnlyCollection<string>? tags = null,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveAsync(
            string key,
            TimeSpan? duration = null,
            TimeSpan? distributedCacheDuration = null,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveByTagAsync(
            string tag,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveByTagAsync(
            IEnumerable<string> tags,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
