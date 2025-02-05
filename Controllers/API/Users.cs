using Microsoft.AspNetCore.Mvc;
using Auth;
using PostgresDB;
using Utilities;

namespace Users.Controllers;

[ApiController]
[Route("api")]
public class UsersController : ControllerBase
{

    private readonly ChirpyDatabase _chirpydb;
    private readonly ConfigurationAccess _cfg;

    public UsersController(IChirpValidator validator, ChirpyDatabase chirpydb, ConfigurationAccess cfg)
    {
        _chirpydb = chirpydb;
        _cfg = cfg;
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] UserAccount newAccount)
    {

        if (newAccount.Email == null) 
        {
            return StatusCode(StatusCodes.Status400BadRequest, new JsonError { Error = "Invalid Email" });
        }
        if (newAccount.Password == null) 
        {
            return StatusCode(StatusCodes.Status400BadRequest, new JsonError { Error = "Invalid Password" });
        }

        var hashedPw = PasswordGenerator.HashPassword(newAccount.Password);
         
        var user = await _chirpydb.CreateUserAsync(newAccount.Email, hashedPw);

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
            IsChirpyRed = user.isChirpyRed
            
        };

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] UserAccount loginUser) {
        if (loginUser.Email == null) 
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Invalid Email" });
        }
        if (loginUser.Password == null) 
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Invalid Password" });
        }

        TimeSpan accessTokenExpiration = TimeSpan.FromHours(1);
        DateTime refreshTokenExpiration = DateTime.UtcNow.AddDays(60);

    var userCredentials = await _chirpydb.LoginUser(loginUser.Email, loginUser.Password);
        if (userCredentials == null) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Invalid credentials" });
        }

        // Generate tokens
        userCredentials.Token = JwTClass.GenerateJwT(userCredentials.Id, _cfg.JwtSecret, accessTokenExpiration);
        userCredentials.RefreshToken = JwTClass.MakeRefreshToken();

        await _chirpydb.CreateRefreshToken(
            userCredentials.Id, 
            userCredentials.RefreshToken, 
            refreshTokenExpiration
        );

        return StatusCode(StatusCodes.Status200OK, userCredentials);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshUserToken() {
        var token = JwTClass.GetBearerToken(Request);
        if (token == null) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "No Token In Header" });
        }
        
        // Find the Token
        var dbResult = await _chirpydb.FindToken(token);
       
        if (dbResult == null) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Token is Expired" });
        }

        var newAccessToken = JwTClass.GenerateJwT(dbResult.UserId, _cfg.JwtSecret, TimeSpan.FromHours(1));
        return StatusCode(StatusCodes.Status200OK, new TokenResponse { Token = newAccessToken});
    }
    
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeUserToken() {
        var token = JwTClass.GetBearerToken(Request);
        if (token == null) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "No Token In Header" });
        }

        var dbResult = await _chirpydb.RevokeToken(token);

        if (!dbResult) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Token Not Found" });
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPut("users")]
    public async Task<IActionResult> UpdateEmailAndPassword([FromBody] EmailAndPassword userInfo) {

        var accessToken = JwTClass.GetBearerToken(Request);
        if (accessToken == null) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Token Not Found" });
        }

        Guid? validateJwT = JwTClass.ValidateJwT(accessToken, _cfg.JwtSecret);
        if (!validateJwT.HasValue) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Invalid JwT" });
        }

        if (userInfo.Password == null || userInfo.Email == null) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "Incorrect Body" });
        }

        var newPw = PasswordGenerator.HashPassword(userInfo.Password);
        var dbResult = await _chirpydb.UpdateUserInfo(userInfo.Email, newPw, validateJwT.Value);
        if (dbResult == null) {
            return StatusCode(StatusCodes.Status401Unauthorized, new JsonError { Error = "No User Found" });
        }

        return StatusCode(StatusCodes.Status200OK, new UserResponse { Email = userInfo.Email, });
    }

}
