using System.Text.Json.Serialization;
public class ChirpRequest
{

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("user_id")]
    public Guid Id { get; set; }
}

