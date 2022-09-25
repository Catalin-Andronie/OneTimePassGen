using MediatR;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Infrastructure.Identity;
using OneTimePassGen.Infrastructure.Persistance;

namespace OneTimePassGen.Application.IntegrationTests;

[SetUpFixture]
public class Testing
{
    private static string? s_currentUserId;

    public static IServiceScopeFactory ScopeFactory { get; private set; } = null!;
    public static DateTimeOffset? CurrentDateTime { get; set; }

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Test";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .Build();

        // Create a new connection string for each test run.
        // TODO: For now we only create unique connection strings for SqLite db. Add support for other database types.
        const string configurationName = "ConnectionStrings:DefaultConnection";
        configuration[configurationName] = !string.IsNullOrEmpty(configuration[configurationName])
            ? SqliteUniqueConnectionString(configuration[configurationName])
            : throw new ApplicationException($"Configuration '{configurationName}' is required");

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddSingleton(Mock.Of<IWebHostEnvironment>(w =>
            w.ApplicationName == "OneTimePassGen.Application.IntegrationTests"
        ));

        services.AddLogging();

        services.ConfigureApplicationServices(configuration);

        // Replace service registration for ICurrentUserService
        ReplaceService(services, _ => Mock.Of<ICurrentUserService>(s => s.UserId == s_currentUserId));

        // Replace service registration for ICurrentUserService
        ReplaceService(services, _ => Mock.Of<IDateTime>(s => s.Now == (CurrentDateTime ?? s.Now) &&
                                                              s.UtcNow == (CurrentDateTime ?? s.UtcNow)));

        ScopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();

        using var scope = ScopeFactory.CreateScope();
        await EnsureDatabaseIsSetupAsync(scope.ServiceProvider).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await using var scope = ScopeFactory.CreateAsyncScope();
        await EnsureDatabaseIsTeardownAsync(scope.ServiceProvider).ConfigureAwait(false);
    }

    public static void ReplaceService<TService>(ServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
    {
        var serviceDescriptor = services.FirstOrDefault(_ => _.ServiceType == typeof(TService));
        if (serviceDescriptor != null)
            services.Remove(serviceDescriptor);

        services.AddTransient(typeof(TService), services => implementationFactory(services)!);
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local", "Testing1234!", Array.Empty<string>());
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@local", "Administrator1234!", new[] { "Administrator" });
    }

    public static async Task<string> RunAsUserAsync(string userName, string password, string[] roles)
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser { UserName = userName, Email = userName };

        var result = await userManager.CreateAsync(user, password);

        if (roles.Length > 0)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (result.Succeeded)
        {
            s_currentUserId = user.Id;

            return s_currentUserId;
        }

        var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));

        throw new Exception($"Unable to create {userName}.{Environment.NewLine}{errors}");
    }

    public static async Task ResetDatabaseStateAsync()
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        await EnsureDatabaseIsTeardownAsync(scope.ServiceProvider).ConfigureAwait(false);
        await EnsureDatabaseIsSetupAsync(scope.ServiceProvider).ConfigureAwait(false);

        ResetUserState();
    }

    private static void ResetUserState()
    {
        s_currentUserId = null;
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues).ConfigureAwait(false);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync().ConfigureAwait(false);
    }

    private static async Task EnsureDatabaseIsSetupAsync(IServiceProvider serviceProvider)
        => await serviceProvider.ApplyDatabaseMigrationsAsync().ConfigureAwait(false);

    private static async Task EnsureDatabaseIsTeardownAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await appDbContext.Database.EnsureDeletedAsync();
    }

    private static string SqliteUniqueConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        var connectionOptions = new SqliteConnectionStringBuilder(connectionString);
        connectionOptions.DataSource = $"{connectionOptions.DataSource}_{Guid.NewGuid().ToString()[0..8]}";
        return connectionOptions.ToString();
    }
}