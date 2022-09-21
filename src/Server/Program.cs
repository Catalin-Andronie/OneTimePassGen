using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using NSwag;
using NSwag.Generation.Processors.Security;
using OneTimePassGen.Infrastructure.Identity;
using OneTimePassGen.Infrastructure.Persistance;

namespace OneTimePassGen;

internal sealed class Program
{
    private Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddIdentityServer()
            .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

        builder.Services.AddAuthentication()
            .AddIdentityServerJwt();

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // Register NSwag Swagger services
        builder.Services.AddSwaggerDocument(config =>
        {
            // Configure swagger document
            config.PostProcess = document =>
            {
                document.Info.Version = "v1";
                document.Info.Title = "One-time password generator - API";
                document.Info.Contact = new NSwag.OpenApiContact
                {
                    Name = "One-time password generator",
                    // TODO: Email = "support@company.com",
                    // TODO: Url = "https://support.com/contact",
                };
            };

            // Configure authentication
            const string securityType = "JWT";
            config.AddSecurity(securityType, Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type as value: Bearer {your JWT token}.",
            });

            config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(securityType));
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();

            // Register the NSwag Swagger generator and the NSwag Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
        else
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.MapFallbackToFile("index.html");

        // Before starting the application we want to ensure the database is up and running.
        await ApplyDatabaseMigrations(app.Services).ConfigureAwait(false);

        app.Run();
    }

    private static async Task ApplyDatabaseMigrations(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            // NOTE: We want to ensure that all migrations are added into the
            //       database, we call `MigrateAsync` method to run our custom
            //       migrations.
            await dbContext.Database.MigrateAsync().ConfigureAwait(false);
        }
        catch
        {
            // ignore
        }
    }
}
