using TC.Agro.Identity.Application.Abstractions;
using TC.Agro.Identity.Tests.Service.Endpoints;

namespace TC.Agro.Identity.Tests.Service.Endpoints.User;

public sealed class UserEndpointConfigurationTests
{
    [Fact]
    public void GetUserByEmailEndpoint_ShouldExposeRouteAndAllowedRoles()
    {
        var source = EndpointSourceAssertions.LoadEndpointSource("User", "GetUserByEmailEndpoint.cs");

        EndpointSourceAssertions.AssertContains(
            source,
            "Get(\"user/by-email/{email}\")",
            $"Roles({nameof(AppConstants)}.{nameof(AppConstants.UserRole)}, {nameof(AppConstants)}.{nameof(AppConstants.AdminRole)}, {nameof(AppConstants)}.{nameof(AppConstants.ProducerRole)})");
    }

    [Fact]
    public void GetUserListEndpoint_ShouldExposeRouteAndAllowedRoles()
    {
        var source = EndpointSourceAssertions.LoadEndpointSource("User", "GetUserListEndpoint.cs");

        EndpointSourceAssertions.AssertContains(
            source,
            "Get(\"user\")",
            $"Roles({nameof(AppConstants)}.{nameof(AppConstants.UserRole)}, {nameof(AppConstants)}.{nameof(AppConstants.AdminRole)}, {nameof(AppConstants)}.{nameof(AppConstants.ProducerRole)})");
    }

    [Fact]
    public void UpdateUserEndpoint_ShouldExposeGuidRouteAndAllowedRoles()
    {
        var source = EndpointSourceAssertions.LoadEndpointSource("User", "UpdateUserEndpoint.cs");

        EndpointSourceAssertions.AssertContains(
            source,
            "Put(\"user/{id:guid}\")",
            $"Roles({nameof(AppConstants)}.{nameof(AppConstants.UserRole)}, {nameof(AppConstants)}.{nameof(AppConstants.AdminRole)}, {nameof(AppConstants)}.{nameof(AppConstants.ProducerRole)})");
    }

    [Fact]
    public void DeleteUserEndpoint_ShouldExposeRouteAndRequireAdminRole()
    {
        var source = EndpointSourceAssertions.LoadEndpointSource("User", "DeleteUserEndpoint.cs");

        EndpointSourceAssertions.AssertContains(
            source,
            "Delete(\"user/{id}\")",
            $"Roles({nameof(AppConstants)}.{nameof(AppConstants.AdminRole)})");
    }
}
