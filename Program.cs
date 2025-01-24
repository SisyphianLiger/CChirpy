class Program
{


    public static void Main(string[] args)
    {
        // Declares a WebApplication Class
        var builder = WebApplication.CreateBuilder(args);

        // Addes the controllers from the modlers folder
        builder.Services.AddControllers();

        // Dependency Injection...container
        /*
         * Lets talk about singletons here. Because this class is only counting the amount of 
         * Hits, we actually want it to persist throughout the entierty of our program. Thus we 
         * use the Singleton service here to ensure that this program will exist until the server
         * stops.
         * */
        builder.Services.AddSingleton<MetricsMiddleware>();
        builder.Services.AddScoped<IChirpValidator, ChirpValidator>();


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
