using System.Net;
using System.Net.Http.Json;
using TC.Agro.Identity.Application.UseCases.CreateUser;
using TC.Agro.Identity.Application.UseCases.LoginUser;
using TC.Agro.Identity.Tests.TestHelpers.Api;

namespace TC.Agro.Identity.Tests.Service.Api;

public sealed class AuthApiFlowTests : IClassFixture<IdentityApiWebApplicationFactory>
{
    private readonly IdentityApiWebApplicationFactory _factory;

    public AuthApiFlowTests(IdentityApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterAndLogin_WithValidPayload_ShouldCreateUserAndReturnJwt()
    {
        var ct = TestContext.Current.CancellationToken;

        await _factory.ResetDatabaseAsync();

        using var client = _factory.CreateClient();
        var email = $"api-user-{Guid.NewGuid():N}@tcagro.test";

        var registerRequest = new
        {
            Name = "API Integration User",
            Email = email,
            Username = $"apiuser{Guid.NewGuid():N}"[..18],
            Password = "Strong@Test123",
            Role = "Producer"
        };

        var registerResponse = await client.PostAsJsonAsync("/auth/register", registerRequest, ct);

        registerResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var createdUser = await registerResponse.Content.ReadFromJsonAsync<CreateUserResponse>(cancellationToken: ct);
        createdUser.ShouldNotBeNull();
        createdUser!.Email.ShouldBe(email);

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new
        {
            Email = email,
            Password = "Strong@Test123"
        }, ct);

        var loginBody = await loginResponse.Content.ReadAsStringAsync(ct);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK, loginBody);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>(cancellationToken: ct);
        loginPayload.ShouldNotBeNull();
        loginPayload!.Email.ShouldBe(email);
        loginPayload.JwtToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetUserList_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var ct = TestContext.Current.CancellationToken;

        await _factory.ResetDatabaseAsync();

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/user?pageNumber=1&pageSize=10", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithInvalidPayload_ShouldReturnBadRequest()
    {
        var ct = TestContext.Current.CancellationToken;

        await _factory.ResetDatabaseAsync();

        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            Name = "A",
            Email = "invalid-email",
            Username = "1",
            Password = "123",
            Role = "InvalidRole"
        }, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync(ct);
        body.ShouldContain("errors", Case.Insensitive);
    }
}
