using Microsoft.AspNetCore.Mvc;


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
    public IActionResult GetMetrics()
    {
        return Ok($"Hits: {_metrics.GetCurrentCount()}");
    }

    [HttpPost("reset")]
    public IActionResult ResetMetrics()
    {
        _metrics.ResetCounter();
        return Ok();
    }
}
