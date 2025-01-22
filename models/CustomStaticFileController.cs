using Microsoft.AspNetCore.Mvc;


// Here we are defining out Handlers
namespace StaticFile.Controllers
{
    [ApiController]
    [Route("/")]
    public class StaticFileController : ControllerBase
    {
        [HttpGet("app/assets")]
        public ActionResult<string> GetChirpyAsHtml()
        {
            var body = "<pre>\n<a href=\"logo.png\">logo.png</a>\n</pre>";
            return Content(body, "text/html");
        }
    }
}
