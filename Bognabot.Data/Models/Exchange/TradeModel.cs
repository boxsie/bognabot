using System;
using System.Collections.Generic;
using System.Text;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Data.Models.Exchange
{
    public class TradeModel
    {
        public InstrumentType Instrument { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TradeType Side { get; set; }
        public long Size { get; set; }
        public double Price { get; set; }
    }
}
