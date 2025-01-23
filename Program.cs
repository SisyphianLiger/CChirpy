class Program
{



    public static void Main(string[] args)
    {
        // Declares a WebApplication Class
        var builder = WebApplication.CreateBuilder(args);

        // Addes the controllers from the modlers folder
        builder.Services.AddControllers();

        // Dependency Injection...container
        builder.Services.AddTransient<MetricsMiddleware>();


        // Adds Options to the Configuration
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(8080);
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
