using Microsoft.AspNetCore.Mvc;
using PostgresDB;

namespace Users.Controllers;

[ApiController]
[Route("api")]
public class UsersController : ControllerBase
{

    private readonly ChirpyDatabase _chirpydb;

    public UsersController(IChirpValidator validator, ChirpyDatabase chirpydb)
    {
        _chirpydb = chirpydb;
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserAccount newAccount)
    {

        if (newAccount.Email == null)
        {
            return StatusCode(StatusCodes.Status400BadRequest, new JsonError { Error = "Invalid Email Format" });
        }
        var user = await _chirpydb.CreateUserAsync(newAccount.Email);

        if (user == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new JsonError { Error = "Duplicate Email found, please sign up with another email Email" });
        }

        var response = new UserResponse
        {
            Id = user.Id,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Email = user.Email,
        };

        return StatusCode(StatusCodes.Status201Created, response);
    }
}
