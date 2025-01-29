using Utilities;
using PostgresDB;


class Program
{

    public static void Main(string[] args)
    {
        // Connection String for Postgres     
        var cfg = new ConfigurationAccess();


        if (cfg.MigrateDB)
        {
            MigrationScript.MigrateDB(cfg.DbUrl);
        }

        // Declares a WebApplication Class
        var builder = WebApplication.CreateBuilder(args);

        // Create and verify database connection
        var db = new ChirpyDatabase(cfg.DbUrl, cfg.DevMode);
        try
        {
            db.OpenAsync().GetAwaiter(); // Make sure your ChirpyDatabase has this method
            builder.Services.AddSingleton<ChirpyDatabase>((_) => db);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to database: {ex.Message}");
            throw; // This will prevent the application from starting if DB connection fails
        }

        // Addes the controllers from the modlers folder
        builder.Services.AddControllers();

        // Singletons that keep track of items
        builder.Services.AddSingleton<MetricsMiddleware>();
        builder.Services.AddScoped<IChirpValidator, ChirpValidator>();



        // Adds Options to the Configuration
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(cfg.LocalHost);
        }
                );

        // Builds Server (now read only)
        var app = builder.Build();

        var useFileServer = new FileServerOptions();
        useFileServer.RequestPath = "/app";
        app.UseFileServer(useFileServer);


        // Mapping all controllers
        app.MapControllers();

        // Use Middleware
        app.UseMiddleware<MetricsMiddleware>();

        // Handle Errors is not in development
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.Run();
    }
}
