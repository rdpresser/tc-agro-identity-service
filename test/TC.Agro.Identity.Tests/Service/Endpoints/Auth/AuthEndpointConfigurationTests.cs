using TC.Agro.Identity.Application.Abstractions;
using TC.Agro.Identity.Tests.Service.Endpoints;

namespace TC.Agro.Identity.Tests.Service.Endpoints.Auth;

public sealed class AuthEndpointConfigurationTests
{
    [Fact]
    public void LoginEndpoint_ShouldExposeAuthLoginRoute()
    {
        var source = EndpointSourceAssertions.LoadEndpointSource("Auth", "LoginEndpoint.cs");

        EndpointSourceAssertions.AssertContains(
            source,
            "Post(\"login\")",
            "RoutePrefixOverride(\"auth\")");
    }

    [Fact]
    public void CreateUserEndpoint_ShouldExposeAuthRegisterRoute()
    {
        var source = EndpointSourceAssertions.LoadEndpointSource("Auth", "CreateUserEndpoint.cs");

        EndpointSourceAssertions.AssertContains(
            source,
            "Post(\"register\")",
            "RoutePrefixOverride(\"auth\")");
    }

    [Fact]
    public void ChangePasswordEndpoint_ShouldExposeAuthChangePasswordRoute()
    {
        var source = EndpointSourceAssertions.LoadEndpointSource("Auth", "ChangePasswordEndpoint.cs");

        EndpointSourceAssertions.AssertContains(
            source,
            "Post(\"change-password\")",
            "RoutePrefixOverride(\"auth\")");
    }

    [Fact]
    public void CheckEmailAvailabilityEndpoint_ShouldExposeAuthCheckEmailRoute()
    {
        var source = EndpointSourceAssertions.LoadEndpointSource("Auth", "CheckEmailAvailabilityEndpoint.cs");

        EndpointSourceAssertions.AssertContains(
            source,
            "Get(\"check-email/{email}\")",
            "RoutePrefixOverride(\"auth\")");
    }

    [Fact]
    public void ReSyncUsersEndpoint_ShouldRequireAdminRoleOnAuthResyncRoute()
    {
        var source = EndpointSourceAssertions.LoadEndpointSource("Auth", "ReSyncUsersEndpoint.cs");

        EndpointSourceAssertions.AssertContains(
            source,
            "Post(\"resync/users\")",
            "RoutePrefixOverride(\"auth\")",
            $"Roles({nameof(AppConstants)}.{nameof(AppConstants.AdminRole)})");
    }
}
