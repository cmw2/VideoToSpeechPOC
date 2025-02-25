using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VideoIndexer.Model
{
    public class VideoResponse
    {
        [JsonPropertyName("results")]
        public List<Video> Results { get; set; }

        [JsonPropertyName("nextPage")]
        public NextPage NextPage { get; set; }
    }

    public class NextPage
    {
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("skip")]
        public int Skip { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }
}
