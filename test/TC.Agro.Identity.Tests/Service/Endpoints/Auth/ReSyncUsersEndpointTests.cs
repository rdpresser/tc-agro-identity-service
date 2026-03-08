using TC.Agro.Identity.Application.Abstractions;
using TC.Agro.Identity.Tests.Service.Endpoints;

namespace TC.Agro.Identity.Tests.Service.Endpoints.Auth
{
    public sealed class ReSyncUsersEndpointTests
    {
        [Fact]
        public void Configure_ShouldSetAdminRoleAndAuthResyncRoute()
        {
            var source = EndpointSourceAssertions.LoadEndpointSource("Auth", "ReSyncUsersEndpoint.cs");

            EndpointSourceAssertions.AssertContains(
                source,
                "Post(\"resync/users\")",
                "RoutePrefixOverride(\"auth\")",
                $"Roles({nameof(AppConstants)}.{nameof(AppConstants.AdminRole)})");
        }
    }
}
