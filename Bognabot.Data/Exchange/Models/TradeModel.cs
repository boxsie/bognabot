using System;
using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Data.Exchange.Models
{
    public class TradeModel
    {
        public Instrument Instrument { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TradeType Side { get; set; }
        public long Size { get; set; }
        public double Price { get; set; }
    }
}
