using Microsoft.AspNetCore.Mvc;
using Auth;
using PostgresDB;
using Utilities;

namespace Users.Controllers;

[ApiController]
[Route("api/polka")]
public class PolkaController : ControllerBase
{

    private readonly ChirpyDatabase _chirpydb;
    private readonly ConfigurationAccess _cfg;

    public PolkaController(IChirpValidator validator, ChirpyDatabase chirpydb, ConfigurationAccess cfg)
    {
        _chirpydb = chirpydb;
        _cfg = cfg;
    }

    [HttpPost("webhooks")]
    public async Task<IActionResult> UpgradeUser([FromBody] PolkaUpgrade PUser) {
        if (PUser == null || PUser.Data == null || PUser.Event == null || PUser.Data.UserID == null) {
            return StatusCode(StatusCodes.Status404NotFound);
        }
        // Check API_Key
        if (PasswordGenerator.GetPolkaKey(Request.Headers) == null) {
            return StatusCode(StatusCodes.Status401Unauthorized);
        }

        if(!string.Equals(PUser.Event, "user.upgraded")){
            return StatusCode(StatusCodes.Status204NoContent);
        }


        var dbResult = await _chirpydb.UpgradeToChirpyRed(PUser.Data.UserID.Value);

        if (dbResult) {
                return StatusCode(StatusCodes.Status204NoContent);
        }

        return StatusCode(StatusCodes.Status404NotFound);



    }
}
