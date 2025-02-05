using System.Text.Json.Serialization;
public class PolkaUpgrade
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }
    [JsonPropertyName("data")]
    public PolkaData? Data { get; set; }
}

public class PolkaData {
    [JsonPropertyName("user_id")]
    public Guid? UserID { get; set;}
}
