using Microsoft.AspNetCore.Mvc;


// Here we are defining out Handlers
namespace Chirpy.Controllers;

[ApiController]
[Route("api")]
public class ChirpyController : ControllerBase
{

    [HttpGet("healthz")]
    public ActionResult<string> HealthCheck()
    {
        return Content("OK", "text/plain; charset=utf-8");
    }
}
