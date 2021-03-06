﻿using System;
using Bognabot.Data.Exchange;
using Newtonsoft.Json;

namespace Bognabot.Bitmex.Response
{
    public class BookResponse : IResponse
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }
    }
}