using System.Text.Json.Serialization;

namespace VideoIndexer.Model;

public class AccountProperties
{
    [JsonPropertyName("accountId")]
    public string Id { get; set; }
}