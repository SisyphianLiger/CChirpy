using Microsoft.AspNetCore.Mvc;


using PostgresDB;


// Here we are defining out Handlers
namespace Admin.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly ChirpyDatabase _chirpydb;
    private readonly MetricsMiddleware _metrics;

    public AdminController(MetricsMiddleware metrics, ChirpyDatabase chirpydb)
    {
        _chirpydb = chirpydb;
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
    public async Task<IActionResult> ResetMetric()
    {
        var DevMode = _chirpydb.DevDB;
        return (DevMode, await _chirpydb.DeleteAllUsers()) switch
        {
            (false, _) => StatusCode(StatusCodes.Status403Forbidden),
            (true, true) => StatusCode(StatusCodes.Status200OK),
            (true, false) => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

}
