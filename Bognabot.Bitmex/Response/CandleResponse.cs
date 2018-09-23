using System;
using Bognabot.Data.Exchange;
using Newtonsoft.Json;

namespace Bognabot.Bitmex.Response
{
    public class CandleResponse
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("open")]
        public double Open { get; set; }

        [JsonProperty("high")]
        public double High { get; set; }

        [JsonProperty("low")]
        public double Low { get; set; }

        [JsonProperty("close")]
        public double Close { get; set; }

        [JsonProperty("trades")]
        public long Trades { get; set; }

        [JsonProperty("volume")]
        public long Volume { get; set; }

        [JsonProperty("vwap")]
        public object Vwap { get; set; }

        [JsonProperty("lastSize")]
        public object LastSize { get; set; }

        [JsonProperty("turnover")]
        public long Turnover { get; set; }

        [JsonProperty("homeNotional")]
        public long HomeNotional { get; set; }

        [JsonProperty("foreignNotional")]
        public long ForeignNotional { get; set; }
    }
}