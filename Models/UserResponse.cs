using System.Text.Json.Serialization;

public class UserResponse
{

    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("email")]
    public string? Email { get; set; }

}
