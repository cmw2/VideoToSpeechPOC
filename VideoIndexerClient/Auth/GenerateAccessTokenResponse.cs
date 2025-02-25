using System.Text.Json.Serialization;

namespace VideoIndexer.Auth
{
    public class GenerateAccessTokenResponse
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
    }
}
