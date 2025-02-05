using Microsoft.AspNetCore.Mvc;
using PostgresDB;
using Utilities;
using Auth;
namespace Chirp.Controller;

[ApiController]
[Route("api/chirps")]
public class ChirpApiController : ControllerBase
{
    private readonly IChirpValidator _validator;
    private readonly ChirpyDatabase _chirpydb;
    private readonly ConfigurationAccess _cfg;

    public ChirpApiController(IChirpValidator validator, ChirpyDatabase chirpydb, ConfigurationAccess cfg)
    {
        _validator = validator;
        _chirpydb = chirpydb;
        _cfg = cfg;
    }

    [HttpPost]
    public async Task<IActionResult> CreateChirp([FromBody] ChirpRequest chirp)
    {

        var token = JwTClass.GetBearerToken(Request);
        if (token == null) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "No token provided" });
        }


        Guid? userId = JwTClass.ValidateJwT(token, _cfg.JwtSecret);
        if (!userId.HasValue) {
             return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Invalid token" });
        }

        if (string.IsNullOrEmpty(chirp.Body))
        {
            return await Task.FromResult(StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Invalid Body, cannot send chirp" }));
        }
        if (!_validator.isValid(chirp.Body))
        {
            return await Task.FromResult(StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Chirp is too long" }));
        }
        
        var chirpIt = await _chirpydb.CreateChirp(_validator.cleanedBody(chirp.Body), userId.Value);

        if (chirpIt == null) 
        {
            return await Task.FromResult(StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Chirp Not Found" }));
        }

        return await Task.FromResult(StatusCode(StatusCodes.Status201Created, new ChirpResponse { Id = chirpIt.Id, Body = _validator.cleanedBody(chirp.Body), UserId = userId.Value }));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllChirpsFromUser(  [FromQuery(Name = "author_id")] string? authorID, [FromQuery] string sort = "asc")
    {
        List<ChirpResponse> result;
        if (authorID == null) {
            result = await _chirpydb.GetAllChirps(sort);
            if (result.Count == 0)
            {
                return await Task.FromResult(StatusCode(StatusCodes.Status500InternalServerError, new JsonError { Error = "No Chirps have been found" }));
            }
            return await Task.FromResult(StatusCode(StatusCodes.Status200OK, result));
        }
        
        result = await _chirpydb.GetAllChirps(sort, authorID);
        return await Task.FromResult(StatusCode(StatusCodes.Status200OK, result));

    }
    [HttpGet("{chirpID}")]
    public async Task<IActionResult> GetAChirpFromUser(Guid chirpID)
    {
        var result = await _chirpydb.GetAChirp(chirpID);

        if (result == null) 
            return await Task.FromResult(StatusCode(StatusCodes.Status404NotFound, new JsonError { Error = "Chirp not found"}));

        return await Task.FromResult(StatusCode(StatusCodes.Status200OK, result));

    }

    [HttpDelete("{chirpID}")]
    public async Task<IActionResult> DeleteAChirp(Guid chirpID) {

        var accessToken = JwTClass.GetBearerToken(Request);
        if (accessToken == null) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Token Not Found" });
        }

        Guid? validateJwT = JwTClass.ValidateJwT(accessToken, _cfg.JwtSecret);
        if (!validateJwT.HasValue) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Invalid JwT" });
        }

        var chirp = await _chirpydb.GetAChirp(chirpID);
        if (chirp == null) {
            return await Task.FromResult(StatusCode(StatusCodes.Status404NotFound, new JsonError { Error = "Chirp not found"}));
        }
        if (chirp.UserId != validateJwT.Value) {
            return await Task.FromResult(StatusCode(StatusCodes.Status403Forbidden, new JsonError { Error = "Not Authorized to delete this chirp"}));
        }
        System.Console.WriteLine(chirpID);
        await _chirpydb.DeleteAChirp(chirpID);

        return await Task.FromResult(StatusCode(StatusCodes.Status204NoContent));
    }

}
