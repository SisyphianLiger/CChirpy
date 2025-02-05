using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace Auth;

public static class JwTClass {

    public static string GenerateJwT(Guid userId, string tokenSecret, TimeSpan expiresIn) {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
                issuer: "chirpy",
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(expiresIn),
                signingCredentials: creds
                );

        return new JwtSecurityTokenHandler().WriteToken(token);

    }

public static Guid? ValidateJwT(string tokenString, string tokenSecret)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));

    try 
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "chirpy",
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "sub",  
            ValidateActor = false
        };

        var claimsPrincipal = tokenHandler.ValidateToken(tokenString, tokenValidationParameters, out var validatedToken);

        var userIdClaim = claimsPrincipal.FindFirst("sub");
        if (userIdClaim == null)
        {
            var jwt = validatedToken as JwtSecurityToken;
            userIdClaim = jwt?.Claims.FirstOrDefault(c => c.Type == "sub");
        }

        if (userIdClaim?.Value == null || !Guid.TryParse(userIdClaim.Value, out Guid result)) 
        {
            return null;
        }

        return result;
    }
    catch (Exception)
    {
        return null;
    }
}

    public static string? GetBearerToken(HttpRequest request) {
        var header = request.Headers["Authorization"].ToString();        
        if (!string.IsNullOrEmpty(header) && header.StartsWith("Bearer ")) {
            return header.ToString().Substring(7);
        }
        return null;
    }


    public static string MakeRefreshToken() {
        byte[] randomBytes = new byte[32];
        RandomNumberGenerator.Fill(randomBytes);
       return Convert.ToHexString(randomBytes);
    }



}
