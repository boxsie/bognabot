using System;
using Bognabot.Exchanges.Core;
using Newtonsoft.Json;

namespace Bognabot.Exchanges.Bitmex.Trade
{
    public class BitmexTradeCommandRequest : CommandRequest
    {
        [JsonProperty("binSize")]
        public string TimeInterval { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("count")]
        public double Count { get; set; }

        [JsonProperty("start")]
        public double StartAt { get; set; }

        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("endTime")]
        public string EndTime { get; set; }
    }
}