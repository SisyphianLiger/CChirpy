namespace Auth;

public static class PasswordGenerator {

    public static string HashPassword(string password) {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        
        return hashedPassword;
    }
    public static bool CheckPassword(string password, string hash) {
        var check = BCrypt.Net.BCrypt.Verify(password, hash);
        return check;
    }

    public static string? GetPolkaKey(IHeaderDictionary headers)
    {
        var header = headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(header) && header.StartsWith("ApiKey ")) {
            return header.ToString().Substring(7);
        }
        return null;
    }
}
