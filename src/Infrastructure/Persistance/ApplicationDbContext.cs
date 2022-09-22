using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Duende.IdentityServer.EntityFramework.Options;
using OneTimePassGen.Infrastructure.Identity;
using OneTimePassGen.Domain.Entities;
using System.Reflection;

namespace OneTimePassGen.Infrastructure.Persistance;

public sealed class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions)
        : base(options, operationalStoreOptions)
    {
    }

    /// <summary>
    ///     Gets or sets the <see cref="DbSet<GeneratedPassword>"/> of User generated passwords.
    /// </summary>
    public DbSet<UserGeneratedPassword> UserGeneratedPasswords => Set<UserGeneratedPassword>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
