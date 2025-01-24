using System.Text.Json.Serialization;

public class JsonError
{

    [JsonPropertyName("error")]
    public string? Error { get; set; }

}
