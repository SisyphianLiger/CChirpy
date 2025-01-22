# Serving Static Files

So lets start with the basics! How can we actually start a server in C#, surely it must be difficult, perhaps incomprehensible to the human mind...

I say we start simple...with the go approach, but first lets look at the go file tree setup.


## It's Go time!

The Go Backend has a simple layout:
```Go
        assets/
        ├── images/
        │   └── logo.png
        styles/
        │   └── main.css
        main.go
```

In `main.go` you will see the following code:
```Go
    func main() {
        const filepathRoot = "."
        const port = "8080"

        mux := http.NewServeMux()
        mux.Handle("/", http.FileServer(http.Dir(filepathRoot)))

        srv := &http.Server{
            Addr:    ":" + port,
            Handler: mux,
        }

        log.Printf("Serving files from %s on port: %s\n", filepathRoot, port)
        log.Fatal(srv.ListenAndServe())
}
```

So just what is going on here? How can our lovely `logo.png` be found in this directory? And for that matter how can we serve our index.html?

The answer is within the mux.Handle func. Here we create a handler that uses some root, in our case `.` to direct traffic. What does this mean, well, upon running the server we will be greated with index.html, as simple as that. But...what about our bird! It's the most important component to a sucessful 5 start business rated via trustpilot!!??

Calm down, that handler also can handle the static image as well. Think of it this way, we can view this file stry like a series of strings, where the root of the string is at the `.`. That means that in order to reach the assets we need to go from `.` to `assets/images/logo.png`. So when our server is online we simply go to that address and we are ready to see our wonderful bird. 

## Let's try to C more Sharply here...(buh dump tis)
Now I have to admit, C#'s setup was...more complicated, BUT! I think there are some definite positive trade-offs for developer experience I would like to highlight. First, let's take a look at the file tree.


```go
        models/
        │   └──ChirpyController.cs
        │   └──StaticFileController.cs
        wwwroot/
        │   └──index.html
        │   └──assets/
                └──logo.png
        Program.cs 
```

Ok, now to be clear there are more folders in both sections, i.e. `/bin` in C\#, but I just want to highlight the sections that matter here. So, what is the `git diff` here?

Well lets start by looking at `Program.cs`

```cs

class Program
{

    public static void Main(string[] args)
    {
        // Declares a WebApplication Class
        var builder = WebApplication.CreateBuilder(args);

        // Addes the controllers from the modlers folder
        builder.Services.AddControllers();

        // Adds Options to the Configuration
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(8080);
        }
                );

        // Builds Server (now read only)
        var app = builder.Build();

        // Mapping all controllers
        app.MapControllers();

        // Configure DefaultFilesOptions with the same path
        var defaultFilesOptions = new DefaultFilesOptions();
        defaultFilesOptions.RequestPath = "/app";

        // Adding this option to the pathway
        app.UseDefaultFiles(defaultFilesOptions);


        // Handle Errors is not in development
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // Vroom Vroom
        app.Run();

    }
}
```

For starters...there is a...bit more code then before but some of this is a result of sending the write html processed correctly, I will get into that later. Now then, `.NET` uses a `WebApplication.CreateBuilder(args)` to build its webserver and with that come a whole lot of interfaces. The important thing here is that we tell the class before building to take into consideration the controller classes within the `models/` folder. That will allow for handlers to be able to be accessed, but again we will get to that later. 

From that point on there are some key differences I would like to highlight from both langs. The first is in golang you are required to add a port number, but it would seem that C\#, out of the box assigns its own port, this quality of life feature is nice, sometimes, but if you do want to assign a port, you need to access Webhosts, Kestrel and assign localhost a port. Not really a problem though.

C\# devs may wonder why the hell there is a "/app" here? That is due to passing the modules response from request. In the response, we have decided to move off of the default root, i.e., "/", and instead use "/app" instead for functionality. Given that is the case we need to set a `defaultFilesOptions` or the place where the webserver will look to connect the file string to the host.

I've gotten a bit ahead of my self, you can view this main as two parts, configuration and runtime. Configuration involved what port number and controllers we specify are in use, and once we build out runtime options tell us where we find our static fields and controllers. 

So all in all, very similar, but this is static files, not really so fun! Up next will be routing!



