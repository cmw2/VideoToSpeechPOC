using System.Text.Json.Serialization;

namespace VideoIndexer.Model;

public class Video
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("partition")]
    public string Partition { get; set; }

    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; }

    [JsonPropertyName("metadata")]
    public string Metadata { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; }

    [JsonPropertyName("lastIndexed")]
    public DateTime LastIndexed { get; set; }

    [JsonPropertyName("privacyMode")]
    public string PrivacyMode { get; set; }

    [JsonPropertyName("userName")]
    public string UserName { get; set; }

    [JsonPropertyName("isOwned")]
    public bool IsOwned { get; set; }

    [JsonPropertyName("isBase")]
    public bool IsBase { get; set; }

    [JsonPropertyName("hasSourceVideoFile")]
    public bool HasSourceVideoFile { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("moderationState")]
    public string ModerationState { get; set; }

    [JsonPropertyName("reviewState")]
    public string ReviewState { get; set; }

    [JsonPropertyName("isSearchable")]
    public bool IsSearchable { get; set; }

    [JsonPropertyName("processingProgress")]
    public string ProcessingProgress { get; set; }

    [JsonPropertyName("durationInSeconds")]
    public double DurationInSeconds { get; set; }

    [JsonPropertyName("thumbnailVideoId")]
    public string ThumbnailVideoId { get; set; }

    [JsonPropertyName("thumbnailId")]
    public string ThumbnailId { get; set; }

    [JsonPropertyName("searchMatches")]
    public List<object> SearchMatches { get; set; }

    [JsonPropertyName("indexingPreset")]
    public string IndexingPreset { get; set; }

    [JsonPropertyName("streamingPreset")]
    public string StreamingPreset { get; set; }

    [JsonPropertyName("sourceLanguage")]
    public string SourceLanguage { get; set; }

    [JsonPropertyName("sourceLanguages")]
    public List<string> SourceLanguages { get; set; }

    [JsonPropertyName("personModelId")]
    public string PersonModelId { get; set; }
}
