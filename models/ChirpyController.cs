using Microsoft.AspNetCore.Mvc;


// Here we are defining out Handlers
namespace Chirpy.Controllers
{
    [ApiController]
    [Route("/")]
    public class ChirpyController : ControllerBase

    {

        [Route("/healthz")]
        public ActionResult<string> HealthCheck()
        {
            return Content("OK", "text/plain; charset=utf-8");
        }


        // [HttpPost]
        // public ActionResult<string> Post([FromBody] string value)
        // {
        //     return $"You Posted: {value}";
        // }
        //
        // [HttpPut("{id}")]
        // public ActionResult<string> Put(int id, [FromBody] string value)
        // {
        //     return $"You updated the value of item {id} to: {value}";
        // }
        //
        // [HttpDelete]
        //
        // public ActionResult<string> Delete(int id, [FromBody] string value)
        // {
        //     return $"You deleted item {id}";
        // }

    }
}
