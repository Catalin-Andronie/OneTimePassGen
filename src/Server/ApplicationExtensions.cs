using Microsoft.EntityFrameworkCore;

using NSwag;
using NSwag.Generation.Processors.Security;

using OneTimePassGen.Application;
using OneTimePassGen.Application.Common.Interfaces;
using OneTimePassGen.Infrastructure;
using OneTimePassGen.Infrastructure.Persistance;
using OneTimePassGen.Server.Services;

namespace OneTimePassGen;

internal static class ApplicationExtensions
{
    public static WebApplicationBuilder ConfigureApplicationServices(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();

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
        return builder;
    }

    public static WebApplication ConfigureApplicationRequestPipeline(this WebApplication app)
    {
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
        return app;
    }

    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            if (dbContext.Database.IsSqlite())
            {
                await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
            }
            else
            {
                // NOTE: We want to ensure that all migrations are added into the
                //       database, we call `MigrateAsync` method to run our custom
                //       migrations.
                await dbContext.Database.MigrateAsync().ConfigureAwait(false);
            }
        }
        catch
        {
            // ignore
        }
    }
}