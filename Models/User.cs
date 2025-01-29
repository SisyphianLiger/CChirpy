using System.Text.Json.Serialization;

namespace PostgresDB;


public class User
{

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    public User() { }

}
