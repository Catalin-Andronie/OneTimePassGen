using Duende.IdentityServer.Models;

using IdentityModel.Client;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OneTimePassGen.Infrastructure.Persistance;

using Xunit;

namespace OneTimePassGen.Server.IntegrationTests;

public sealed class OneTimePassGenWebAppFactory : CustomWebApplicationFactory<Program>, IAsyncLifetime
{
    private string AppClientId { get; } = "OneTimePassGen.Server.IntegrationTests.Client";
    private string AppClientSecret { get; } = "secret";

    public async Task InitializeAsync()
    {
        await AppDbActions.EnsureDatabaseIsSetupAsync(Services).ConfigureAwait(false);
        await AppDbActions.SeedTestUsersAsync(Services).ConfigureAwait(false);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await AppDbActions.EnsureDatabaseIsTeardownAsync<ApplicationDbContext>(Services).ConfigureAwait(false);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Test";

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .Build();

        // Create a new connection string for each test run.
        // TODO: For now we only create unique connection strings for SqLite db. Add support for other database types.
        {
            string databaseNamePrefix = Guid.NewGuid().ToString()[0..8];
            string appDatabaseConnectionString = configuration["ConnectionStrings:DefaultConnection"]!;
            configuration["ConnectionStrings:DefaultConnection"] = AppDbActions.SqliteUniqueConnectionString(appDatabaseConnectionString);
        }

        builder
            .UseEnvironment(environmentName)
            .ConfigureAppConfiguration(config => config
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddConfiguration(configuration)
            )
            .ConfigureServices(services =>
                services.AddIdentityServerBuilder()
                    .AddInMemoryClients(new Client[]
                    {
                        new Client
                        {
                            ClientId = AppClientId,
                            ClientSecrets = { new Secret(AppClientSecret.Sha256()) },

                            AllowedGrantTypes = { GrantType.ResourceOwnerPassword },
                            AllowedScopes = { "OneTimePassGen.ServerAPI", "openid", "profile" }
                        }
                    })
            );
    }

    protected override async Task<string> GetAccessTokenAsync(HttpClient client, string userName, string password)
    {
        DiscoveryDocumentResponse disco = await client.GetDiscoveryDocumentAsync();

        if (disco.IsError)
            throw new Exception(disco.Error);

        TokenResponse response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = AppClientId,
            ClientSecret = AppClientSecret,

            Scope = "OneTimePassGen.ServerAPI openid profile",
            UserName = userName,
            Password = password
        });

        return response.IsError
            ? throw new Exception(response.Error)
            : response.AccessToken;
    }

    internal async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        // TODO: Instead of hardcoding an authenticated user credentials, we should have a type which hold this info.
        return await GetAuthenticatedClientAsync("jane.smith@example.com", "Password12!");
    }

    private record AuthResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}