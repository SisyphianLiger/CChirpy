using System.Text.Json.Serialization;
public class ChirpRequest
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}

