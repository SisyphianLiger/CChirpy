public class MetricsMiddleware : IMiddleware
{

    private long _counter = 0;

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
