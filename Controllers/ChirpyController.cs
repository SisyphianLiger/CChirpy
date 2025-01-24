using Microsoft.AspNetCore.Mvc;


// Here we are defining out Handlers
namespace Chirpy.Controllers;

[ApiController]
[Route("api")]
public class ChirpyController : ControllerBase
{

    private readonly IChirpValidator _validator;

    public ChirpyController(IChirpValidator validator)
    {
        _validator = validator;
    }


    [HttpGet("healthz")]
    public ActionResult<string> HealthCheck()
    {
        return Content("OK", "text/plain; charset=utf-8");
    }

    [HttpPost("validate_chirp")]
    public ActionResult ValidateChirp([FromBody] Chirp? request) =>
        request switch {
                { Body: null }                                          => BadRequest( new JsonError { Error = "Body is required" }),
                { Body: var paylod }  when !_validator.isValid(paylod)  => BadRequest( new JsonError { Error = "Chirp is too long" }),
                _                                                       =>  Ok(new ChirpResponse { CleanedBody = _validator.cleanedBody(request?.Body) }),
        };

}

