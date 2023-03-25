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
public sealed class Testing
{
    private static string? s_currentUserId;

    public static IServiceScopeFactory ScopeFactory { get; private set; } = null!;
    public static DateTimeOffset? CurrentDateTime { get; set; }
    public static string? GeneratedPasswordValue { get; set; } = string.Empty;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Test";

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .Build();

        // Create a new connection string for each test run.
        // TODO: For now we only create unique connection strings for SqLite db. Add support for other database types.
        const string configurationName = "ConnectionStrings:DefaultConnection";
        configuration[configurationName] = !string.IsNullOrEmpty(configuration[configurationName])
            ? SqliteUniqueConnectionString(configuration[configurationName]!)
            : throw new ApplicationException($"Configuration '{configurationName}' is required");

        ServiceCollection services = new();

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

        // Replace service registration for IPasswordGenerator
        ReplaceService(services, _ =>
        {
            Mock<IPasswordGenerator> mock = new();
            mock.Setup(s => s.GeneratePassword()).Returns(GeneratedPasswordValue ?? string.Empty);
            return mock.Object;
        });

        ScopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();

        using IServiceScope scope = ScopeFactory.CreateScope();
        await EnsureDatabaseIsSetupAsync(scope.ServiceProvider).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await using AsyncServiceScope scope = ScopeFactory.CreateAsyncScope();
        await EnsureDatabaseIsTeardownAsync(scope.ServiceProvider).ConfigureAwait(false);
    }

    public static void ReplaceService<TService>(ServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
    {
        ServiceDescriptor? serviceDescriptor = services.FirstOrDefault(_ => _.ServiceType == typeof(TService));
        if (serviceDescriptor != null)
        {
            services.Remove(serviceDescriptor);
        }

        services.AddTransient(typeof(TService), services => implementationFactory(services)!);
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        await using AsyncServiceScope scope = ScopeFactory.CreateAsyncScope();

        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

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
        await using AsyncServiceScope scope = ScopeFactory.CreateAsyncScope();

        UserManager<ApplicationUser> userManager = scope
            .ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        ApplicationUser user = new() { UserName = userName, Email = userName };

        IdentityResult result = await userManager.CreateAsync(user, password);

        if (roles.Length > 0)
        {
            RoleManager<IdentityRole> roleManager = scope
                .ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            foreach (string role in roles)
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

        string errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));

        throw new Exception($"Unable to create {userName}.{Environment.NewLine}{errors}");
    }

    public static async Task ResetDatabaseStateAsync()
    {
        await using AsyncServiceScope scope = ScopeFactory.CreateAsyncScope();

        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
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
        await using AsyncServiceScope scope = ScopeFactory.CreateAsyncScope();

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues).ConfigureAwait(false);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        await using AsyncServiceScope scope = ScopeFactory.CreateAsyncScope();

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        await using AsyncServiceScope scope = ScopeFactory.CreateAsyncScope();

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync().ConfigureAwait(false);
    }

    private static async Task EnsureDatabaseIsSetupAsync(IServiceProvider serviceProvider)
        => await serviceProvider.ApplyDatabaseMigrationsAsync().ConfigureAwait(false);

    private static async Task EnsureDatabaseIsTeardownAsync(IServiceProvider serviceProvider)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

        ApplicationDbContext appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await appDbContext.Database.EnsureDeletedAsync();
    }

    private static string SqliteUniqueConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        SqliteConnectionStringBuilder connectionOptions = new(connectionString);
        connectionOptions.DataSource = $"{connectionOptions.DataSource}_{Guid.NewGuid().ToString()[0..8]}";
        return connectionOptions.ToString();
    }
}