using Microsoft.AspNetCore.Mvc;
using PostgresDB;

namespace Chirp.Controller;

[ApiController]
[Route("api/chirps")]
public class ChirpApiController : ControllerBase
{
    private readonly IChirpValidator _validator;
    private readonly ChirpyDatabase _chirpydb;

    public ChirpApiController(IChirpValidator validator, ChirpyDatabase chirpydb)
    {
        _validator = validator;
        _chirpydb = chirpydb;
    }

    [HttpPost]
    public async Task<IActionResult> CreateChirp([FromBody] ChirpRequest chirp)
    {
        if (chirp.Body == null)
        {
            return await Task.FromResult(StatusCode(StatusCodes.Status400BadRequest, new JsonError { Error = "Invalid Body, cannot send chirp" }));
        }
        if (!_validator.isValid(chirp.Body))
        {
            return await Task.FromResult(StatusCode(StatusCodes.Status400BadRequest, new JsonError { Error = "Chirp is too long" }));
        }

        var chirpIt = await _chirpydb.CreateChirp(_validator.cleanedBody(chirp.Body), chirp.Id);

        return await Task.FromResult(StatusCode(StatusCodes.Status201Created, new ChirpResponse { Id = chirp.Id, Body = _validator.cleanedBody(chirp.Body) }));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllChirpsFromUser()
    {
        var result = await _chirpydb.GetAllChirps();
        if (result.Count == 0)
        {
            return await Task.FromResult(StatusCode(StatusCodes.Status500InternalServerError, new JsonError { Error = "No Chirps have been found" }));
        }
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

}
