using Utilities;
using PostgresDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

        // Create JwT Webservices
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "yourdomain.com",
                    ValidAudience = "yourdomain.com",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_super_secret_key"))
                };
            });



        builder.Services.AddAuthorization();
        // Addes the controllers from the modlers folder
        builder.Services.AddControllers();

        // Singletons that keep track of items
        builder.Services.AddSingleton<MetricsMiddleware>();
        builder.Services.AddScoped<IChirpValidator, ChirpValidator>();
        builder.Services.AddScoped<ConfigurationAccess>();



        // Adds Options to the Configuration
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(cfg.LocalHost);
        }
                );

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

        // Builds Server (now read only)
        var app = builder.Build();

        var useFileServer = new FileServerOptions();
        useFileServer.RequestPath = "/app";
        app.UseFileServer(useFileServer);

        // Use Middleware
        app.UseMiddleware<MetricsMiddleware>();

        // For JwT's
        app.UseAuthentication();
        app.UseAuthorization();

        // Mapping all controllers
        app.MapControllers();


        // Handle Errors is not in development
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.Run();
    }
}
