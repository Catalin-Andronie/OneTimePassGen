using IdentityModel.Client;

using Microsoft.AspNetCore.Mvc.Testing;

namespace OneTimePassGen.Server.IntegrationTests;

public abstract class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected abstract Task<string> GetAccessTokenAsync(HttpClient client, string userName, string password);

    public Task<HttpClient> GetAnonymousClientAsync()
    {
        HttpClient client = CreateClient();

        return Task.FromResult(client);
    }

    public async Task<HttpClient> GetAuthenticatedClientAsync(string userName, string password)
    {
        HttpClient client = CreateClient();

        string token = await GetAccessTokenAsync(client, userName, password);

        client.SetBearerToken(token);

        return client;
    }
}