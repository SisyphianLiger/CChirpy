using System.Text.Json.Serialization;

public class Chirp
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}
