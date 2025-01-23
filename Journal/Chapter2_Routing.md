# What Route are we taking?

As with all RESTful API's there are some conventions, namely the keywords we use to perform CRUD, Create, Read, update, and delete. Now Go and C#'s handlers are quite similar, but obviously have different flavors worth disecting.

Instead of going through the Go code first then the C# code second however, I will in the same sections cover both code snippets highlighting their differences and what have you.


# Making an Atomic Counter

Atomics are something I have largely been introduced to during my time in University, and I understand them as the following. A Compare and Swap Data structure that is given a special location in the heap, and is able to handle multiple requests at one time causing other threads to spinlock. That being stated, for our task we needed to add middleware to actually take into account how much traffic was coming to our site. In other words, how many times is our handlers associated with the "/app" keyword called.

## The Middleware Compare
Atomic Integers are primitives found both in Go and C#, so there implementation is quite simple. However in Go, with Functions being first class citizens you can very easily wrap your middleware inside of your handler with the following style middleware.

```Go
// This struct will hold the fileserverHits info
type apiConfig struct {
	fileserverHits atomic.Int32
}

// Here we wrap our middlewareMetrics, the rest is just making the path start in the root dir
mux.Handle("/app/", apiCfg.middlewareMetricsInc(http.StripPrefix("/app", http.FileServer(http.Dir(filepathRoot)))))


// Finally our middlewareMetrics which will take in a http handler and return the next call but add 1 
func (cfg *apiConfig) middlewareMetricsInc(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		cfg.fileserverHits.Add(1)
		next.ServeHTTP(w, r)
	})
}


// For clarity here is reset
func (cfg *apiConfig) handlerReset(w http.ResponseWriter, r *http.Request) {
	cfg.fileserverHits.Store(0)
	w.WriteHeader(http.StatusOK)
	w.Write([]byte("Hits reset to 0"))
}
```

As you can see here, the call is pretty straight forward, when our file is at /app and above, our middleware triggers an event increasing fileserverHits by 1. Pretty simple I would say. What is this like in C#.

### Perhaps a bit of the same?
```cs
    public static void Main(string[] args)
    {
        // Declares a WebApplication Class
        var builder = WebApplication.CreateBuilder(args);
        
        //...

        // Dependency Injection...container
        builder.Services.AddTransient<MetricsMiddleware>();

        //...

        // Use Middleware
        app.UseMiddleware<MetricsMiddleware>();
        // ...
    }
}
```

In C#, we...you guessed it, create a class that implements a IMiddleware Interface. What is the IMiddleware Interface, well effectively its a function that plays the role of the Middleware similar to Golangs:

```cs
public class MetricsMiddleware : IMiddleware
{

    private static long _counter = 0;

    private void TrackWebsiteHits()
    {
        Interlocked.Increment(ref _counter);
    }

    public long GetCurrentCount()
    {
        return Interlocked.Read(ref _counter);
    }

    public void ResetCounter()
    {
        Interlocked.Exchange(ref _counter, 0);
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {

        // Count everyhit that exists with /app
        if (context.Request.Path.ToString().StartsWith("/app"))
        {
            TrackWebsiteHits();
        }
        await next(context);
    }
}
```

So what are we looking at here...well for starters the middleware is assigned methods that correspond to Reseting, getting, and incrementing the current count. The counter may look wierd here, and very different from golang, but allow me to explain.

I made the variable private because it is only via `GetCounter()` that it should be accessable...don't want people messing with it. The static keyword is to make sure it persists outside of an instance of MetricsMiddleware. Why is this important? Well it needs to track all calls going into the system, and static, effectively makes it a global variable. 

You may also be noticing that there is a if condition in the function interface provided by IMiddleware. It is there because there are two handlers my code is searching through, the "/" level and the "/app" level. Without that check, the counter would increment for all handlers including `reset`. Definitely not working as intended. 

## Middleware is great and all but what do the Handlers look like?
Well lets get started Golang Hooooo!
```Go
    // ...
	mux.HandleFunc("GET /healthz", handlerReadiness)
	mux.HandleFunc("POST /reset", apiCfg.handlerReset)
	mux.HandleFunc("GET /metrics", apiCfg.handlerMetrics)

    // Handlers in the HandleFunc
    func handlerReadiness(w http.ResponseWriter, r *http.Request) {
        w.Header().Add("Content-Type", "text/plain; charset=utf-8")
        w.WriteHeader(http.StatusOK)
        w.Write([]byte(http.StatusText(http.StatusOK)))
    }

    func (cfg *apiConfig) handlerReset(w http.ResponseWriter, r *http.Request) {
        cfg.fileserverHits.Store(0)
        w.WriteHeader(http.StatusOK)
        w.Write([]byte("Hits reset to 0"))
    }

    func (cfg *apiConfig) handlerMetrics(w http.ResponseWriter, r *http.Request) {
        w.Header().Add("Content-Type", "text/plain; charset=utf-8")
        w.WriteHeader(http.StatusOK)
        w.Write([]byte(fmt.Sprintf("Hits: %d", cfg.fileserverHits.Load())))
    }
```

Go's is pretty compact and dare I say easy. But there is some interesting components to note, you gotta really add the header add the status, and write the body all the time. So in that sense its a bit like manual labor. But you know its also a 4 line function call to do so, so not bad

What about C# however, is it awful, unproductive, horrible?

Nah its alright
```cs

// Here we are defining out Handlers
namespace Chirpy.Controllers;

[ApiController]
[Route("/")]
public class ChirpyController : ControllerBase
{

    private readonly MetricsMiddleware _metrics;

    public ChirpyController(MetricsMiddleware metrics)
    {
        _metrics = metrics;
    }

    [HttpGet("healthz")]
    public ActionResult<string> HealthCheck()
    {
        return Content("OK", "text/plain; charset=utf-8");
    }

    [HttpGet("metrics")]
    public IActionResult GetMetrics() {
        return Ok($"Hits: {_metrics.GetCurrentCount()}");
    }

    [HttpPost("reset")]
    public IActionResult ResetMetrics()
    {
        _metrics.ResetCounter();
        return Ok();
    }
}
```

So yea, a lot more boiler plate as to be expected, but honestly not too bad. And you can see that there is a dependency injection here with MetricsMiddleware that is quite different then Golang. Even though we have passed the MetricsMiddleware to our middleware, we are also passing it in our controller class because we need access to methods like `GetCurrentCount()`. But we want MetricsMiddleware to not be coupled with our controller obviously. 

There is also just a caveat that the Route at the top can be anything by the way, but for this assignment where these methods need to be called at the "/" file path and the other handlers for static files at the "/app" level, Route can also be `[Route([controller])]`. That will mean that the Controller in ChirpyController will be replaced, and the route default will be set to "/Chirpy"

## A Final Mention!
Soo...unlike Go, which will just give ya the StaticFile middleware wrapping, C#'s middleware doesn't do that, or at least there was skill issues for me to get my custom middleware wrapped around my file system paths. 

I.E. `localhost:8080/app/assets/logo.png` should add 1 to the counter but doesn't in C# with the standard File Set up... to my knowledge.

So I decided in the spirit of OOP, to make a staticfile serving class! (I think this will start to become a theme btw)

```cs
namespace StaticFile.Controllers;

[ApiController]
[Route("app")]
public class StaticFileController : ControllerBase
{
    [HttpGet("assets")]
    public ActionResult<string> GetChirpyAsHtml()
    {
        // Get the path to wwwroot/assets/logo.png
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "logo.png");
        return PhysicalFile(filePath, "image/png");
    }
    [HttpGet]
    public ActionResult GetChirpyHomePage()
    {
        // Get the path to wwwroot/index.html
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
        return PhysicalFile(filePath, "text/html");
    }
}
```

So lets see here, first we have the top route being app, this makes sense because well, that is what the static file server (wwwroot) is set as. Then we have the two paths, our index.html as `GetChirpyHomePage()` and `GetChirpyPic()` for our sweet, sweet bird pic. 

Now then...You may notice that HttpGet for `GetChirpyHomePage()` doesn't have a "route", well that is because it will default to the Route from the top...very nice! Gotta say this reminds me a lot of Rust's frameworks I have used in the path.

### The final final thing :tm:
Ok if you look in the functions for getting the static html you may notice...filepath. What the hell is Path.Combine() doing?? Well, we are starting at app which is the root of our static files, and we are saying hey find the physical file in the filepath, and return that content type, either an image or text. 

This took me...awhile to find out, and I happily stumbled upon it reading through the method signatures.

## Conclusion
That is it for routes! The project will start becoming more complex so it may be reasonable to have less huge code blocks and more snippets, but I will be sure to caveat/let you know if that is the case. But yea, so far so good.


