using System.Reflection;

using Duende.IdentityServer.EntityFramework.Options;

using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;

using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Domain.Entities;
using OneTimePassGen.Infrastructure.Identity;

namespace OneTimePassGen.Infrastructure.Persistance;

public sealed class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>, IApplicationDbContext
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

        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            builder.UseValueConverterForType<DateTimeOffset>(new DateTimeOffsetToBinaryConverter());
            builder.UseValueConverterForType<DateTimeOffset?>(new DateTimeOffsetToBinaryConverter());
        }
    }
}