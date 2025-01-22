using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

class Program
{



    public static void Main(string[] args)
    {
        // Declares a WebApplication Class
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages();
        builder.Services.AddControllers();


        // Adds Options to the Configuration
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(8080);
        }
                );

        // Builds Server (now read only)
        var app = builder.Build();

        app.UseRouting();
        // Mapping all controllers
        app.MapControllers();
        // Configure DefaultFilesOptions with the same path
        var defaultFilesOptions = new DefaultFilesOptions();
        defaultFilesOptions.DefaultFileNames.Clear();
        defaultFilesOptions.DefaultFileNames.Add("index.html");
        defaultFilesOptions.RequestPath = "/app";
        app.UseDefaultFiles(defaultFilesOptions);

        // Then your static files configuration
        app.UseStaticFiles(new StaticFileOptions
        {
            RequestPath = "/app",
        });
        app.UseDirectoryBrowser( new DirectoryBrowserOptions {
                RequestPath = "/app"
        });

        // Handle Errors is not in development
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }


        app.Run();

    }
}
