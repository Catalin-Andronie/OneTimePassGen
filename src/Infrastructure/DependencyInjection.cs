using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OneTimePassGen.Infrastructure.Identity;
using OneTimePassGen.Infrastructure.Persistance;

namespace OneTimePassGen.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlite(connectionString));

        services
            .AddDefaultIdentity<ApplicationUser>(
                options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services
            .AddIdentityServer()
            .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

        services
            .AddAuthentication()
            .AddIdentityServerJwt();
    }
}