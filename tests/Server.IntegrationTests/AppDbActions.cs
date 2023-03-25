using System.Linq.Expressions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OneTimePassGen.Infrastructure.Identity;
using OneTimePassGen.Infrastructure.Persistance;

namespace OneTimePassGen.Server.IntegrationTests;

internal static class AppDbActions
{
    public static string SqliteUniqueConnectionString(string connectionString, string? databaseNamePrefix = null)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        if (string.IsNullOrEmpty(databaseNamePrefix))
            databaseNamePrefix = Guid.NewGuid().ToString()[0..8];

        SqliteConnectionStringBuilder connectionOptions = new(connectionString);
        connectionOptions.DataSource = $"{connectionOptions.DataSource}_{databaseNamePrefix}";
        return connectionOptions.ToString();
    }

    public static void SeedTestUsers(IServiceProvider provider)
        => SeedTestUsersAsync(provider).ConfigureAwait(false).GetAwaiter().GetResult();

    public static async Task SeedTestUsersAsync(IServiceProvider provider)
    {
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        UserManager<ApplicationUser> userManager = scope
            .ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        // TODO: Instead of hardcoding the default list of user here, we should have a type to hold it.
        const string defaultUserPassword = "Password12!";

        ApplicationUser[] defaultUsers = new[]
        {
            new ApplicationUser
            {
                UserName = "jane.smith@example.com",
                Email = "jane.smith@example.com",
                EmailConfirmed = true,
                LockoutEnabled = false
            }
        };

        foreach (ApplicationUser defaultUser in defaultUsers)
        {
            bool userExist = await userManager
                .FindByIdAsync(defaultUser.Id)
                .ConfigureAwait(false) is not null;

            if (!userExist)
            {
                await userManager
                    .CreateAsync(defaultUser, defaultUserPassword)
                    .ConfigureAwait(false);
            }
        }
    }

    public static void EnsureDatabaseIsSetup(IServiceProvider provider)
    {
        EnsureDatabaseIsSetupAsync(provider)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
    }

    public static Task EnsureDatabaseIsSetupAsync(IServiceProvider provider)
        => provider.ApplyDatabaseMigrationsAsync();

    public static void EnsureDatabaseIsTeardown(IServiceProvider provider)
        => EnsureDatabaseIsTeardownAsync(provider).ConfigureAwait(false).GetAwaiter().GetResult();

    public static Task EnsureDatabaseIsTeardownAsync(IServiceProvider provider)
        => EnsureDatabaseIsTeardownAsync<ApplicationDbContext>(provider);

    public static async Task EnsureDatabaseIsTeardownAsync<TContext>(IServiceProvider provider)
        where TContext : DbContext
    {
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        TContext appDbContext = scope
            .ServiceProvider
            .GetRequiredService<TContext>();

        await appDbContext
            .Database
            .EnsureDeletedAsync()
            .ConfigureAwait(false);
    }

    public static async Task<TEntity?> FindEntityAsync<TContext, TEntity>(IServiceProvider provider, Expression<Func<TEntity, bool>> predicate)
        where TContext : DbContext
        where TEntity : class
    {
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        TContext context = scope
            .ServiceProvider
            .GetRequiredService<TContext>();

        return await context
            .Set<TEntity>()
            .FirstOrDefaultAsync(predicate)
            .ConfigureAwait(false);
    }

    public static async Task<TEntity> EnsureFindEntityAsync<TContext, TEntity>(IServiceProvider provider, Expression<Func<TEntity, bool>> predicate)
        where TContext : DbContext
        where TEntity : class
    {
        TEntity? entity = await FindEntityAsync<TContext, TEntity>(provider, predicate).ConfigureAwait(false);
        return entity ?? throw new Exception($"Entity '{typeof(TEntity)}' could not be found.");
    }

    public static async Task<int> AddEntitiesAsync<TContext, TEntity>(IServiceProvider provider, params TEntity[] entities)
        where TContext : DbContext
        where TEntity : class
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        TContext context = scope
            .ServiceProvider
            .GetRequiredService<TContext>();

        context.Set<TEntity>().AddRange(entities);

        return await context
            .SaveChangesAsync()
            .ConfigureAwait(false);
    }

    public static async Task EnsureAddEntitiesAsync<TContext, TEntity>(IServiceProvider provider, int expectedItemsSaved, params TEntity[] entities)
        where TContext : DbContext
        where TEntity : class
    {
        int itemsSaved = await AddEntitiesAsync<TContext, TEntity>(provider, entities).ConfigureAwait(false);

        if (itemsSaved != expectedItemsSaved)
        {
            throw new Exception($"Only {itemsSaved} out of {entities.Length} '{typeof(TEntity)}' entities saved.");
        }
    }

    public static async Task<int> CountEntitiesAsync<TEntity>(IServiceProvider provider) where TEntity : class
    {
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        ApplicationDbContext context = scope
            .ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        return await context
            .Set<TEntity>()
            .CountAsync()
            .ConfigureAwait(false);
    }
}