using System;
using Bognabot.Exchanges.Core;
using Newtonsoft.Json;

namespace Bognabot.Exchanges.Bitmex.Trade
{
    public class BitmexTradeCommandResponse : CommandResponse
    {
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

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