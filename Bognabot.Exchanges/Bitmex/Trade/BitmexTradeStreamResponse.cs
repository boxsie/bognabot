using System;
using Bognabot.Exchanges.Core;
using Newtonsoft.Json;

namespace Bognabot.Exchanges.Bitmex.Trade
{
    public class BitmexTradeStreamResponse : StreamResponse
    {
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("tickDirection")]
        public string TickDirection { get; set; }

        [JsonProperty("trdMatchID")]
        public Guid TrdMatchId { get; set; }

        [JsonProperty("grossValue")]
        public long GrossValue { get; set; }

        [JsonProperty("homeNotional")]
        public double HomeNotional { get; set; }

        [JsonProperty("foreignNotional")]
        public long ForeignNotional { get; set; }
    }
}
