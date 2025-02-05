
using System.Text.Json.Serialization;

public class RefreshToken {
    
    [JsonPropertyName("token")]
    public string? Token { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }
    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }
    [JsonPropertyName("revoked_at")]
    public DateTime? RevokedAt { get; set; }

}


