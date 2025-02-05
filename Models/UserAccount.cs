using System.Text.Json.Serialization;

public class UserAccount
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("password")]
    public string? Password { get; set; }
    [JsonPropertyName("expiresinseconds")]
    public TimeSpan? ExpiresInSeconds { get; set; }
}
