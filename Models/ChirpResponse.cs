using System.Text.Json.Serialization;

public class ChirpResponse
{

    [JsonPropertyName("cleaned_body")]
    public string? CleanedBody { get; set; }

}
