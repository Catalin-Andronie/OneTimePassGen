using System.Net;

using FluentAssertions;

using Xunit;

namespace OneTimePassGen.Server.IntegrationTests.Controllers.UserGeneratedPasswords;

public sealed class GetUserGeneratedPasswordsApiTests : IClassFixture<OneTimePassGenWebAppFactory>
{
    private readonly OneTimePassGenWebAppFactory _factory;

    public GetUserGeneratedPasswordsApiTests(OneTimePassGenWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUserGeneratedPasswordsApi_ReturnsUnauthorizedStatus_ForAnonymousUser()
    {
        using var client = await _factory.GetAnonymousClientAsync();

        var response = await client.GetAsync("api/user-generated-passwords");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserGeneratedPasswordsApi_ReturnsSuccessStatus_ForAuthenticatedUser()
    {
        using var client = await _factory.GetAuthenticatedClientAsync();

        var response = await client.GetAsync("api/user-generated-passwords");

        response.EnsureSuccessStatusCode();
    }
}