# Monoliths or Microservices...???!!!
So, this chapter entailed the differences between monoliths and microserices, because up to this point in a technical sense we just through a ton of methods, and handlers, etc. While the technical concepts in this chapter are important what is more imporant is the seperation of concerns.

So, what does that mean, well when I check the metrics of my site, I am technically speaking an admin. However the methods attached to the API class, contain that logic. THIS CALLS FOR SEPERATION! 

## Divide....and Conquer!
```cs

// Here we are defining out Handlers
namespace Admin.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{

    private readonly MetricsMiddleware _metrics;

    public AdminController(MetricsMiddleware metrics)
    {
        _metrics = metrics;
    }

    [HttpGet("metrics")]
    public IActionResult GetMetrics()
    {
        var body = "<html>\n<body>\n<h1>Welcome, Chirpy Admin</h1>\n<p>Chirpy has been visited "
                    + _metrics.GetCurrentCount()
                    + " times!</p>\n</body>\n</html>";
        return Content(body, "text/html");
    }

    [HttpPost("reset")]
    public IActionResult ResetMetrics()
    {
        _metrics.ResetCounter();
        return Ok();
    }
}
```

So what do we have here now? Well, now our route starts at admin, this means that we can gather data here from the admin take, and later with authorization make it such that there are credentials to access these methods. Ok, great, so you may notice that the MetricsMiddleware is here! Have we injected it twice over? 

No, in fact, we have seperated our logic here, because technically it is the admin that needs this middleware, and this middleware needs to be within the program. Ok, but what is happening with Get Metrics now? 

Well It is decided that we will display to the admin text showing the current hits on the website. This is a rather archaic way to do so, but making a simple string of html and adding the count from our middleware will suffice. Now we have seperated Admin, and API, into two distinguishable classes. 

There is one more thing to note here, I identified that the class, MiddlewareMetrics would be instantiated each time it passed through the handlers. This is a bad idea, given the concept of the class, is such that it persists throughout the program. 

The fix here, is to use a Singleton pattern, that is, a pattern that allows for a class to exist throughout the entire program. So how can we do that?

```cs
    // Before it was AddTransient causing instances of the class now its a singleton!        
    builder.Services.AddSingleton<MetricsMiddleware>();
```

Well, wonderfully enought we have the pattern ready with our builder. So we add the class into the AddSingleton function. That means that our static counter we used before, is no longer needed, and we save our server the hassle of garbage collecting instances of our class that are not needed. 

Here is the updated version of MiddlewareMetrics
```cs

public class MetricsMiddleware : IMiddleware
{

    private long _counter = 0; // No longer needs to be static

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

Now as seen from our comment, the _counter no longer needs to be static.

## Ok, Ok, but what about GOLANG!!!???
So, the golang solution is much easier to implement, but it does have its own issues with coupling. Lets view it!
```go
    
    // Main ...

	apiCfg := apiConfig{
		fileserverHits: atomic.Int32{},
	}
    
    // Handlers

	mux.HandleFunc("POST /admin/reset", apiCfg.handlerReset) // Here we add the /admin
	mux.HandleFunc("GET /admin/metrics", apiCfg.handlerMetrics) // Here we add the /admin
```

As you can see we haven't added a class like in C#, but that is ok, we have a string afterall that will handle the route to admin. So much less abstractions here, and far simpler. Albiet, a bit more clutter in fariness.

What of the Handlers now? What does `apiCfg.handlerReset` and `apiCfg.handlerMetrics` look like?

```go
    func (cfg *apiConfig) handlerMetrics(w http.ResponseWriter, r *http.Request) {
        w.Header().Add("Content-Type", "text/html")
        w.WriteHeader(http.StatusOK)
        w.Write([]byte(fmt.Sprintf(`
    <html>

    <body>
        <h1>Welcome, Chirpy Admin</h1>
        <p>Chirpy has been visited %d times!</p>
    </body>

    </html>
        `, cfg.fileserverHits.Load())))
    }

    func (cfg *apiConfig) handlerReset(w http.ResponseWriter, r *http.Request) {
        cfg.fileserverHits.Store(0)
        w.WriteHeader(http.StatusOK)
        w.Write([]byte("Hits reset to 0"))
    }
```

Well pretty much as we expect, the is now a function reciever that attaches the apiConfig struct to these functions, meaning it is via the apiConfig that we can call them, and well, for our metrics, we supply html to the pathway by a html string.

Reset, is again pretty straight forward just responding with a status of 200 and setting the fileserverHits, which would be out count from out singleton to 0.

## Conclusions
So, it is obvious to me the more we code more complex things that OOP, has the need to abstract all that more rather than golang. The idea that came to mind to seperate for example, admin versus api came simply from wanting to make sure that admins have their own logic and api has their own logic as they are both implementations of different things. Golang has that seperation, but does not require whole classes to do that, in fact having a large apiconfig allows for both handlers to be handled given the string path of the routes. 

This is probably the point where I say, I think I perfer the OOP method here. To my understanding Admin is a thing, or seperate entity, and should only have methods applying to its own logic. But I also see the simplicity in goes structure, where that is also seperate but not as distinct. 
