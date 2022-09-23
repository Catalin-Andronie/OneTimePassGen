namespace OneTimePassGen;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.ConfigureApplicationServices();

        var app = builder.Build();
        app.ConfigureApplicationRequestPipeline();

        // Before starting the application we want to ensure the database is up and running.
        await app.ApplyDatabaseMigrationsAsync();

        app.Run();
    }
}
