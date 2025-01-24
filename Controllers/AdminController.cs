using Microsoft.AspNetCore.Mvc;


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
