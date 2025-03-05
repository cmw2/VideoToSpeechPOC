using System.Text.Json.Serialization;

namespace VideoIndexer.Model
{
    public class VideoSummary
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string VideoId { get; set; }
        public int State { get; set; }
        public string ModelName { get; set; }
        public int SummaryStyle { get; set; }
        public int SummaryLength { get; set; }
        public int IncludedFrames { get; set; }
        public string CreateTime { get; set; }
        public string LastUpdateTime { get; set; }
        public string FailureMessage { get; set; }
        public int Progress { get; set; }
        public string DeploymentName { get; set; }
        public string Disclaimer { get; set; }
    }

    public class VideoSummaryResponse
    {
        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("items")]
        public List<VideoSummaryContent> Items { get; set; } = new();

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }

    public class VideoSummaryContent
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("disclaimer")]
        public string Disclaimer { get; set; }

        [JsonPropertyName("sensitiveContentPercent")]
        public double SensitiveContentPercent { get; set; }

        [JsonPropertyName("deploymentName")]
        public string DeploymentName { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        [JsonPropertyName("videoId")]
        public string VideoId { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("modelName")]
        public string ModelName { get; set; }

        [JsonPropertyName("summaryStyle")]
        public string SummaryStyle { get; set; }

        [JsonPropertyName("summaryLength")]
        public string SummaryLength { get; set; }

        [JsonPropertyName("includedFrames")]
        public string IncludedFrames { get; set; }

        [JsonPropertyName("createTime")]
        public string CreateTime { get; set; }

        [JsonPropertyName("lastUpdateTime")]
        public string LastUpdateTime { get; set; }

        [JsonPropertyName("failureMessage")]
        public string FailureMessage { get; set; }

        [JsonPropertyName("progress")]
        public int Progress { get; set; }
    }
}
