using Microsoft.AspNetCore.Mvc;


// Here we are defining out Handlers
namespace StaticFile.Controllers;

[ApiController]
[Route("app")]
public class StaticFileController : ControllerBase
{
    [HttpGet("assets")]
    public ActionResult<string> GetChirpyAsHtml()
    {

        var body = "<a href=\"logo.png\">logo.png</a>";
        return Content(body, "text/html");

    }
    [HttpGet]
    public ActionResult GetChirpyHomePage()
    {
        // Get the path to wwwroot/index.html
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
        return PhysicalFile(filePath, "text/html");
    }
}

