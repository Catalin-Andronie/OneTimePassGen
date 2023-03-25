using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Infrastructure.Identity;
using OneTimePassGen.Infrastructure.Persistance;
using OneTimePassGen.Infrastructure.Services;

namespace OneTimePassGen.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlite(connectionString));

        services.AddTransient<IApplicationConfiguration, ApplicationConfiguration>((serviceProvider) =>
        {
            IConfiguration cfg = serviceProvider.GetRequiredService<IConfiguration>();
            return new ApplicationConfiguration(cfg);
        });

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

        services.AddScoped<IApplicationDbContext>(
            provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddTransient<IDateTime, DateTimeService>();
        services.AddTransient<IPasswordGenerator, PasswordGeneratorService>();
        services.AddTransient<IIdentityService, IdentityService>();
    }
}