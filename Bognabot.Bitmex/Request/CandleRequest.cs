using Bognabot.Data.Exchange;
using Newtonsoft.Json;

namespace Bognabot.Bitmex.Request
{
    public class CandleRequest : ICollectionRequest
    {
        [JsonProperty("binSize")]
        public string TimeInterval { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("endTime")]
        public string EndTime { get; set; }

        [JsonProperty("count")]
        public double Count { get; set; }

        [JsonProperty("start")]
        public double StartAt { get; set; }
    }
}